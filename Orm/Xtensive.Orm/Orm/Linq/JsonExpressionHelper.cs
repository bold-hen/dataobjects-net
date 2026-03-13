// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text.Json;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Provides helper methods for JSON property access in LINQ expressions.
  /// These methods serve as markers that the SQL compiler translates to
  /// JSON_VALUE / JSON_QUERY SQL function calls.
  /// The runtime implementations serve as fallbacks for client-side evaluation.
  /// </summary>
  internal static class JsonExpressionHelper
  {
    /// <summary>
    /// Extracts a scalar value from a JSON string at the specified path.
    /// Translates to JSON_VALUE(json, path) in SQL.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="path">The JSON path (e.g., "$.Name", "$.Address.City").</param>
    /// <returns>The extracted string value, or null if the path doesn't exist.</returns>
    public static string JsonValue(string json, string path)
    {
      if (string.IsNullOrEmpty(json)) {
        return null;
      }

      try {
        using var document = JsonDocument.Parse(json);
        var pathSegments = path.TrimStart('$', '.').Split('.');
        JsonElement current = document.RootElement;
        foreach (var segment in pathSegments) {
          if (string.IsNullOrEmpty(segment)) {
            continue;
          }

          if (!current.TryGetProperty(segment, out var next)) {
            return null;
          }

          current = next;
        }

        return current.ValueKind switch {
          JsonValueKind.String => current.GetString(),
          JsonValueKind.Number => current.GetRawText(),
          JsonValueKind.True => "true",
          JsonValueKind.False => "false",
          JsonValueKind.Null => null,
          _ => current.GetRawText()
        };
      }
      catch {
        return null;
      }
    }

    /// <summary>
    /// Extracts an object or array from a JSON string at the specified path.
    /// Translates to JSON_QUERY(json, path) in SQL.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="path">The JSON path (e.g., "$.Address", "$.Items").</param>
    /// <returns>The extracted JSON fragment, or null if the path doesn't exist.</returns>
    public static string JsonQuery(string json, string path)
    {
      if (string.IsNullOrEmpty(json)) {
        return null;
      }

      try {
        using var document = JsonDocument.Parse(json);
        var pathSegments = path.TrimStart('$', '.').Split('.');
        JsonElement current = document.RootElement;
        foreach (var segment in pathSegments) {
          if (string.IsNullOrEmpty(segment)) {
            continue;
          }

          if (!current.TryGetProperty(segment, out var next)) {
            return null;
          }

          current = next;
        }

        return current.ValueKind is JsonValueKind.Object or JsonValueKind.Array
          ? current.GetRawText()
          : null;
      }
      catch {
        return null;
      }
    }
  }
}
