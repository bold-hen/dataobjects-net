// Copyright (C) 2024-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.JsonTypeTestModel;

namespace Xtensive.Orm.Tests.Storage.JsonTypeTestModel
{
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

  [HierarchyRoot]
  public class JsonOwner : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TestJsonPoco JsonField { get; set; }

    [Field]
    public JsonTypeArray<TestJsonPoco> JsonArrayField { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public int NumericValue { get; set; }
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
}

namespace Xtensive.Orm.Tests.Storage
{
  [Category("Json")]
  [TestFixture]
  public class JsonTypeTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(JsonOwner));
      config.Types.Register(typeof(MultiJsonOwner));
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner1 = new JsonOwner {
          Name = "Owner1",
          NumericValue = 10,
          JsonField = new TestJsonPoco {
            StringField = "hello",
            IntField = 1,
            DecimalField = 100.50m,
            BoolField = true,
            DateTimeField = new DateTime(2025, 1, 15, 10, 30, 0)
          }
        };
        owner1.JsonArrayField.Add(new TestJsonPoco {
          StringField = "item1", IntField = 1, DecimalField = 10.5m, BoolField = true,
          DateTimeField = new DateTime(2025, 1, 1)
        });
        owner1.JsonArrayField.Add(new TestJsonPoco {
          StringField = "item2", IntField = 2, DecimalField = 20.5m, BoolField = false,
          DateTimeField = new DateTime(2025, 2, 1)
        });
        owner1.JsonArrayField.Add(new TestJsonPoco {
          StringField = "item3", IntField = 1, DecimalField = 30.0m, BoolField = true,
          DateTimeField = new DateTime(2025, 3, 1)
        });

        var owner2 = new JsonOwner {
          Name = "Owner2",
          NumericValue = 20,
          JsonField = new TestJsonPoco {
            StringField = "world",
            IntField = 2,
            DecimalField = 200.75m,
            BoolField = false,
            DateTimeField = new DateTime(2025, 6, 20, 14, 0, 0)
          }
        };
        owner2.JsonArrayField.Add(new TestJsonPoco {
          StringField = "itemA", IntField = 2, DecimalField = 5.25m, BoolField = true,
          DateTimeField = new DateTime(2025, 4, 1)
        });
        owner2.JsonArrayField.Add(new TestJsonPoco {
          StringField = "itemB", IntField = 3, DecimalField = 15.75m, BoolField = false,
          DateTimeField = new DateTime(2025, 5, 1)
        });

        var owner3 = new JsonOwner {
          Name = "Owner3",
          NumericValue = 1,
          JsonField = new TestJsonPoco {
            StringField = "empty",
            IntField = 3,
            DecimalField = 0m,
            BoolField = false,
            DateTimeField = new DateTime(2025, 12, 31)
          }
        };
        // owner3 has an empty JsonArrayField

        var multi = new MultiJsonOwner {
          FirstJson = new TestJsonPoco {
            StringField = "first", IntField = 10, DecimalField = 1.1m,
            BoolField = true, DateTimeField = new DateTime(2025, 1, 1)
          },
          SecondJson = new AnotherJsonPoco { Name = "tag1", Value = 3.14 }
        };
        multi.Items.Add(new TestJsonPoco {
          StringField = "multiItem1", IntField = 5, DecimalField = 50m,
          BoolField = true, DateTimeField = new DateTime(2025, 6, 1)
        });
        multi.Tags.Add(new AnotherJsonPoco { Name = "tagA", Value = 1.0 });
        multi.Tags.Add(new AnotherJsonPoco { Name = "tagB", Value = 2.0 });

        t.Complete();
      }
    }

    #region 1. SQL column type tests

    [Test]
    [RequireSqlServer(MinVersion = "17")]
    public void JsonFieldColumnTypeIsJsonOnV17()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var typeName = GetColumnTypeName(session, "JsonField");
        Assert.AreEqual("json", typeName.ToLowerInvariant(),
          "JsonField should be stored as 'json' type on SQL Server v17+");
      }
    }

    [Test]
    [RequireSqlServer(MinVersion = "17")]
    public void JsonArrayFieldColumnTypeIsJsonOnV17()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var typeName = GetColumnTypeName(session, "JsonArrayField");
        Assert.AreEqual("json", typeName.ToLowerInvariant(),
          "JsonArrayField should be stored as 'json' type on SQL Server v17+");
      }
    }

    [Test]
    [RequireSqlServer(MaxVersion = "16")]
    public void JsonFieldColumnTypeIsNvarcharOnPreV17()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var typeName = GetColumnTypeName(session, "JsonField");
        Assert.AreEqual("nvarchar", typeName.ToLowerInvariant(),
          "JsonField should be stored as 'nvarchar' type on SQL Server before v17");
      }
    }

    [Test]
    [RequireSqlServer(MaxVersion = "16")]
    public void JsonArrayFieldColumnTypeIsNvarcharOnPreV17()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var typeName = GetColumnTypeName(session, "JsonArrayField");
        Assert.AreEqual("nvarchar", typeName.ToLowerInvariant(),
          "JsonArrayField should be stored as 'nvarchar' type on SQL Server before v17");
      }
    }

    private string GetColumnTypeName(Session session, string columnName)
    {
      var accessor = session.Services.Demand<DirectSqlAccessor>();
      var command = accessor.CreateCommand();
      command.CommandText = $@"
        SELECT t.name
        FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        JOIN sys.tables tbl ON c.object_id = tbl.object_id
        WHERE tbl.name LIKE '%JsonOwner' AND c.name = '{columnName}'";
      using (var reader = command.ExecuteReader()) {
        Assert.IsTrue(reader.Read(), $"Column '{columnName}' not found in JsonOwner table");
        return reader.GetString(0);
      }
    }

    #endregion

    #region 2. Change tracking tests

    [Test]
    public void JsonFieldChangeTrackingTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;

        // Modify JSON field
        owner.JsonField = new TestJsonPoco {
          StringField = "modified",
          IntField = 999,
          DecimalField = 0.01m,
          BoolField = false,
          DateTimeField = new DateTime(2026, 1, 1)
        };
        t.Complete();
      }

      // Verify in new session
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("modified", owner.JsonField.StringField);
        Assert.AreEqual(999, owner.JsonField.IntField);
        Assert.AreEqual(0.01m, owner.JsonField.DecimalField);
        Assert.AreEqual(false, owner.JsonField.BoolField);
        Assert.AreEqual(new DateTime(2026, 1, 1), owner.JsonField.DateTimeField);
      }
    }

    [Test]
    public void JsonFieldSetToNullTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        owner.JsonField = null;
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.IsNull(owner.JsonField);
      }
    }

    [Test]
    public void JsonTypeArrayAddTrackingTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        var countBefore = owner.JsonArrayField.Count;
        Assert.AreEqual(3, countBefore);

        owner.JsonArrayField.Add(new TestJsonPoco {
          StringField = "newItem", IntField = 42, DecimalField = 99.99m,
          BoolField = true, DateTimeField = new DateTime(2026, 6, 15)
        });
        Assert.AreEqual(4, owner.JsonArrayField.Count);
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual(4, owner.JsonArrayField.Count);
        var newItem = owner.JsonArrayField[3];
        Assert.AreEqual("newItem", newItem.StringField);
        Assert.AreEqual(42, newItem.IntField);
        Assert.AreEqual(99.99m, newItem.DecimalField);
      }
    }

    [Test]
    public void JsonTypeArrayRemoveTrackingTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        var itemToRemove = owner.JsonArrayField.First(i => i.StringField == "item2");
        var removed = owner.JsonArrayField.Remove(itemToRemove);
        Assert.IsTrue(removed);
        Assert.AreEqual(2, owner.JsonArrayField.Count);
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual(2, owner.JsonArrayField.Count);
        Assert.IsFalse(owner.JsonArrayField.Any(i => i.StringField == "item2"));
      }
    }

    [Test]
    public void JsonTypeArrayClearTrackingTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        owner.JsonArrayField.Clear();
        Assert.AreEqual(0, owner.JsonArrayField.Count);
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual(0, owner.JsonArrayField.Count);
      }
    }

    [Test]
    public void JsonTypeArrayIndexerSetTrackingTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        owner.JsonArrayField[0] = new TestJsonPoco {
          StringField = "replaced", IntField = 777, DecimalField = 0m,
          BoolField = false, DateTimeField = new DateTime(2026, 3, 1)
        };
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("replaced", owner.JsonArrayField[0].StringField);
        Assert.AreEqual(777, owner.JsonArrayField[0].IntField);
      }
    }

    [Test]
    public void JsonFieldInnerPropertyChangeTrackedTest()
    {
      // Modifying inner properties of an existing JsonType instance
      // is automatically detected via snapshot comparison before persist.
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;

        // Mutate inner property directly — this IS tracked via snapshot comparison
        owner.JsonField.StringField = "mutated_directly";

        t.Complete();
      }

      // Verify: the inner mutation WAS persisted
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("mutated_directly", owner.JsonField.StringField,
          "Inner property mutation should be tracked via snapshot comparison");
      }
    }

    [Test]
    public void JsonFieldInnerPropertyChangeTrackedViaReassignmentTest()
    {
      // The correct way to change inner properties: read, modify, reassign.
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;

        // Read current, modify, reassign
        var json = owner.JsonField;
        var updated = new TestJsonPoco {
          StringField = "properly_updated",
          IntField = json.IntField,
          DecimalField = json.DecimalField,
          BoolField = json.BoolField,
          DateTimeField = json.DateTimeField
        };
        owner.JsonField = updated;
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("properly_updated", owner.JsonField.StringField,
          "Reassigning the whole JsonField should persist inner property changes");
        // Other fields should be preserved
        Assert.AreEqual(1, owner.JsonField.IntField);
        Assert.AreEqual(100.50m, owner.JsonField.DecimalField);
      }
    }

    [Test]
    public void JsonTypeArrayItemInnerPropertyChangeTrackedTest()
    {
      // Modifying inner properties of an existing array item
      // is automatically detected via snapshot comparison before persist.
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        Assert.AreEqual("item1", owner.JsonArrayField[0].StringField);

        // Mutate item's inner property directly — this IS tracked
        owner.JsonArrayField[0].StringField = "mutated_item";

        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("mutated_item", owner.JsonArrayField[0].StringField,
          "Inner property mutation on array item should be tracked via snapshot comparison");
      }
    }

    [Test]
    public void JsonTypeArrayItemInnerPropertyChangeTrackedViaIndexerTest()
    {
      // The correct way to change an array item's inner property:
      // replace the item via the indexer.
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;
        var original = owner.JsonArrayField[0];

        // Replace via indexer to trigger change tracking
        owner.JsonArrayField[0] = new TestJsonPoco {
          StringField = "properly_updated_item",
          IntField = original.IntField,
          DecimalField = original.DecimalField,
          BoolField = original.BoolField,
          DateTimeField = original.DateTimeField
        };
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("properly_updated_item", owner.JsonArrayField[0].StringField,
          "Replacing array item via indexer should persist inner property changes");
        Assert.AreEqual(1, owner.JsonArrayField[0].IntField);
      }
    }

    [Test]
    public void JsonFieldReadWithoutModifyDoesNotTriggerUpdateTest()
    {
      // Reading a JSON field without modifying it should not cause a spurious update.
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;

        // Read inner properties without modifying them
        var s = owner.JsonField.StringField;
        var i = owner.JsonField.IntField;
        var d = owner.JsonField.DecimalField;
        var b = owner.JsonField.BoolField;
        var dt = owner.JsonField.DateTimeField;
        var arr = owner.JsonArrayField[0].StringField;

        // Entity should NOT be marked as modified
        Assert.AreEqual(PersistenceState.Synchronized, owner.PersistenceState,
          "Reading JSON field without modifying should not mark entity as Modified");
        t.Complete();
      }

      // Verify original values are still intact
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("hello", owner.JsonField.StringField);
        Assert.AreEqual(1, owner.JsonField.IntField);
        Assert.AreEqual("item1", owner.JsonArrayField[0].StringField);
      }
    }

    [Test]
    public void MultipleJsonFieldChangesInSameTransactionTest()
    {
      int ownerId;
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Name == "Owner1");
        ownerId = owner.Id;

        // Modify JSON field
        owner.JsonField = new TestJsonPoco {
          StringField = "updated", IntField = 50, DecimalField = 50m,
          BoolField = true, DateTimeField = new DateTime(2026, 1, 1)
        };

        // Modify JSON array
        owner.JsonArrayField.Add(new TestJsonPoco {
          StringField = "added", IntField = 60, DecimalField = 60m,
          BoolField = false, DateTimeField = new DateTime(2026, 2, 1)
        });

        // Also modify entity scalar field
        owner.Name = "Owner1Updated";
        owner.NumericValue = 999;

        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var owner = session.Query.All<JsonOwner>().First(o => o.Id == ownerId);
        Assert.AreEqual("updated", owner.JsonField.StringField);
        Assert.AreEqual(50, owner.JsonField.IntField);
        Assert.AreEqual(4, owner.JsonArrayField.Count);
        Assert.AreEqual("added", owner.JsonArrayField[3].StringField);
        Assert.AreEqual("Owner1Updated", owner.Name);
        Assert.AreEqual(999, owner.NumericValue);
      }
    }

    #endregion
  }
}
