// Copyright (C) 2024-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.ComponentModel;
using System.Text.Json;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class JsonFieldAccessor<T> : FieldAccessor<T>
    where T : JsonType
  {
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = null,
      WriteIndented = false,
    };

    // Per-entity cached instance and subscription tracking
    // Since FieldAccessor is a singleton per field, we track per-entity state
    // through the Persistent's field adapter mechanism.

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var field = Field;
      var adapter = (JsonFieldCacheAdapter<T>) obj.GetFieldValueAdapter(field, JsonFieldCacheAdapter<T>.Factory);
      return adapter.Value;
    }

    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      if (ReferenceEquals(oldValue, newValue))
        return true;
      if (oldValue == null || newValue == null)
        return false;
      // Compare by serialized form
      var oldJson = JsonSerializer.Serialize(oldValue, oldValue.GetType(), SerializerOptions);
      var newJson = JsonSerializer.Serialize(newValue, newValue.GetType(), SerializerOptions);
      return string.Equals(oldJson, newJson, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, T value)
    {
      var fieldIndex = Field.MappingInfo.Offset;
      string jsonString;
      if (value == null) {
        jsonString = null;
        obj.Tuple.SetValue(fieldIndex, (string) null);
      }
      else {
        jsonString = JsonSerializer.Serialize(value, typeof(T), SerializerOptions);
        obj.Tuple.SetValue(fieldIndex, jsonString);
      }

      // Keep the cached adapter in sync if it already exists
      var adapter = obj.TryGetFieldValueAdapter(Field) as JsonFieldCacheAdapter<T>;
      if (adapter != null) {
        adapter.UpdateValue(value);
      }
    }
  }

  /// <summary>
  /// Cached wrapper for a scalar <see cref="JsonType"/> field.
  /// Subscribes to <see cref="INotifyPropertyChanged.PropertyChanged"/> on the cached instance
  /// and writes changes back to the entity's tuple immediately.
  /// </summary>
  internal sealed class JsonFieldCacheAdapter<T> : IFieldValueAdapter
    where T : JsonType
  {
    internal static readonly Func<Persistent, FieldInfo, IFieldValueAdapter> Factory =
      (owner, field) => new JsonFieldCacheAdapter<T>(owner, field);

    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = null,
      WriteIndented = false,
    };

    private readonly Persistent owner;
    private readonly FieldInfo field;
    private T cachedValue;

    Persistent IFieldValueAdapter.Owner => owner;
    FieldInfo IFieldValueAdapter.Field => field;

    /// <summary>
    /// Gets the cached deserialized value.
    /// </summary>
    public T Value => cachedValue;

    /// <summary>
    /// Updates the cached value when a whole-object replacement
    /// occurs via <c>SetFieldValue</c>. Keeps the cache in sync.
    /// </summary>
    internal void UpdateValue(T newValue)
    {
      Unsubscribe(cachedValue);
      cachedValue = newValue;
      Subscribe(newValue);
    }

    private void Subscribe(T value)
    {
      if (value != null) {
        value.PropertyChanged += OnInnerPropertyChanged;
      }
    }

    private void Unsubscribe(T value)
    {
      if (value != null) {
        value.PropertyChanged -= OnInnerPropertyChanged;
      }
    }

    private void OnInnerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      // Re-serialize and write to the entity's tuple
      var fieldIndex = field.MappingInfo.Offset;
      var jsonString = JsonSerializer.Serialize(cachedValue, typeof(T), SerializerOptions);
      owner.SystemBeforeTupleChange();
      owner.Tuple.SetValue(fieldIndex, jsonString);
      owner.SystemTupleChange();
    }

    internal JsonFieldCacheAdapter(Persistent owner, FieldInfo field)
    {
      this.owner = owner;
      this.field = field;

      // Deserialize the initial value from the tuple
      var fieldIndex = field.MappingInfo.Offset;
      var jsonString = owner.Tuple.GetValueOrDefault<string>(fieldIndex);

      if (!string.IsNullOrEmpty(jsonString)) {
        cachedValue = JsonSerializer.Deserialize<T>(jsonString, SerializerOptions);
        Subscribe(cachedValue);
      }
    }
  }
}
