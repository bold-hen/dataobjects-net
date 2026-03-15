// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Orm.Internals.FieldAccessors
{
  internal class JsonArrayFieldAccessor<T> : CachingFieldAccessor<T>
  {
    /// <inheritdoc/>
    public override bool AreSameValues(object oldValue, object newValue)
    {
      return ReferenceEquals(oldValue, newValue);
    }

    /// <inheritdoc/>
    public override void SetValue(Persistent obj, T value)
    {
      // JsonTypeArray is mutated in-place (like EntitySet).
      // Full replacement is not supported; use Add/Remove/Clear.
      throw new InvalidOperationException(
        "JsonTypeArray collection cannot be reassigned. Use Add, Remove, or Clear to modify its contents.");
    }

    // Type initializer

    static JsonArrayFieldAccessor()
    {
      Constructor = (owner, field) => {
        // Create JsonTypeArray<TElement> instance
        var elementType = field.ValueType.GetGenericArguments()[0];
        var arrayType = typeof(JsonTypeArray<>).MakeGenericType(elementType);
        return (IFieldValueAdapter) System.Activator.CreateInstance(
          arrayType,
          System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
          null,
          new object[] { owner, field },
          null);
      };
    }
  }
}
