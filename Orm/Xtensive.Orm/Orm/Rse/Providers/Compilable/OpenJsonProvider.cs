// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Expands a JSON array column into a rowset via OPENJSON.
  /// Each property of the JSON element type becomes a column.
  /// </summary>
  [Serializable]
  public sealed class OpenJsonProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Gets the column index in the outer provider that contains the JSON data.
    /// </summary>
    public int JsonColumnIndex { get; }

    /// <summary>
    /// Gets the element type of the JSON array (the JsonType subclass).
    /// </summary>
    public Type ElementType { get; }

    /// <summary>
    /// Gets the JSON field info from the model.
    /// </summary>
    public FieldInfo JsonField { get; }

    /// <summary>
    /// Gets the column definitions for OPENJSON WITH clause.
    /// Maps property name → (CLR type, SQL type name, JSON path).
    /// </summary>
    public IReadOnlyList<OpenJsonColumnInfo> ColumnInfos { get; }

    protected override RecordSetHeader BuildHeader() => header;

    public OpenJsonProvider(FieldInfo jsonField, Type elementType, IList<OpenJsonColumnInfo> columnInfos)
      : base(ProviderType.OpenJson)
    {
      JsonField = jsonField;
      ElementType = elementType;
      JsonColumnIndex = jsonField.MappingInfo.Offset;
      ColumnInfos = columnInfos.ToList().AsReadOnly();

      var fieldTypes = columnInfos.Select(c => c.ClrType).ToArray();
      var tupleDescriptor = TupleDescriptor.Create(fieldTypes);
      var columns = columnInfos
        .Select((c, i) => (Column) new MappedColumn(c.Name, i, c.ClrType))
        .ToArray();
      header = new RecordSetHeader(tupleDescriptor, columns);

      Initialize();
    }
  }

  /// <summary>
  /// Describes a column produced by OPENJSON WITH clause.
  /// </summary>
  public class OpenJsonColumnInfo
  {
    public string Name { get; }
    public Type ClrType { get; }
    public string SqlTypeName { get; }
    public string JsonPath { get; }

    public OpenJsonColumnInfo(string name, Type clrType, string sqlTypeName, string jsonPath)
    {
      Name = name;
      ClrType = clrType;
      SqlTypeName = sqlTypeName;
      JsonPath = jsonPath;
    }
  }
}
