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

    public Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
