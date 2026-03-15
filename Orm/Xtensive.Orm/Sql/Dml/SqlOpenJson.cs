// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents an OPENJSON table-valued function call.
  /// Produces a rowset from a JSON array column.
  /// </summary>
  [Serializable]
  public class SqlOpenJson : SqlTable, ISqlQueryExpression
  {
    /// <summary>
    /// Gets the JSON expression (column reference or expression producing JSON text).
    /// </summary>
    public SqlExpression JsonExpression { get; private set; }

    /// <summary>
    /// Gets the optional JSON path to the array inside the JSON document.
    /// </summary>
    public SqlExpression Path { get; private set; }

    /// <summary>
    /// Gets the column definitions for the WITH clause.
    /// Each entry maps column name → (SQL type name, JSON path).
    /// </summary>
    public IReadOnlyList<OpenJsonColumnDef> ColumnDefinitions { get; private set; }

    internal override SqlOpenJson Clone(SqlNodeCloneContext context)
    {
      throw new NotImplementedException();
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public new IEnumerator<ISqlQueryExpression> GetEnumerator()
    {
      yield return this;
    }

    public SqlQueryExpression Except(ISqlQueryExpression operand) => throw new NotImplementedException();
    public SqlQueryExpression ExceptAll(ISqlQueryExpression operand) => throw new NotImplementedException();
    public SqlQueryExpression Intersect(ISqlQueryExpression operand) => throw new NotImplementedException();
    public SqlQueryExpression IntersectAll(ISqlQueryExpression operand) => throw new NotImplementedException();
    public SqlQueryExpression Union(ISqlQueryExpression operand) => throw new NotImplementedException();
    public SqlQueryExpression UnionAll(ISqlQueryExpression operand) => throw new NotImplementedException();

    // Constructors

    internal SqlOpenJson(SqlExpression jsonExpression, SqlExpression path, IList<OpenJsonColumnDef> columnDefs)
      : base(string.Empty)
    {
      JsonExpression = jsonExpression;
      Path = path;
      ColumnDefinitions = columnDefs?.ToList().AsReadOnly();

      // Build output columns from the column definitions
      var columnList = new List<SqlTableColumn>();
      if (columnDefs != null) {
        columnList.AddRange(columnDefs.Select(cd => SqlDml.TableColumn(this, cd.Name)));
      }
      columns = new SqlTableColumnCollection(columnList);
    }
  }

  /// <summary>
  /// Defines a column in the OPENJSON WITH clause.
  /// </summary>
  public class OpenJsonColumnDef
  {
    /// <summary>
    /// Gets the output column name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the SQL type name (e.g. "nvarchar(max)", "int", "decimal(18,4)").
    /// </summary>
    public string SqlTypeName { get; }

    /// <summary>
    /// Gets the JSON path for this column (e.g. "$.StringField").
    /// </summary>
    public string JsonPath { get; }

    /// <summary>
    /// Gets whether this column should use AS JSON modifier
    /// (for nested objects/arrays).
    /// </summary>
    public bool AsJson { get; }

    public OpenJsonColumnDef(string name, string sqlTypeName, string jsonPath, bool asJson = false)
    {
      Name = name;
      SqlTypeName = sqlTypeName;
      JsonPath = jsonPath;
      AsJson = asJson;
    }
  }
}
