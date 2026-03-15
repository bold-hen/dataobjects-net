// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SqlServer.v17
{
  internal class Driver : v13.Driver
  {
    protected override Sql.Compiler.SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override Sql.Compiler.SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override Sql.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    protected override void RegisterCustomReverseMappings(TypeMappingRegistryBuilder builder)
    {
      base.RegisterCustomReverseMappings(builder);
      builder.AddReverse(SqlType.Json, typeof(string));
    }

    public Driver(CoreServerInfo coreServerInfo, ErrorMessageParser errorMessageParser, bool checkConnectionIsAlive)
      : base(coreServerInfo, errorMessageParser, checkConnectionIsAlive)
    {
    }
  }
}
