using DataObjectNotation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class Basics
    {
        [TestMethod]
        public void ListOfItems_Works()
        {
            var input = @"Apples,Bananas,Cherries";
            var expected = input.Split(',').ToList();

            var result = DON.Parse(input);
            AssertChildren(result, expected);

            result = DON.Parse("Apples\nBananas\nCherries");
            AssertChildren(result, expected);
        }

        [TestMethod]
        public void ListOfItems_TrailingLeading_Works()
        {
            var input = @"Apples,Bananas,Cherries,";
            var expected = new List<string>()
            {
                "Apples",
                "Bananas",
                "Cherries"
            };

            var result = DON.Parse(input);
            AssertChildren(result, expected);

            input = @",Apples,Bananas,Cherries";
            result = DON.Parse(input);
            AssertChildren(result, expected);
        }

        [TestMethod]
        public void ItemWithProperties_Works()
        {
            var input = "Apple(small,red,ounces=5.2)";
            var expected = new Dictionary<string, object>()
            {
                {"small", null },
                {"red", null },
                {"ounces", "5.2"}
            };

            var result = DON.Parse(input);
            Assert.AreEqual(1, result.Children.Count);
            AssertProperties(result.Children[0], expected);

            // Append more items and it should still work
            input += ",Bananas,Cherries";
            result = DON.Parse(input);
            Assert.AreEqual(3, result.Children.Count);
        }

        [TestMethod]
        public void ItemWithChildren_Works()
        {
            var result = DON.Parse("Apple{One,Two,Three}");
            Assert.AreEqual(3, result.Children[0].Children.Count);
        }

        [TestMethod]
        public void ItemWithPropertiesAndChilder_Works()
        {
            var result = DON.Parse("Apple( small, red, ounces=5.2){One,Two,Three}");
            var expectedChildren = new List<string>()
            {
                "One",
                "Two",
                "Three"
            };
            var expectedProperties = new Dictionary<string, object>()
            {
                {"small", null },
                {"red", null },
                {"ounces", "5.2"}
            };
            Assert.AreEqual(1, result.Children.Count);
            Assert.AreEqual("Apple", result.Children[0].Name);
            AssertChildren(result.Children[0], expectedChildren);
            AssertProperties(result.Children[0], expectedProperties);

            result = DON.Parse("Apple(small, red, ounces=5.2)\n{\nOne\nTwo\nThree\n}");
            AssertChildren(result.Children[0], expectedChildren);
            AssertProperties(result.Children[0], expectedProperties);

            result = DON.Parse("Apple(small, red, ounces=5.2)\n{\n\tOne\n\tTwo\n\tThree\n}");
            AssertChildren(result.Children[0], expectedChildren);
            AssertProperties(result.Children[0], expectedProperties);
        }

        [TestMethod]
        public void LeadingSpaceIgnored_Works()
        {
            var result = DON.Parse(" Apples,  Bananas,   \tCherries");
            var expected = new List<string>()
            {
                "Apples",
                "Bananas",
                "Cherries"
            };
            AssertChildren(result, expected);
        }

        [TestMethod]
        public void Nested_Works()
        {
            var input = @"
Apple(red,small)
{
    One(number,1)
    {
        Foo
        Bar
        Baz
    }
    Two(number, 2)
}
";
            var result = DON.Parse(input);
            Assert.AreEqual(1, result.Children.Count);
            Assert.AreEqual(2, result.Children[0].Children.Count);
            Assert.AreEqual(3, result.Children[0].Children[0].Children.Count);

            var expectedChildren = new List<string>()
            {
                "Foo",
                "Bar",
                "Baz"
            };
            AssertChildren(result.Children[0].Children[0], expectedChildren);
        }

        [TestMethod]
        public void FancyLayout_Works()
        {
            var input = @"
Raspberry
(
    type=fruit
    color=red
    text=The raspberry is the edible fruit of a multitude of plant species in the genus Rubus of the rose family, most of which are in the subgenus Idaeobatus; the name also applies to these plants themselves.
)
{
    Bug1
    Bug2
}
";
            var result = DON.Parse(input);
            Assert.AreEqual(1, result.Children.Count);
            Assert.AreEqual(3, result.Children[0].Properties.Count);
            Assert.AreEqual(2, result.Children[0].Children.Count);
        }

        [TestMethod]
        public void Escaping_Works()
        {
            var input = @"
Menu(id=file,value=File)
{
	popup
	{
		menuitem(value=New, onclick=||CreateNewDoc()||)
		menuitem(value=Open, onclick=||CreateNewDoc()||)
		menuitem(value=||Close, everything!||, onclick=||CreateNewDoc()||)
	}
    ||This is some text that
just continues on for days with its built in line breaks
and only one thing stops it||
}
";
            var result = DON.Parse(input);
            Assert.AreEqual(2, result.Children[0].Children.Count);
            var popup = result.Children[0].Children[0];
            Assert.AreEqual(3, popup.Children.Count);
            foreach (var child in popup.Children)
            {
                Assert.AreEqual(2, child.Properties.Count);
            }
        }

        private void AssertChildren(DataObject obj, List<string> expectedNames)
        {
            Assert.AreEqual(expectedNames.Count, obj.Children.Count);
            for (int i = 0; i < expectedNames.Count; i++)
            {
                Assert.AreEqual(expectedNames[i], obj.Children[i].Name, string.Join(',', obj.Children.Select(o => o.Name)));
            }
        }

        private void AssertProperties(DataObject obj, Dictionary<string, object> expected)
        {
            Assert.AreEqual(expected.Count, obj.Properties.Count);

            foreach (var kvp in expected)
            {
                Assert.IsTrue(obj.Properties.ContainsKey(kvp.Key), $"Missing property {kvp.Key}");
                if (kvp.Value != null)
                {
                    Assert.AreEqual(kvp.Value.ToString(), obj.Properties[kvp.Key].ToString());
                }
            }
        }
    }
}
