// Copyright (C) 2024-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// A persistent JSON-backed collection for <see cref="JsonType"/> elements.
  /// Similar to <see cref="EntitySet{TItem}"/> but stores items as a JSON array
  /// in a single database column.
  /// </summary>
  /// <typeparam name="TItem">The type of elements, must inherit from <see cref="JsonType"/>.</typeparam>
  /// <remarks>
  /// <para>
  /// Mutations (Add, Remove, Clear) are automatically persisted back to the
  /// entity's underlying tuple, so changes are included in the current transaction.
  /// Inner property mutations on array items (e.g., <c>array[0].Name = "x"</c>)
  /// are detected automatically via snapshot comparison before persist.
  /// </para>
  /// <para>
  /// Declare properties of this type with <c>{ get; private set; }</c>,
  /// as the ORM manages the collection instance.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code>
  /// [HierarchyRoot]
  /// public class Person : Entity
  /// {
  ///   [Field, Key]
  ///   public int Id { get; private set; }
  ///
  ///   [Field]
  ///   public JsonTypeArray&lt;Address&gt; Addresses { get; private set; }
  /// }
  /// </code>
  /// </example>
  public class JsonTypeArray<TItem> : IFieldValueAdapter, IJsonFieldValueAdapter, ICollection<TItem>
    where TItem : JsonType
  {
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions {
      PropertyNamingPolicy = null,
      WriteIndented = false,
    };

    private readonly List<TItem> innerList;
    private readonly Persistent owner;
    private readonly FieldInfo field;
    private string originalJson;

    /// <inheritdoc/>
    Persistent IFieldValueAdapter.Owner => owner;

    /// <inheritdoc/>
    FieldInfo IFieldValueAdapter.Field => field;

    /// <summary>
    /// Gets the owner entity of this collection.
    /// </summary>
    public Persistent Owner => owner;

    /// <summary>
    /// Gets the field this collection is bound to.
    /// </summary>
    public FieldInfo Field => field;

    /// <inheritdoc/>
    public int Count => innerList.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets an element at the specified index.
    /// </summary>
    public TItem this[int index]
    {
      get => innerList[index];
      set {
        innerList[index] = value;
        PersistChanges();
      }
    }

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      innerList.Add(item);
      PersistChanges();
    }

    /// <inheritdoc/>
    public bool Remove(TItem item)
    {
      var result = innerList.Remove(item);
      if (result)
        PersistChanges();
      return result;
    }

    /// <inheritdoc/>
    public void Clear()
    {
      innerList.Clear();
      PersistChanges();
    }

    /// <inheritdoc/>
    public bool Contains(TItem item) => innerList.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(TItem[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator() => innerList.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Re-serializes the collection and, if it differs from the original snapshot,
    /// writes the updated JSON back to the owner's tuple.
    /// This catches inner property mutations on array items.
    /// </summary>
    bool IJsonFieldValueAdapter.FlushIfDirty()
    {
      if (owner == null || field == null)
        return false;

      var currentJson = JsonSerializer.Serialize(innerList, SerializerOptions);
      if (string.Equals(currentJson, originalJson, StringComparison.Ordinal))
        return false;

      // Inner properties of items were mutated — write updated JSON to the tuple
      var fieldIndex = field.MappingInfo.Offset;
      owner.SystemBeforeTupleChange();
      owner.Tuple.SetValue(fieldIndex, currentJson);
      owner.SystemTupleChange();

      originalJson = currentJson;
      return true;
    }

    private void PersistChanges()
    {
      if (owner == null || field == null) {
        // Detached instance (from LINQ materialization) — no persistence
        return;
      }
      var jsonString = JsonSerializer.Serialize(innerList, SerializerOptions);
      var fieldIndex = field.MappingInfo.Offset;

      // Follow the same pattern as Persistent.SetFieldValue:
      // BeforeTupleChange ensures DifferentialTuple is created,
      // then TupleChange marks the entity as Modified.
      owner.SystemBeforeTupleChange();
      owner.Tuple.SetValue(fieldIndex, jsonString);
      owner.SystemTupleChange();

      // Update snapshot so FlushIfDirty doesn't re-detect this change
      originalJson = jsonString;
    }

    /// <summary>
    /// Creates a detached (read-only snapshot) <see cref="JsonTypeArray{TItem}"/> from
    /// a deserialized JSON string. Used by LINQ materialization.
    /// </summary>
    internal static JsonTypeArray<TItem> FromJson(string json)
    {
      var list = string.IsNullOrEmpty(json)
        ? new List<TItem>()
        : JsonSerializer.Deserialize<List<TItem>>(json, SerializerOptions) ?? new List<TItem>();
      return new JsonTypeArray<TItem>(list);
    }

    // Constructors

    /// <summary>
    /// Creates a detached (not bound to entity) instance from an existing list.
    /// Mutations won't be persisted. Used for LINQ materialization results.
    /// </summary>
    private JsonTypeArray(List<TItem> items)
    {
      innerList = items ?? new List<TItem>();
    }

    internal JsonTypeArray(Persistent owner, FieldInfo field)
    {
      this.owner = owner;
      this.field = field;

      // Deserialize existing JSON from the tuple and take a snapshot
      var fieldIndex = field.MappingInfo.Offset;
      var jsonString = owner.Tuple.GetValueOrDefault<string>(fieldIndex);
      originalJson = jsonString;
      innerList = string.IsNullOrEmpty(jsonString)
        ? new List<TItem>()
        : JsonSerializer.Deserialize<List<TItem>>(jsonString, SerializerOptions) ?? new List<TItem>();

      // Register with the session for dirty-checking before persist
      if (owner is Entity entity && entity.Session != null) {
        entity.Session.RegisterJsonFieldAdapter(this);
      }
    }
  }
}
