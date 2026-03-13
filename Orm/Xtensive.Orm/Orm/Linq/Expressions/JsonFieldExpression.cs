// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Linq.Expressions
{
  /// <summary>
  /// Represents a JSON field expression in LINQ translation.
  /// Unlike StructureFieldExpression which maps to multiple columns,
  /// JsonFieldExpression maps to a single column containing serialized JSON
  /// and carries a JSON path for property access translation.
  /// </summary>
  internal sealed class JsonFieldExpression : PersistentFieldExpression
  {
    /// <summary>
    /// Gets the underlying persistent field (the JSON column field).
    /// </summary>
    public FieldInfo Field { get; }

    /// <summary>
    /// Gets the JSON path for this expression (e.g., "$", "$.Name", "$.Address.City").
    /// </summary>
    public string JsonPath { get; }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var newMapping = new Segment<int>(Mapping.Offset + offset, Mapping.Length);
      result = new JsonFieldExpression(Field, newMapping, JsonPath, Type, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap) {
        return this;
      }

      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      var offset = map.IndexOf(Mapping.Offset);
      if (offset < 0) {
        processedExpressions.Add(this, null);
        return null;
      }

      var newMapping = new Segment<int>(offset, Mapping.Length);
      result = new JsonFieldExpression(Field, newMapping, JsonPath, Type, OuterParameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      result = new JsonFieldExpression(Field, Mapping, JsonPath, Type, parameter, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      if (processedExpressions.TryGetValue(this, out var result)) {
        return result;
      }

      result = new JsonFieldExpression(Field, Mapping, JsonPath, Type, null, DefaultIfEmpty);
      processedExpressions.Add(this, result);
      return result;
    }

    /// <summary>
    /// Creates a new JsonFieldExpression for a nested property access.
    /// </summary>
    /// <param name="propertyName">The name of the property being accessed.</param>
    /// <param name="propertyType">The CLR type of the property.</param>
    /// <returns>A new JsonFieldExpression with the extended JSON path.</returns>
    public JsonFieldExpression CreatePropertyAccess(string propertyName, Type propertyType)
    {
      var newPath = JsonPath + "." + propertyName;
      return new JsonFieldExpression(Field, Mapping, newPath, propertyType, OuterParameter, DefaultIfEmpty);
    }

    /// <summary>
    /// Creates a JsonFieldExpression for a JSON column field on an entity.
    /// </summary>
    public static JsonFieldExpression CreateJsonField(FieldInfo jsonField, int offset)
    {
      var fieldMappingInfo = jsonField.MappingInfo;
      var mapping = new Segment<int>(offset + fieldMappingInfo.Offset, fieldMappingInfo.Length);
      return new JsonFieldExpression(jsonField, mapping, "$", jsonField.ValueType, null, false);
    }

    public override string ToString()
    {
      return $"{base.ToString()} JsonPath={JsonPath}";
    }

    // Constructors

    private JsonFieldExpression(
      FieldInfo field,
      in Segment<int> mapping,
      string jsonPath,
      Type type,
      ParameterExpression parameterExpression,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.JsonField, field.Name, type, mapping, field.UnderlyingProperty, parameterExpression, defaultIfEmpty)
    {
      Field = field;
      JsonPath = jsonPath;
    }
  }
}
