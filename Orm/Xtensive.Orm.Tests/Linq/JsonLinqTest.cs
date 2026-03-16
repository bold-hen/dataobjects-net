// Copyright (C) 2024-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.JsonTypeTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Json")]
  [TestFixture]
  public class JsonLinqTest : AutoBuildTest
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

    #region Server-side Select tests

    [Test]
    public void SelectJsonFieldPropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var strings = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.StringField)
          .ToList();
        Assert.AreEqual(3, strings.Count);
        Assert.AreEqual("hello", strings[0]);
        Assert.AreEqual("world", strings[1]);
        Assert.AreEqual("empty", strings[2]);
      }
    }

    [Test]
    public void SelectJsonFieldIntPropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var ints = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.IntField)
          .ToList();
        Assert.AreEqual(3, ints.Count);
        Assert.AreEqual(1, ints[0]);
        Assert.AreEqual(2, ints[1]);
        Assert.AreEqual(3, ints[2]);
      }
    }

    [Test]
    public void SelectJsonFieldDecimalPropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var decimals = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.DecimalField)
          .ToList();
        Assert.AreEqual(3, decimals.Count);
        Assert.AreEqual(100.50m, decimals[0]);
        Assert.AreEqual(200.75m, decimals[1]);
        Assert.AreEqual(0m, decimals[2]);
      }
    }

    [Test]
    public void SelectJsonFieldBoolPropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var bools = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.BoolField)
          .ToList();
        Assert.AreEqual(3, bools.Count);
        Assert.AreEqual(true, bools[0]);
        Assert.AreEqual(false, bools[1]);
        Assert.AreEqual(false, bools[2]);
      }
    }

    [Test]
    public void SelectJsonFieldDateTimePropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var dates = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField.DateTimeField)
          .ToList();
        Assert.AreEqual(3, dates.Count);
        Assert.AreEqual(new DateTime(2025, 1, 15, 10, 30, 0), dates[0]);
        Assert.AreEqual(new DateTime(2025, 6, 20, 14, 0, 0), dates[1]);
        Assert.AreEqual(new DateTime(2025, 12, 31), dates[2]);
      }
    }

    [Test]
    public void SelectMixedEntityAndJsonFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => new {
            o.Id,
            o.Name,
            o.NumericValue,
            JsonString = o.JsonField.StringField,
            JsonInt = o.JsonField.IntField
          })
          .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Owner1", result[0].Name);
        Assert.AreEqual("hello", result[0].JsonString);
        Assert.AreEqual(1, result[0].JsonInt);
        Assert.AreEqual(10, result[0].NumericValue);
      }
    }

    [Test]
    public void SelectWholeJsonFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonField)
          .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("hello", result[0].StringField);
        Assert.AreEqual(1, result[0].IntField);
      }
    }

    [Test]
    public void SelectWholeJsonArrayFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => o.JsonArrayField)
          .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(3, result[0].Count);
        Assert.AreEqual("item1", result[0][0].StringField);
        Assert.AreEqual(2, result[1].Count);
        Assert.AreEqual(0, result[2].Count);
      }
    }

    [Test]
    public void SelectEntityFieldsPlusJsonAggregationTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => new {
            o.Name,
            JsonString = o.JsonField.StringField,
            ArraySum = o.JsonArrayField.Sum(item => item.DecimalField)
          })
          .ToList();

        Assert.AreEqual(3, result.Count);

        // Owner1: items have DecimalField 10.5, 20.5, 30.0 => sum = 61.0
        Assert.AreEqual("Owner1", result[0].Name);
        Assert.AreEqual("hello", result[0].JsonString);
        Assert.AreEqual(61.0m, result[0].ArraySum);

        // Owner2: items have DecimalField 5.25, 15.75 => sum = 21.0
        Assert.AreEqual("Owner2", result[1].Name);
        Assert.AreEqual(21.0m, result[1].ArraySum);

        // Owner3: empty array => sum = 0
        Assert.AreEqual("Owner3", result[2].Name);
        Assert.AreEqual(0m, result[2].ArraySum);
      }
    }

    [Test]
    public void SelectEntityFieldsPlusJsonArrayCountTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .OrderBy(o => o.Id)
          .Select(o => new {
            o.Name,
            ArrayCount = o.JsonArrayField.Count()
          })
          .ToList();

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(3, result[0].ArrayCount);
        Assert.AreEqual(2, result[1].ArrayCount);
        Assert.AreEqual(0, result[2].ArrayCount);
      }
    }

    #endregion

    #region Server-side Join tests

    [Test]
    public void LeftJoinOnJsonFieldIntKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        // Join: left.JsonField.IntField == right.JsonField.IntField
        var result = session.Query.All<JsonOwner>()
          .LeftJoin(
            session.Query.All<JsonOwner>(),
            left => left.JsonField.IntField,
            right => right.JsonField.IntField,
            (left, right) => new {
              LeftName = left.Name,
              RightName = right.Name,
              Key = left.JsonField.IntField
            })
          .OrderBy(x => x.LeftName).ThenBy(x => x.RightName)
          .ToList();

        // Each owner should match itself (IntField 1->1, 2->2, 3->3)
        Assert.IsTrue(result.Any(r => r.LeftName == "Owner1" && r.RightName == "Owner1"));
        Assert.IsTrue(result.Any(r => r.LeftName == "Owner2" && r.RightName == "Owner2"));
        Assert.IsTrue(result.Any(r => r.LeftName == "Owner3" && r.RightName == "Owner3"));
      }
    }

    [Test]
    public void LeftJoinEntityKeyWithJsonKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        // Join: left.NumericValue (entity field) == right.JsonField.IntField (json field)
        var result = session.Query.All<JsonOwner>()
          .LeftJoin(
            session.Query.All<JsonOwner>(),
            left => left.NumericValue,
            right => right.JsonField.IntField,
            (left, right) => new {
              LeftName = left.Name,
              RightName = right.Name,
              LeftValue = left.NumericValue,
              RightJsonInt = right.JsonField.IntField
            })
          .OrderBy(x => x.LeftName).ThenBy(x => x.RightName)
          .ToList();

        // Owner3 has NumericValue=1, matches Owner1 (IntField=1)
        Assert.IsTrue(result.Any(r => r.LeftName == "Owner3" && r.RightName == "Owner1"));
      }
    }

    [Test]
    public void LeftJoinWithJsonArraySelectManyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        // Join entity with expanded JSON array rows
        var result = session.Query.All<JsonOwner>()
          .LeftJoin(
            session.Query.All<JsonOwner>().SelectMany(o => o.JsonArrayField),
            left => left.JsonField.IntField,
            right => right.IntField,
            (left, right) => new {
              OwnerName = left.Name,
              ArrayItemString = right.StringField
            })
          .OrderBy(x => x.OwnerName).ThenBy(x => x.ArrayItemString)
          .ToList();

        // Owner1.JsonField.IntField = 1, matches array items with IntField=1 (item1, item3)
        Assert.IsTrue(result.Any(r => r.OwnerName == "Owner1" && r.ArrayItemString == "item1"));
        Assert.IsTrue(result.Any(r => r.OwnerName == "Owner1" && r.ArrayItemString == "item3"));
      }
    }

    [Test]
    public void JoinOnJsonFieldStringKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        // Self-join on JsonField.StringField
        var result = session.Query.All<JsonOwner>()
          .LeftJoin(
            session.Query.All<JsonOwner>(),
            left => left.JsonField.StringField,
            right => right.JsonField.StringField,
            (left, right) => new {
              LeftName = left.Name,
              RightName = right.Name,
              JsonString = left.JsonField.StringField
            })
          .OrderBy(x => x.LeftName)
          .ToList();

        // Each should match itself by StringField
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.All(r => r.LeftName == r.RightName));
      }
    }

    #endregion

    #region SelectMany projection tests with JsonTypeArray

    [Test]
    public void SelectManyBasicTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField)
          .ToList();

        // Owner1 has 3 items, Owner2 has 2 items, Owner3 has 0 items
        Assert.AreEqual(5, result.Count);
      }
    }

    [Test]
    public void SelectManyWithSelectStringFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField.Select(item => item.StringField))
          .OrderBy(s => s)
          .ToList();

        Assert.AreEqual(5, result.Count);
        CollectionAssert.Contains(result, "item1");
        CollectionAssert.Contains(result, "item2");
        CollectionAssert.Contains(result, "item3");
        CollectionAssert.Contains(result, "itemA");
        CollectionAssert.Contains(result, "itemB");
      }
    }

    [Test]
    public void SelectManyWithAnonymousProjectionTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            item.StringField,
            item.IntField,
            item.DecimalField
          }))
          .OrderBy(x => x.StringField)
          .ToList();

        Assert.AreEqual(5, result.Count);
        var item1 = result.First(r => r.StringField == "item1");
        Assert.AreEqual(1, item1.IntField);
        Assert.AreEqual(10.5m, item1.DecimalField);
      }
    }

    [Test]
    public void SelectManyWithMixedEntityAndArrayFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            OwnerName = o.Name,
            OwnerId = o.Id,
            item.StringField,
            OwnerJsonInt = o.JsonField.IntField,
          }))
          .OrderBy(x => x.OwnerName).ThenBy(x => x.StringField)
          .ToList();

        Assert.AreEqual(5, result.Count);

        // Check Owner1's items
        var owner1Items = result.Where(r => r.OwnerName == "Owner1").ToList();
        Assert.AreEqual(3, owner1Items.Count);
        Assert.IsTrue(owner1Items.All(i => i.OwnerJsonInt == 1)); // Owner1.JsonField.IntField = 1

        // Check Owner2's items
        var owner2Items = result.Where(r => r.OwnerName == "Owner2").ToList();
        Assert.AreEqual(2, owner2Items.Count);
        Assert.IsTrue(owner2Items.All(i => i.OwnerJsonInt == 2)); // Owner2.JsonField.IntField = 2
      }
    }

    [Test]
    public void SelectManyWithWholeJsonFieldInProjectionTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            item.StringField,
            o.JsonField  // whole JsonType object from outer entity
          }))
          .OrderBy(x => x.StringField)
          .ToList();

        Assert.AreEqual(5, result.Count);
        // item1 comes from Owner1
        var item1 = result.First(r => r.StringField == "item1");
        Assert.AreEqual("hello", item1.JsonField.StringField);
        Assert.AreEqual(1, item1.JsonField.IntField);

        // itemA comes from Owner2
        var itemA = result.First(r => r.StringField == "itemA");
        Assert.AreEqual("world", itemA.JsonField.StringField);
        Assert.AreEqual(2, itemA.JsonField.IntField);
      }
    }

    [Test]
    public void SelectManyWithFilterOnArrayItemTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField)
          .Where(item => item.IntField == 1)
          .ToList();

        // item1 (Owner1) and item3 (Owner1) have IntField = 1
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(r => r.IntField == 1));
      }
    }

    [Test]
    public void SelectManyWithIdInProjectionTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<JsonOwner>()
          .SelectMany(o => o.JsonArrayField.Select(item => new {
            item.StringField,
            o.JsonField,
            o.Id
          }))
          .ToList();

        Assert.AreEqual(5, result.Count);
        // All items should have valid entity IDs
        Assert.IsTrue(result.All(r => r.Id > 0));
      }
    }

    #endregion
  }
}
