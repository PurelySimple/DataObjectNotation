using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataObjectNotation.UnitTest
{
    [TestClass]
    public class Deserializer
    {
        [TestMethod]
        public void GetPropertyAs_Works()
        {
            var obj = DON.Parse("(apple=one, banana=2, cherry=3.4)");

            Assert.AreEqual("one", obj.GetPropertyAs<string>("apple"));
            Assert.AreEqual(2, obj.GetPropertyAs<int>("banana"));
            Assert.AreEqual(3.4f, obj.GetPropertyAs<float>("cherry"), 0.000001f);

            Assert.IsNull(obj.GetPropertyAs<string>("doesntExist"));
        }

        [TestMethod]
        public void IntegerArray_Works()
        {
            var obj = DON.Parse("1,2,3,4,5,6,7,8");
            var result = obj.Deserialize<int[]>();

            Assert.AreEqual(8, result.Length);
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(i + 1, result[i]);
            }
        }

        [TestMethod]
        public void FloatArray_Works()
        {
            var obj = DON.Parse("1,2,3,4.2,5,6.0,7,8.9");

            var result = obj.Deserialize<float[]>();

            Assert.AreEqual(8f, result.Length);
            Assert.AreEqual(2f, result[1]);
            Assert.AreEqual(4.2f, result[3], 0.000001f);
            Assert.AreEqual(8.9f, result[7], 0.000001f);
        }

        [TestMethod]
        public void StringArray_Works()
        {
            var obj = DON.Parse("apple,banana,cherry");
            var result = obj.Deserialize<string[]>();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("banana", result[1]);
        }

        [TestMethod]
        public void HashSet_String_Works()
        {
            // Should support both property and child way
            var result = DON.Parse("(apple,banana,cherry)").Deserialize<HashSet<string>>();

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains("apple"));
            Assert.IsTrue(result.Contains("banana"));
            Assert.IsTrue(result.Contains("cherry"));

            result = DON.Parse("apple,banana,cherry").Deserialize<HashSet<string>>();

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Contains("apple"));
            Assert.IsTrue(result.Contains("banana"));
            Assert.IsTrue(result.Contains("cherry"));
        }

        [TestMethod]
        public void Dictionary_String_Works()
        {
            var result = DON.Parse("(apple=one,banana=two,cherry=three)").Deserialize<Dictionary<string,string>>();

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result["apple"] == "one");
            Assert.IsTrue(result["banana"] == "two");
            Assert.IsTrue(result["cherry"] == "three");
        }

        [TestMethod]
        public void List_String_Works()
        {
            var result = DON.Parse("apple,banana,cherry").Deserialize<List<string>>();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("apple", result[0]);
            Assert.AreEqual("banana", result[1]);
            Assert.AreEqual("cherry", result[2]);
        }

        [TestMethod]
        public void List_Integer_Works()
        {
            var result = DON.Parse("0,1,2,3,4,5").Deserialize<List<int>>();

            Assert.AreEqual(6, result.Count);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(i, result[i]);
            }
        }

        [TestMethod]
        public void CustomClass_Note_Works()
        {
            var input = @"
(
To=Alice
From=Bob
Title=Hello
Body=How are you doing?
)
";
            var result = DON.Parse(input).Deserialize<Note>();

            Assert.AreEqual("Alice", result.To);
            Assert.AreEqual("Bob", result.From);
            Assert.AreEqual("Hello", result.Title);
            Assert.AreEqual("How are you doing?", result.Body);
        }

        [TestMethod]
        public void CustomClass_MixedPropertyAndFields_Works()
        {
            var input = @"
(
    Integer=1234
    Number=12.34
    Character=A
    Text=ABCDEFG
)
{
    BigNumbers
    {
        123456789
        987654321
    }
    Names
    {
        Apples
        Bananas
        Cherries
    }
}
";
            var result = DON.Parse(input).Deserialize<MixedPropertyAndFields>();
            Assert.AreEqual(1234, result.Integer);
            Assert.AreEqual(12.34f, result.Number, 0.000001f);
            Assert.AreEqual('A', result.Character);
            Assert.AreEqual("ABCDEFG", result.Text);

            Assert.AreEqual(2, result.BigNumbers.Length);
            Assert.AreEqual(123456789, result.BigNumbers[0]);
            Assert.AreEqual(987654321, result.BigNumbers[1]);

            Assert.AreEqual(3, result.Names.Count);
            Assert.AreEqual("Apples", result.Names[0]);
            Assert.AreEqual("Bananas", result.Names[1]);
            Assert.AreEqual("Cherries", result.Names[2]);
        }

        [TestMethod]
        public void NestedCustomClasses_Works()
        {
            var input = @"
(Id=1)
{
    Child(Id=2)
    {
        Child(Id=3)
    }
}
";
            var result = DON.Parse(input).Deserialize<TestContainer>();
            Assert.AreEqual(1, result.Id);
            Assert.IsNotNull(result.Child);
            Assert.AreEqual(2, result.Child.Id);
            Assert.IsNotNull(result.Child.Child);
            Assert.AreEqual(3, result.Child.Child.Id);
        }

        [TestMethod]
        public void ListOfCustomClass_Works()
        {
            var input = @"
{
    Fruit(Text=Apples,Integer=1)
    Fruit(Text=Bananas,Integer=2)
}
";
            var result = DON.Parse(input).Deserialize<List<MixedPropertyAndFields>>();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Apples", result[0].Text);
            Assert.AreEqual(1, result[0].Integer);
            Assert.AreEqual("Bananas", result[1].Text);
            Assert.AreEqual(2, result[1].Integer);
        }

        public class Note
        {
            public string To { get; set; }
            public string From { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
        }

        public class MixedPropertyAndFields
        {
            public int Integer;
            public float Number { get; set; }
            public char Character;
            public string Text { get; set; }
            public long[] BigNumbers;
            public List<string> Names { get; set; }
        }

        public class TestContainer
        {
            public int Id { get; set; }
            public TestContainer Child { get; set; }
        }
    }
}
