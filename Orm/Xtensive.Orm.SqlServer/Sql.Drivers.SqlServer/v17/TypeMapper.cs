// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data;
using System.Data.Common;

namespace Xtensive.Sql.Drivers.SqlServer.v17
{
  internal class TypeMapper : v09.TypeMapper
  {
    public override void BindString(DbParameter parameter, object value)
    {
      base.BindString(parameter, value);
    }

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
