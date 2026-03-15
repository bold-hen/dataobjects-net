// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// A tracked collection wrapper for JSON fields that automatically
  /// persists changes back to the entity's tuple when the collection is mutated.
  /// </summary>
  internal class JsonTrackedCollection<T> : ICollection<T>
  {
    private readonly List<T> innerList;
    private readonly Persistent owner;
    private readonly FieldInfo field;

    public int Count => innerList.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
      innerList.Add(item);
      PersistChanges();
    }

    public bool Remove(T item)
    {
      var result = innerList.Remove(item);
      if (result)
        PersistChanges();
      return result;
    }

    public void Clear()
    {
      innerList.Clear();
      PersistChanges();
    }

    public bool Contains(T item) => innerList.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => innerList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void PersistChanges()
    {
      // Write the updated collection back through the normal SetFieldValue path
      // which will serialize it to JSON and update the tuple + change tracking
      owner.SetFieldValue(field, (object) this);
    }

    internal JsonTrackedCollection(List<T> items, Persistent owner, FieldInfo field)
    {
      this.innerList = items ?? new List<T>();
      this.owner = owner;
      this.field = field;
    }
  }
}
