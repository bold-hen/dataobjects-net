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
    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.JsonValue), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression JsonValueCompiler(SqlExpression json, SqlExpression path)
    {
      return SqlDml.JsonValue(json, path);
    }

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.JsonQuery), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression JsonQueryCompiler(SqlExpression json, SqlExpression path)
    {
      return SqlDml.JsonQuery(json, path);
    }

    // Invariant-culture parse helpers — translate to SQL CAST

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseByte), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseByteCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToByte(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseSByte), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseSByteCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToSbyte(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseInt16), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseInt16Compiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToShort(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseUInt16), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseUInt16Compiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToUshort(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseInt32), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseInt32Compiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToInt(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseUInt32), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseUInt32Compiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToUint(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseInt64), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseInt64Compiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToLong(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseUInt64), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseUInt64Compiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToUlong(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseSingle), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseSingleCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToFloat(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseDouble), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseDoubleCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToDouble(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseDecimal), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseDecimalCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToDecimal(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseBoolean), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseBooleanCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToBool(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseDateTime), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseDateTimeCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToDateTime(str);

    [Compiler(typeof(JsonExpressionHelper), nameof(JsonExpressionHelper.ParseDateTimeOffset), TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ParseDateTimeOffsetCompiler(SqlExpression str)
      => ExpressionTranslationHelpers.ToDateTime(str);
  }
}
