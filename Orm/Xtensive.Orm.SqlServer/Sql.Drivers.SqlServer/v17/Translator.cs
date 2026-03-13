// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.SqlServer.v17
{
  internal class Translator : v13.Translator
  {
    public override void Translate(IOutput output, SqlFunctionType functionType)
    {
      switch (functionType) {
        case SqlFunctionType.JsonValue:
          _ = output.Append("JSON_VALUE");
          break;
        case SqlFunctionType.JsonQuery:
          _ = output.Append("JSON_QUERY");
          break;
        case SqlFunctionType.JsonModify:
          _ = output.Append("JSON_MODIFY");
          break;
        case SqlFunctionType.IsJson:
          _ = output.Append("ISJSON");
          break;
        case SqlFunctionType.JsonPathExists:
          _ = output.Append("JSON_PATH_EXISTS");
          break;
        default:
          base.Translate(output, functionType);
          break;
      }
    }

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
