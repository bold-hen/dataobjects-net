// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm
{
  /// <summary>
  /// Abstract base class for types stored as JSON columns in the database.
  /// Unlike <see cref="Structure"/>, which maps to multiple columns,
  /// a <see cref="JsonType"/> maps to a single JSON column.
  /// Properties are plain C# properties and do not require <c>[Field]</c> attributes.
  /// Serialization is performed using <c>System.Text.Json</c>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// On SQL Server 2025 (v17+), the native <c>json</c> data type is used.
  /// On older SQL Server versions, <c>nvarchar(max)</c> is used as a fallback.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code>
  /// public class Address : JsonType
  /// {
  ///   public string City { get; set; }
  ///   public string Street { get; set; }
  ///   public int Building { get; set; }
  /// }
  ///
  /// [HierarchyRoot]
  /// public class Person : Entity
  /// {
  ///   [Field, Key]
  ///   public int Id { get; private set; }
  ///
  ///   [Field]
  ///   public Address HomeAddress { get; set; }
  ///
  ///   [Field]
  ///   public Address[] PreviousAddresses { get; set; }
  /// }
  /// </code>
  /// </example>
  public abstract class JsonType
  {
  }
}
