using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLite.Net.Attributes;

namespace SQLite.Net.Tests
{
    [TestFixture]
    public class DeleteTest
    {
        private class TestTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int Datum { get; set; }

            public string Test { get; set; }
        }

        private class TestTableMulti
        {
            [PrimaryKey]
            public string Id { get; set; }

            [PrimaryKey, Indexed]
            public string Id2 { get; set; }

            public int Datum { get; set; }
        }


        private const int Count = 100;

        private SQLiteConnection CreateDb()
        {
            var db = new TestDb();
            db.CreateTable<TestTable>();
            IEnumerable<TestTable> items = from i in Enumerable.Range(0, Count)
                select new TestTable
                {
                    Datum = 1000 + i, Test = "Hello World"
                };
            db.InsertAll(items);
            Assert.AreEqual(Count, db.Table<TestTable>().Count());

            db.CreateTable<TestTableMulti>();
            var items2 = from i in Enumerable.Range(0, Count)
                select new TestTableMulti
                {
                    Id = "t" + i,
                    Id2 = "t" + i,
                    Datum = 1000 + i
                };
            db.InsertAll(items2);
            Assert.AreEqual(Count, db.Table<TestTableMulti>().Count());
            
            return db;
        }

        [Test]
        public void DeleteAll()
        {
            SQLiteConnection db = CreateDb();

            int r = db.DeleteAll<TestTable>();

            Assert.AreEqual(Count, r);
            Assert.AreEqual(0, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeleteIn()
        {
            using (var db = CreateDb())
            {
                var ids = db.Table<TestTable>().Take(5).Select(t => t.Id).ToArray();
                var r = db.DeleteIn<TestTable>(ids);

                Assert.AreEqual(5, r);
                Assert.AreEqual(Count - 5, db.Table<TestTable>().Count());

                //Test with an empty array
                ids = new int[0];
                db.DeleteIn<TestTable>(ids);
            }
        }

        [Test]
        public void DeleteEntityOne()
        {
            SQLiteConnection db = CreateDb();

            int r = db.Delete(db.Get<TestTable>(1));

            Assert.AreEqual(1, r);
            Assert.AreEqual(Count - 1, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeletePKNone()
        {
            SQLiteConnection db = CreateDb();

            int r = db.Delete<TestTable>(new []{348597});

            Assert.AreEqual(0, r);
            Assert.AreEqual(Count, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeletePKOne()
        {
            SQLiteConnection db = CreateDb();

            int r = db.Delete<TestTable>(new []{1});

            Assert.AreEqual(1, r);
            Assert.AreEqual(Count - 1, db.Table<TestTable>().Count());
        }

        [Test]
        public void DeleteMultiPK()
        {
            var db = CreateDb();

            //Full key
            var s1 = "t12";
            var s2 = "t12";
            var r = db.Delete<TestTableMulti>(new []{s1,s2});
            Assert.AreEqual(1, r);
            Assert.AreEqual(Count-1, db.Table<TestTableMulti>().Count());

            //Partial key
            var r2 = db.Delete<TestTableMulti>(new []{"t13"});
            Assert.AreEqual(1, r2);
            Assert.AreEqual(Count-2, db.Table<TestTableMulti>().Count());
        }
    }
}
