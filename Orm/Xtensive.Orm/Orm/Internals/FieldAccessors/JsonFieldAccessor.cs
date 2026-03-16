// Copyright (C) 2024-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text.Json;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class JsonFieldAccessor<T> : CachingFieldAccessor<T>
  {
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = null,
      WriteIndented = false,
    };

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var field = Field;
      var valueAdapter = (JsonFieldValueAdapter<T>) obj.GetFieldValueAdapter(field, Constructor);
      return valueAdapter.Value;
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
      var adapter = obj.TryGetFieldValueAdapter(Field) as JsonFieldValueAdapter<T>;
      if (adapter != null) {
        adapter.UpdateValue(value, jsonString);
      }
    }

    // Type initializer

    static JsonFieldAccessor()
    {
      Constructor = (owner, field) => new JsonFieldValueAdapter<T>(owner, field);
    }
  }
}
