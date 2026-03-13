// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v17
{
  internal class ServerInfoProvider : v13.ServerInfoProvider
  {
    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = base.GetDataTypesInfo();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      // SQL Server 2025 native JSON data type
      types.Json = DataTypeInfo.Regular(SqlType.Json, common, "json");

      return types;
    }

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
