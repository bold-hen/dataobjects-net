using System;

namespace Xtensive.Orm;

/// <summary>
/// Extended fields for all indexes
/// </summary>
public class IndexesExtensionAttribute : Attribute
{
  /// <summary>
  /// ctor
  /// </summary>
  /// <param name="fields">fields</param>
  public IndexesExtensionAttribute(string[] fields)
  {
    KeyFields = fields;
  }
  
  /// <summary>
  /// Fields
  /// Fields to extend current's type primary index
  /// </summary>
  public string[] KeyFields { get; set; }
}