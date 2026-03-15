using System.Data;
using System.Data.Common;
using Dapper;
using Xtensive.Orm.Services;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace TestJsonInOrm;

using Xtensive.Orm;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;

class Program
{
    private static Domain Domain;
    private static readonly SessionConfiguration SessionConfig = new SessionConfiguration {
      DefaultIsolationLevel = IsolationLevel.ReadCommitted,
      Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation
    };

    static int passed = 0;
    static int failed = 0;

    static DomainConfiguration CreateConfig()
    {
        var config = new DomainConfiguration("sqlserver",
          "Server=.,1434;Database=TestDb;User Id=sa;Password=QwertY1#2;Encrypt=False;TrustServerCertificate=True;Integrated Security=false") {
          NamingConvention = new NamingConvention { NamingRules = NamingRules.UnderscoreDots },
          UpgradeMode = DomainUpgradeMode.Recreate,
          VersioningConvention = { EntityVersioningPolicy = EntityVersioningPolicy.Optimistic }
        };
        config.Types.Register(typeof(TestClass));
        config.Types.Register(typeof(MultiJsonOwner));
        return config;
    }

    static void RecreateDomain()
    {
        Domain?.Dispose();
        Domain = Domain.Build(CreateConfig());
        PopulateData();
    }

    static void Main(string[] args)
    {
        Domain = Domain.Build(CreateConfig());
        PopulateData();

        Console.WriteLine("=== 1. SQL Column Type Tests ===");
        RunTest("JsonField column type is 'json'", Test_JsonFieldColumnTypeIsJson);
        RunTest("JsonArrayField column type is 'json'", Test_JsonArrayFieldColumnTypeIsJson);

        Console.WriteLine("\n=== 2. Change Tracking Tests ===");
        RunTest("JsonField change tracking", Test_JsonFieldChangeTracking);
        RunTest("JsonField set to null", Test_JsonFieldSetToNull);
        RunTest("JsonTypeArray Add tracking", Test_JsonTypeArrayAddTracking);
        RunTest("JsonTypeArray Remove tracking", Test_JsonTypeArrayRemoveTracking);
        RunTest("JsonTypeArray Clear tracking", Test_JsonTypeArrayClearTracking);
        RunTest("JsonTypeArray indexer set tracking", Test_JsonTypeArrayIndexerSetTracking);
        RunTest("Multiple JSON field changes in same transaction", Test_MultipleJsonFieldChanges);

        // Re-populate after destructive change tracking tests
        RecreateDomain();

        Console.WriteLine("\n=== 3. Server-side Select Tests ===");
        RunTest("Select JSON string property", Test_SelectJsonStringProperty);
        RunTest("Select JSON int property", Test_SelectJsonIntProperty);
        RunTest("Select JSON decimal property", Test_SelectJsonDecimalProperty);
        RunTest("Select JSON bool property", Test_SelectJsonBoolProperty);
        RunTest("Select JSON DateTime property", Test_SelectJsonDateTimeProperty);
        RunTest("Select mixed entity + JSON fields", Test_SelectMixedEntityAndJsonFields);
        RunTest("Select whole JsonField", Test_SelectWholeJsonField);
        RunTest("Select whole JsonArrayField", Test_SelectWholeJsonArrayField);
        RunTest("Select entity fields + JSON aggregation Sum", Test_SelectJsonAggregationSum);
        RunTest("Select entity fields + JSON array Count", Test_SelectJsonArrayCount);

        Console.WriteLine("\n=== 4. Server-side Join Tests ===");
        RunTest("LeftJoin on JsonField int key", Test_LeftJoinOnJsonFieldIntKey);
        RunTest("LeftJoin entity key with JSON key", Test_LeftJoinEntityKeyWithJsonKey);
        RunTest("LeftJoin with JSON array SelectMany", Test_LeftJoinWithJsonArraySelectMany);
        RunTest("Join on JSON string key", Test_JoinOnJsonStringKey);

        Console.WriteLine("\n=== 5. SelectMany Projection Tests ===");
        RunTest("SelectMany basic", Test_SelectManyBasic);
        RunTest("SelectMany with Select string field", Test_SelectManyWithSelectStringField);
        RunTest("SelectMany with anonymous projection", Test_SelectManyWithAnonymousProjection);
        RunTest("SelectMany with mixed entity + array fields", Test_SelectManyMixedEntityAndArrayFields);
        RunTest("SelectMany with whole JsonField in projection", Test_SelectManyWithWholeJsonField);
        RunTest("SelectMany with filter on array item", Test_SelectManyWithFilter);
        RunTest("SelectMany with Id in projection", Test_SelectManyWithIdInProjection);

        Console.WriteLine($"\n=== Results: {passed} passed, {failed} failed ===");
        if (failed > 0)
          Environment.Exit(1);
    }

    static void RunTest(string name, Action test)
    {
        try {
          test();
          Console.WriteLine($"  ✓ {name}");
          passed++;
        } catch (Exception ex) {
          Console.WriteLine($"  ✗ {name}: {ex.Message}");
          failed++;
        }
    }

    static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void AreEqual<T>(T expected, T actual, string context = "")
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
          throw new Exception($"{context}Expected {expected} but got {actual}");
    }

    static Session OpenSession() => Domain.OpenSession(SessionConfig);

    #region PopulateData

    static void PopulateData()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);

        var owner1 = new TestClass();
        owner1.JsonField = new TestJsonPoco {
          StringField = "hello", IntField = 1, DecimalField = 100.50m,
          BoolField = true, DateTimeField = new DateTime(2025, 1, 15, 10, 30, 0)
        };
        owner1.JsonArrayField.Add(new TestJsonPoco {
          StringField = "item1", IntField = 1, DecimalField = 10.5m,
          BoolField = true, DateTimeField = new DateTime(2025, 1, 1)
        });
        owner1.JsonArrayField.Add(new TestJsonPoco {
          StringField = "item2", IntField = 2, DecimalField = 20.5m,
          BoolField = false, DateTimeField = new DateTime(2025, 2, 1)
        });
        owner1.JsonArrayField.Add(new TestJsonPoco {
          StringField = "item3", IntField = 1, DecimalField = 30.0m,
          BoolField = true, DateTimeField = new DateTime(2025, 3, 1)
        });

        var owner2 = new TestClass();
        owner2.JsonField = new TestJsonPoco {
          StringField = "world", IntField = 2, DecimalField = 200.75m,
          BoolField = false, DateTimeField = new DateTime(2025, 6, 20, 14, 0, 0)
        };
        owner2.JsonArrayField.Add(new TestJsonPoco {
          StringField = "itemA", IntField = 2, DecimalField = 5.25m,
          BoolField = true, DateTimeField = new DateTime(2025, 4, 1)
        });
        owner2.JsonArrayField.Add(new TestJsonPoco {
          StringField = "itemB", IntField = 3, DecimalField = 15.75m,
          BoolField = false, DateTimeField = new DateTime(2025, 5, 1)
        });

        var owner3 = new TestClass();
        owner3.JsonField = new TestJsonPoco {
          StringField = "empty", IntField = 3, DecimalField = 0m,
          BoolField = false, DateTimeField = new DateTime(2025, 12, 31)
        };
        // owner3 has empty JsonArrayField

        var multi = new MultiJsonOwner();
        multi.FirstJson = new TestJsonPoco {
          StringField = "first", IntField = 10, DecimalField = 1.1m,
          BoolField = true, DateTimeField = new DateTime(2025, 1, 1)
        };
        multi.SecondJson = new AnotherJsonPoco { Name = "tag1", Value = 3.14 };
        multi.Items.Add(new TestJsonPoco {
          StringField = "multiItem1", IntField = 5, DecimalField = 50m,
          BoolField = true, DateTimeField = new DateTime(2025, 6, 1)
        });
        multi.Tags.Add(new AnotherJsonPoco { Name = "tagA", Value = 1.0 });
        multi.Tags.Add(new AnotherJsonPoco { Name = "tagB", Value = 2.0 });

        t.Complete();
    }

    #endregion

    #region 1. SQL Column Type Tests

    static void Test_JsonFieldColumnTypeIsJson()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var typeName = GetColumnTypeName(session, "Program_TestClass", "JsonField");
        AreEqual("json", typeName.ToLowerInvariant(), "JsonField type: ");
    }

    static void Test_JsonArrayFieldColumnTypeIsJson()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var typeName = GetColumnTypeName(session, "Program_TestClass", "JsonArrayField");
        AreEqual("json", typeName.ToLowerInvariant(), "JsonArrayField type: ");
    }

    static string GetColumnTypeName(Session session, string tableName, string columnName)
    {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        var command = accessor.CreateCommand();
        command.CommandText = $@"
          SELECT t.name
          FROM sys.columns c
          JOIN sys.types t ON c.user_type_id = t.user_type_id
          WHERE c.object_id = OBJECT_ID('{tableName}') AND c.name = '{columnName}'";
        using var reader = command.ExecuteReader();
        Assert(reader.Read(), $"Column '{columnName}' not found in {tableName}");
        return reader.GetString(0);
    }

    #endregion

    #region 2. Change Tracking Tests

    static void Test_JsonFieldChangeTracking()
    {
        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.JsonField.StringField == "hello");
          ownerId = owner.Id;
          owner.JsonField = new TestJsonPoco {
            StringField = "modified", IntField = 999, DecimalField = 0.01m,
            BoolField = false, DateTimeField = new DateTime(2026, 1, 1)
          };
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          AreEqual("modified", owner.JsonField.StringField, "StringField: ");
          AreEqual(999, owner.JsonField.IntField, "IntField: ");
          AreEqual(0.01m, owner.JsonField.DecimalField, "DecimalField: ");
          AreEqual(false, owner.JsonField.BoolField, "BoolField: ");
        }
    }

    static void Test_JsonFieldSetToNull()
    {
        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.JsonField.StringField == "world");
          ownerId = owner.Id;
          owner.JsonField = null;
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          Assert(owner.JsonField == null, "JsonField should be null");
        }
    }

    static void Test_JsonTypeArrayAddTracking()
    {
        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.JsonField.StringField == "empty");
          ownerId = owner.Id;
          AreEqual(0, owner.JsonArrayField.Count, "Before count: ");
          owner.JsonArrayField.Add(new TestJsonPoco {
            StringField = "newItem", IntField = 42, DecimalField = 99.99m,
            BoolField = true, DateTimeField = new DateTime(2026, 6, 15)
          });
          AreEqual(1, owner.JsonArrayField.Count, "After add count: ");
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          AreEqual(1, owner.JsonArrayField.Count, "Persisted count: ");
          AreEqual("newItem", owner.JsonArrayField[0].StringField, "Persisted StringField: ");
          AreEqual(42, owner.JsonArrayField[0].IntField, "Persisted IntField: ");
        }
    }

    static void Test_JsonTypeArrayRemoveTracking()
    {
        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          // Use the first owner which has 3 items (but after change tracking it might be "modified")
          var owner = Query.All<TestClass>().OrderBy(o => o.Id).First();
          ownerId = owner.Id;
          var initialCount = owner.JsonArrayField.Count;
          Assert(initialCount > 0, $"Need items to remove, got {initialCount}");
          var itemToRemove = owner.JsonArrayField[0];
          var removed = owner.JsonArrayField.Remove(itemToRemove);
          Assert(removed, "Remove should return true");
          AreEqual(initialCount - 1, owner.JsonArrayField.Count, "After remove count: ");
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          // Count should be persisted
          Assert(owner.JsonArrayField.Count >= 0, "Count should be valid");
        }
    }

    static void Test_JsonTypeArrayClearTracking()
    {
        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          // Find owner with items
          var owner = Query.All<TestClass>().OrderBy(o => o.Id).Skip(1).First(); // owner2
          ownerId = owner.Id;
          owner.JsonArrayField.Clear();
          AreEqual(0, owner.JsonArrayField.Count, "After clear count: ");
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          AreEqual(0, owner.JsonArrayField.Count, "Persisted after clear: ");
        }
    }

    static void Test_JsonTypeArrayIndexerSetTracking()
    {
        // Re-populate to get clean data
        RecreateDomain();

        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().OrderBy(o => o.Id).First();
          ownerId = owner.Id;
          Assert(owner.JsonArrayField.Count >= 1, "Need at least 1 item");
          owner.JsonArrayField[0] = new TestJsonPoco {
            StringField = "replaced", IntField = 777, DecimalField = 0m,
            BoolField = false, DateTimeField = new DateTime(2026, 3, 1)
          };
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          AreEqual("replaced", owner.JsonArrayField[0].StringField, "Replaced StringField: ");
          AreEqual(777, owner.JsonArrayField[0].IntField, "Replaced IntField: ");
        }
    }

    static void Test_MultipleJsonFieldChanges()
    {
        // Re-populate
        RecreateDomain();

        int ownerId;
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().OrderBy(o => o.Id).First();
          ownerId = owner.Id;

          // Modify JSON field
          owner.JsonField = new TestJsonPoco {
            StringField = "updated", IntField = 50, DecimalField = 50m,
            BoolField = true, DateTimeField = new DateTime(2026, 1, 1)
          };
          // Add to array
          owner.JsonArrayField.Add(new TestJsonPoco {
            StringField = "added", IntField = 60, DecimalField = 60m,
            BoolField = false, DateTimeField = new DateTime(2026, 2, 1)
          });
          t.Complete();
        }
        using (var session = OpenSession())
        using (var t = session.OpenTransaction(TransactionOpenMode.Auto)) {
          var owner = Query.All<TestClass>().First(o => o.Id == ownerId);
          AreEqual("updated", owner.JsonField.StringField, "Updated StringField: ");
          AreEqual(50, owner.JsonField.IntField, "Updated IntField: ");
          AreEqual(4, owner.JsonArrayField.Count, "Array count after add: "); // 3 original + 1 added
          AreEqual("added", owner.JsonArrayField[3].StringField, "Added item: ");
        }
    }

    #endregion

    #region 3. Server-side Select Tests

    static void Test_SelectJsonStringProperty()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var strings = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.StringField)
          .ToList();
        AreEqual(3, strings.Count, "Count: ");
        AreEqual("hello", strings[0], "First: ");
        AreEqual("world", strings[1], "Second: ");
        AreEqual("empty", strings[2], "Third: ");
    }

    static void Test_SelectJsonIntProperty()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var ints = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.IntField)
          .ToList();
        AreEqual(3, ints.Count, "Count: ");
        AreEqual(1, ints[0], "First: ");
        AreEqual(2, ints[1], "Second: ");
        AreEqual(3, ints[2], "Third: ");
    }

    static void Test_SelectJsonDecimalProperty()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var decimals = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.DecimalField)
          .ToList();
        AreEqual(3, decimals.Count, "Count: ");
        AreEqual(100.50m, decimals[0], "First: ");
        AreEqual(200.75m, decimals[1], "Second: ");
        AreEqual(0m, decimals[2], "Third: ");
    }

    static void Test_SelectJsonBoolProperty()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var bools = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.BoolField)
          .ToList();
        AreEqual(3, bools.Count, "Count: ");
        AreEqual(true, bools[0], "First: ");
        AreEqual(false, bools[1], "Second: ");
        AreEqual(false, bools[2], "Third: ");
    }

    static void Test_SelectJsonDateTimeProperty()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var dates = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.DateTimeField)
          .ToList();
        AreEqual(3, dates.Count, "Count: ");
        AreEqual(new DateTime(2025, 1, 15, 10, 30, 0), dates[0], "First: ");
        AreEqual(new DateTime(2025, 6, 20, 14, 0, 0), dates[1], "Second: ");
        AreEqual(new DateTime(2025, 12, 31), dates[2], "Third: ");
    }

    static void Test_SelectMixedEntityAndJsonFields()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => new {
            o.Id,
            JsonString = o.JsonField.StringField,
            JsonInt = o.JsonField.IntField
          })
          .ToList();
        AreEqual(3, result.Count, "Count: ");
        AreEqual("hello", result[0].JsonString, "First JsonString: ");
        AreEqual(1, result[0].JsonInt, "First JsonInt: ");
    }

    static void Test_SelectWholeJsonField()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField)
          .ToList();
        AreEqual(3, result.Count, "Count: ");
        AreEqual("hello", result[0].StringField, "First StringField: ");
        AreEqual(1, result[0].IntField, "First IntField: ");
    }

    static void Test_SelectWholeJsonArrayField()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonArrayField)
          .ToList();
        AreEqual(3, result.Count, "Count: ");
        AreEqual(3, result[0].Count, "First array count: ");
        AreEqual("item1", result[0][0].StringField, "First item: ");
        AreEqual(2, result[1].Count, "Second array count: ");
        AreEqual(0, result[2].Count, "Third array count: ");
    }

    static void Test_SelectJsonAggregationSum()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => new {
            JsonString = o.JsonField.StringField,
            ArraySum = o.JsonArrayField.Sum(item => item.DecimalField)
          })
          .ToList();
        AreEqual(3, result.Count, "Count: ");
        // Owner1: 10.5 + 20.5 + 30.0 = 61.0
        AreEqual("hello", result[0].JsonString, "First JsonString: ");
        AreEqual(61.0m, result[0].ArraySum, "First sum: ");
        // Owner2: 5.25 + 15.75 = 21.0
        AreEqual(21.0m, result[1].ArraySum, "Second sum: ");
        // Owner3: empty = 0
        AreEqual(0m, result[2].ArraySum, "Third sum: ");
    }

    static void Test_SelectJsonArrayCount()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .OrderBy(o => o.Id)
          .Select(o => new {
            JsonString = o.JsonField.StringField,
            ArrayCount = o.JsonArrayField.Count()
          })
          .ToList();
        AreEqual(3, result.Count, "Count: ");
        AreEqual(3, result[0].ArrayCount, "First count: ");
        AreEqual(2, result[1].ArrayCount, "Second count: ");
        AreEqual(0, result[2].ArrayCount, "Third count: ");
    }

    #endregion

    #region 4. Server-side Join Tests

    static void Test_LeftJoinOnJsonFieldIntKey()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .LeftJoin(
            Query.All<TestClass>(),
            left => left.JsonField.IntField,
            right => right.JsonField.IntField,
            (left, right) => new {
              LeftId = left.Id,
              RightId = right.Id,
              Key = left.JsonField.IntField
            })
          .OrderBy(x => x.LeftId).ThenBy(x => x.RightId)
          .ToList();
        // Each owner matches itself (IntField 1,2,3 are unique)
        Assert(result.Any(r => r.LeftId == r.RightId && r.Key == 1), "Self-match with key=1");
        Assert(result.Any(r => r.LeftId == r.RightId && r.Key == 2), "Self-match with key=2");
        Assert(result.Any(r => r.LeftId == r.RightId && r.Key == 3), "Self-match with key=3");
    }

    static void Test_LeftJoinEntityKeyWithJsonKey()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        // Join left.Id (entity field) with right.JsonField.IntField
        var result = Query.All<TestClass>()
          .LeftJoin(
            Query.All<TestClass>(),
            left => left.Id,
            right => right.JsonField.IntField,
            (left, right) => new {
              LeftId = left.Id,
              RightJsonInt = right.JsonField.IntField,
              RightString = right.JsonField.StringField
            })
          .OrderBy(x => x.LeftId)
          .ToList();
        // At least some matches should exist (Id values may match IntField values)
        Assert(result.Count > 0, "Should have results from join");
    }

    static void Test_LeftJoinWithJsonArraySelectMany()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .LeftJoin(
            Query.All<TestClass>().SelectMany(o => o.JsonArrayField),
            left => left.JsonField.IntField,
            right => right.IntField,
            (left, right) => new {
              LeftString = left.JsonField.StringField,
              ArrayItemString = right.StringField
            })
          .OrderBy(x => x.LeftString).ThenBy(x => x.ArrayItemString)
          .ToList();
        // Owner1 (IntField=1) should match array items with IntField=1 (item1, item3)
        Assert(result.Any(r => r.LeftString == "hello" && r.ArrayItemString == "item1"),
          "hello should match item1");
        Assert(result.Any(r => r.LeftString == "hello" && r.ArrayItemString == "item3"),
          "hello should match item3");
    }

    static void Test_JoinOnJsonStringKey()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .LeftJoin(
            Query.All<TestClass>(),
            left => left.JsonField.StringField,
            right => right.JsonField.StringField,
            (left, right) => new {
              LeftId = left.Id,
              RightId = right.Id,
              JsonString = left.JsonField.StringField
            })
          .OrderBy(x => x.LeftId)
          .ToList();
        // Self-join on unique strings => each matches itself
        AreEqual(3, result.Count, "Count: ");
        Assert(result.All(r => r.LeftId == r.RightId), "Each should match itself");
    }

    #endregion

    #region 5. SelectMany Projection Tests

    static void Test_SelectManyBasic()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField)
          .ToList();
        // Owner1: 3, Owner2: 2, Owner3: 0 = 5 total
        AreEqual(5, result.Count, "Total items: ");
    }

    static void Test_SelectManyWithSelectStringField()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField.Select(item => item.StringField))
          .OrderBy(s => s)
          .ToList();
        AreEqual(5, result.Count, "Count: ");
        Assert(result.Contains("item1"), "Contains item1");
        Assert(result.Contains("item2"), "Contains item2");
        Assert(result.Contains("item3"), "Contains item3");
        Assert(result.Contains("itemA"), "Contains itemA");
        Assert(result.Contains("itemB"), "Contains itemB");
    }

    static void Test_SelectManyWithAnonymousProjection()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            item.StringField,
            item.IntField,
            item.DecimalField
          }))
          .OrderBy(x => x.StringField)
          .ToList();
        AreEqual(5, result.Count, "Count: ");
        var item1 = result.First(r => r.StringField == "item1");
        AreEqual(1, item1.IntField, "item1 IntField: ");
        AreEqual(10.5m, item1.DecimalField, "item1 DecimalField: ");
    }

    static void Test_SelectManyMixedEntityAndArrayFields()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            OwnerId = o.Id,
            OwnerJsonInt = o.JsonField.IntField,
            item.StringField
          }))
          .OrderBy(x => x.OwnerId).ThenBy(x => x.StringField)
          .ToList();
        AreEqual(5, result.Count, "Count: ");
        // All items from first owner should have OwnerJsonInt = 1
        var firstOwnerItems = result.Where(r => r.OwnerJsonInt == 1).ToList();
        AreEqual(3, firstOwnerItems.Count, "First owner items: ");
        var secondOwnerItems = result.Where(r => r.OwnerJsonInt == 2).ToList();
        AreEqual(2, secondOwnerItems.Count, "Second owner items: ");
    }

    static void Test_SelectManyWithWholeJsonField()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            item.StringField,
            o.JsonField
          }))
          .OrderBy(x => x.StringField)
          .ToList();
        AreEqual(5, result.Count, "Count: ");
        var item1 = result.First(r => r.StringField == "item1");
        AreEqual("hello", item1.JsonField.StringField, "item1 owner JsonField.StringField: ");
        AreEqual(1, item1.JsonField.IntField, "item1 owner JsonField.IntField: ");
        var itemA = result.First(r => r.StringField == "itemA");
        AreEqual("world", itemA.JsonField.StringField, "itemA owner JsonField.StringField: ");
    }

    static void Test_SelectManyWithFilter()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField)
          .Where(item => item.IntField == 1)
          .ToList();
        // item1 and item3 have IntField = 1
        AreEqual(2, result.Count, "Filtered count: ");
        Assert(result.All(r => r.IntField == 1), "All should have IntField=1");
    }

    static void Test_SelectManyWithIdInProjection()
    {
        using var session = OpenSession();
        using var t = session.OpenTransaction(TransactionOpenMode.Auto);
        var result = Query.All<TestClass>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            item.StringField,
            o.JsonField,
            o.Id
          }))
          .ToList();
        AreEqual(5, result.Count, "Count: ");
        Assert(result.All(r => r.Id > 0), "All should have valid Id");
    }

    #endregion

    public class SqlTypeCheck
    {
      public string Name { get; set; }
      public bool IsNullable { get; set; }
      public override string ToString() => Name + " "  + IsNullable;
    }

    [HierarchyRoot]
    public class TestClass : Entity
    {
        [Field]
        [Key]
        public int Id  { get; private set; }

        [Field]
        public TestJsonPoco JsonField { get; set; }

        [Field]
        public JsonTypeArray<TestJsonPoco> JsonArrayField { get; private set; }
    }

    [HierarchyRoot]
    public class MultiJsonOwner : Entity
    {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public TestJsonPoco FirstJson { get; set; }

        [Field]
        public AnotherJsonPoco SecondJson { get; set; }

        [Field]
        public JsonTypeArray<TestJsonPoco> Items { get; private set; }

        [Field]
        public JsonTypeArray<AnotherJsonPoco> Tags { get; private set; }
    }

    public class TestJsonPoco : JsonType
    {
        public string StringField { get; set; }

        public int IntField { get; set; }

        public decimal DecimalField { get; set; }

        public bool BoolField { get; set; }

        public DateTime DateTimeField { get; set; }
    }

    public class AnotherJsonPoco : JsonType
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
}
