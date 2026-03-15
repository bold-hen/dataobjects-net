// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v17
{
  internal class Compiler : v13.Compiler
  {
    public override void Visit(SqlFunctionCall node)
    {
      switch (node.FunctionType) {
        case SqlFunctionType.JsonValue:
        case SqlFunctionType.JsonQuery:
        case SqlFunctionType.JsonModify:
        case SqlFunctionType.IsJson:
        case SqlFunctionType.JsonPathExists:
          // JSON functions compile with standard pattern: FUNCTION_NAME(arg1, arg2, ...)
          base.Visit(node);
          break;
        default:
          base.Visit(node);
          break;
      }
    }

    /// <inheritdoc/>
    public override void Visit(SqlOpenJson node)
    {
      var output = context.Output;

      _ = output.AppendOpeningPunctuation("OPENJSON(");
      node.JsonExpression.AcceptVisitor(this);

      if (node.Path != null) {
        _ = output.Append(", ");
        node.Path.AcceptVisitor(this);
      }

      _ = output.Append(")");

      // WITH clause for typed column definitions
      if (node.ColumnDefinitions?.Count > 0) {
        _ = output.Append(" WITH (");
        var first = true;
        foreach (var colDef in node.ColumnDefinitions) {
          if (!first)
            _ = output.Append(", ");
          first = false;
          translator.TranslateIdentifier(output, colDef.Name);
          _ = output.Append(" ");
          _ = output.Append(colDef.SqlTypeName);
          _ = output.Append(" '");
          _ = output.Append(colDef.JsonPath);
          _ = output.Append("'");
          if (colDef.AsJson)
            _ = output.Append(" AS JSON");
        }
        _ = output.Append(") ");
      }
      else {
        _ = output.Append(" ");
      }
    }

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
