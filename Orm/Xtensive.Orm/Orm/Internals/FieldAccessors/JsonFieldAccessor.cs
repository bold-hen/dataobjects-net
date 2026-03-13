// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text.Json;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class JsonFieldAccessor<T> : FieldAccessor<T>
  {
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = null,
      WriteIndented = false,
    };

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
      return string.Equals(oldJson, newJson, System.StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override T GetValue(Persistent obj)
    {
      var fieldIndex = Field.MappingInfo.Offset;
      var tuple = obj.Tuple;
      var jsonString = tuple.GetValueOrDefault<string>(fieldIndex);
      if (string.IsNullOrEmpty(jsonString))
        return default;
      return JsonSerializer.Deserialize<T>(jsonString, SerializerOptions);
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, T value)
    {
      var fieldIndex = Field.MappingInfo.Offset;
      if (value == null) {
        obj.Tuple.SetValue(fieldIndex, (string) null);
        return;
      }
      var jsonString = JsonSerializer.Serialize(value, typeof(T), SerializerOptions);
      obj.Tuple.SetValue(fieldIndex, jsonString);
    }
  }
}
