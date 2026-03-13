// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Orm.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class JsonCompilers
  {
    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.JsonValue))]
    public static SqlExpression JsonValueCompiler(SqlExpression json, SqlExpression path)
    {
      return SqlDml.JsonValue(json, path);
    }

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.JsonQuery))]
    public static SqlExpression JsonQueryCompiler(SqlExpression json, SqlExpression path)
    {
      return SqlDml.JsonQuery(json, path);
    }
  }
}
