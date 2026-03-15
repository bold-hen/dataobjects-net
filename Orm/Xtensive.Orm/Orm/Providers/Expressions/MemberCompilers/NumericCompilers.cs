// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.15

using System;
using Xtensive.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  [CompilerContainer(typeof(SqlExpression))]
  internal static class NumericCompilers
  {
    #region ToString mappings

    [Compiler(typeof(byte), "ToString")]
    public static SqlExpression ByteToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(sbyte), "ToString")]
    public static SqlExpression SByteToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(short), "ToString")]
    public static SqlExpression ShortToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(ushort), "ToString")]
    public static SqlExpression UShortToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(int), "ToString")]
    public static SqlExpression IntToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(uint), "ToString")]
    public static SqlExpression UIntToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(long), "ToString")]
    public static SqlExpression LongToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(ulong), "ToString")]
    public static SqlExpression ULongToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(float), "ToString")]
    public static SqlExpression FloatToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(double), "ToString")]
    public static SqlExpression DoubleToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    [Compiler(typeof(decimal), "ToString")]
    public static SqlExpression DecimalToString(SqlExpression _this)
    {
      return ExpressionTranslationHelpers.ToChar(_this);
    }

    #endregion

    #region Parse mappings

    [Compiler(typeof(byte), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ByteParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToByte(str);
    }

    [Compiler(typeof(sbyte), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression SByteParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToSbyte(str);
    }

    [Compiler(typeof(short), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ShortParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToShort(str);
    }

    [Compiler(typeof(ushort), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression UShortParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToUshort(str);
    }

    [Compiler(typeof(int), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression IntParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToInt(str);
    }

    [Compiler(typeof(uint), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression UIntParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToUint(str);
    }

    [Compiler(typeof(long), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression LongParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToLong(str);
    }

    [Compiler(typeof(ulong), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression ULongParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToUlong(str);
    }

    [Compiler(typeof(float), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression FloatParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToFloat(str);
    }

    [Compiler(typeof(double), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression DoubleParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToDouble(str);
    }

    [Compiler(typeof(bool), "Parse", TargetKind.Static | TargetKind.Method)]
    public static SqlExpression BoolParse([Type(typeof(string))] SqlExpression str)
    {
      return ExpressionTranslationHelpers.ToBool(str);
    }

    #endregion

    #region CompareTo mappings

    [Compiler(typeof(byte), "CompareTo")]
    public static SqlExpression ByteCompareTo(SqlExpression _this,
      [Type(typeof(byte))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }
    
    [Compiler(typeof(sbyte), "CompareTo")]
    public static SqlExpression SByteCompareTo(SqlExpression _this,
      [Type(typeof(sbyte))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(short), "CompareTo")]
    public static SqlExpression ShortCompareTo(SqlExpression _this,
      [Type(typeof(short))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(ushort), "CompareTo")]
    public static SqlExpression UShortCompareTo(SqlExpression _this,
      [Type(typeof(ushort))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(int), "CompareTo")]
    public static SqlExpression IntCompareTo(SqlExpression _this,
      [Type(typeof(int))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(uint), "CompareTo")]
    public static SqlExpression UIntCompareTo(SqlExpression _this,
      [Type(typeof(uint))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(long), "CompareTo")]
    public static SqlExpression LongCompareTo(SqlExpression _this,
      [Type(typeof(long))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(ulong), "CompareTo")]
    public static SqlExpression ULongCompareTo(SqlExpression _this,
      [Type(typeof(ulong))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(float), "CompareTo")]
    public static SqlExpression FloatCompareTo(SqlExpression _this,
      [Type(typeof(float))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(double), "CompareTo")]
    public static SqlExpression DoubleCompareTo(SqlExpression _this,
      [Type(typeof(double))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    [Compiler(typeof(decimal), "CompareTo")]
    public static SqlExpression DecimalCompareTo(SqlExpression _this,
      [Type(typeof(decimal))] SqlExpression value)
    {
      return MathCompilers.GenericSign(_this - value);
    }

    #endregion

  }
}