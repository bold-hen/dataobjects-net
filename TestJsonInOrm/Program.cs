using System.Transactions;

namespace TestJsonInOrm;

using Xtensive.Orm;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;

class Program
{
    private static Domain Domain;
    
    static void Main(string[] args)
    {
        var config =
        new DomainConfiguration("sqlserver",
          "Server=.;Database=TestDb;User Id=sa;Password=QwertY1#2;Encrypt=False;TrustServerCertificate=True;Integrated Security=false") {
          NamingConvention = new NamingConvention { NamingRules = NamingRules.UnderscoreDots },
          UpgradeMode = DomainUpgradeMode.Recreate,
          VersioningConvention = { EntityVersioningPolicy = EntityVersioningPolicy.Optimistic }
        };

        config.Types.Register(typeof(TestClass));
        Domain = Domain.Build(config);

        using (var session = Domain.OpenSession(new SessionConfiguration() {
                 DefaultIsolationLevel = IsolationLevel.ReadCommitted,
                 Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation
               }))
        using (var transaction = session.OpenTransaction(TransactionOpenMode.Auto)) {

          var entity = new TestClass();
          entity.JsonField = new TestJsonPoco()
          {
            BoolField = true,
            IntField = 123,
            DecimalField = 123.45m,
            DateTimeField = DateTime.Now,
            StringField = "Hello World!"
          };
          
          transaction.Complete();
        }
        
        
        using (var session = Domain.OpenSession(new SessionConfiguration() {
                 DefaultIsolationLevel = IsolationLevel.ReadCommitted,
                 Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation
               }))
        using (var transaction = session.OpenTransaction(TransactionOpenMode.Auto)) {

          var test = Query.All<TestClass>()
            .FirstOrDefault();

          if (test == null) {
            throw new Exception("TestClass with id = 1 was not found in db");
          }

          if (test.JsonField == null) {
            throw new Exception("jsonField must not be null");
          }

          if (test.JsonField.StringField == null) {
            throw new Exception("jsonField.StringField must not be null");
          }

          if (test.JsonField.IntField != 123) {
            throw new Exception("jsonField.IntField must be 123");
          }

          if (test.JsonField.DecimalField != 123.45m) {
            throw new Exception("jsonField.DecimalField must be 123.45");
          }

          if (test.JsonField.DateTimeField == (default)) {
            throw new Exception("jsonField.DateTimeField must not be default");
          }

          if (test.JsonField.BoolField != true) {
            throw new Exception("jsonField.BoolField must be true");
          }
          
          Console.WriteLine("Success!");
          
          transaction.Complete();
        }
    }

    [HierarchyRoot]
    public class TestClass : Entity
    {
        [Field]
        [Key]
        public int Id  { get; private set; }
        
        [Field]
        public TestJsonPoco JsonField { get; set; }
    }

    public class TestJsonPoco : JsonType
    {
        public string StringField { get; set; }
        
        public int IntField { get; set; }

        public decimal DecimalField { get; set; }
        
        public bool BoolField { get; set; }
        
        public DateTime DateTimeField { get; set; }
    }
}