// Copyright (C) 2024-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Interface for JSON field value adapters that support snapshot-based dirty checking.
  /// Implemented by both <see cref="JsonFieldValueAdapter{T}"/> (scalar JSON fields)
  /// and <see cref="JsonTypeArray{TItem}"/> (JSON array fields).
  /// </summary>
  internal interface IJsonFieldValueAdapter
  {
    /// <summary>
    /// Re-serializes the cached value and, if it differs from the original snapshot,
    /// writes the updated JSON back to the owner's tuple.
    /// Returns <see langword="true"/> if a change was detected and persisted.
    /// </summary>
    bool FlushIfDirty();
  }

  /// <summary>
  /// Cached wrapper for a scalar <see cref="JsonType"/> field value.
  /// Implements <see cref="IFieldValueAdapter"/> so it participates in the
  /// <see cref="Persistent.GetFieldValueAdapter"/> caching mechanism,
  /// and <see cref="IJsonFieldValueAdapter"/> for snapshot-based dirty checking
  /// before session persist.
  /// </summary>
  /// <typeparam name="T">The concrete <see cref="JsonType"/> subclass.</typeparam>
  internal sealed class JsonFieldValueAdapter<T> : IFieldValueAdapter, IJsonFieldValueAdapter
  {
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = null,
      WriteIndented = false,
    };

    private readonly Persistent owner;
    private readonly FieldInfo field;
    private T cachedValue;
    private string originalJson;

    /// <inheritdoc/>
    Persistent IFieldValueAdapter.Owner => owner;

    /// <inheritdoc/>
    FieldInfo IFieldValueAdapter.Field => field;

    /// <summary>
    /// Gets the cached deserialized value. This is the same instance
    /// returned to user code, so in-place mutations are visible here.
    /// </summary>
    public T Value => cachedValue;

    /// <summary>
    /// Updates the cached value and snapshot when a whole-object replacement
    /// occurs via <c>SetFieldValue</c>. Keeps the cache in sync.
    /// </summary>
    internal void UpdateValue(T newValue, string newJson)
    {
      cachedValue = newValue;
      originalJson = newJson;
    }

    /// <inheritdoc/>
    public bool FlushIfDirty()
    {
      if (cachedValue == null) {
        // If the cached value is null and original was also null/empty, nothing to do
        if (string.IsNullOrEmpty(originalJson))
          return false;
        // Value was set to null via direct field mutation (unlikely for reference type, but handle it)
        return false;
      }

      var currentJson = JsonSerializer.Serialize(cachedValue, typeof(T), SerializerOptions);
      if (string.Equals(currentJson, originalJson, StringComparison.Ordinal))
        return false;

      // Inner properties were mutated — write the updated JSON to the tuple
      var fieldIndex = field.MappingInfo.Offset;
      owner.SystemBeforeTupleChange();
      owner.Tuple.SetValue(fieldIndex, currentJson);
      owner.SystemTupleChange();

      // Update snapshot so subsequent FlushIfDirty calls don't re-detect the same change
      originalJson = currentJson;
      return true;
    }

    // Constructor

    internal JsonFieldValueAdapter(Persistent owner, FieldInfo field)
    {
      this.owner = owner;
      this.field = field;

      // Deserialize the initial value from the tuple and take a snapshot
      var fieldIndex = field.MappingInfo.Offset;
      var jsonString = owner.Tuple.GetValueOrDefault<string>(fieldIndex);
      originalJson = jsonString;

      if (!string.IsNullOrEmpty(jsonString)) {
        cachedValue = JsonSerializer.Deserialize<T>(jsonString, SerializerOptions);
      }

      // Register with the session for dirty-checking before persist
      if (owner is Entity entity && entity.Session != null) {
        entity.Session.RegisterJsonFieldAdapter(this);
      }
    }
  }
}
