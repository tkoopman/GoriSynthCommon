using Common;

namespace CommonTests
{
    public class ExtensionsTests
    {
        [Theory]
        [InlineData(new[] { "a", "b", "c" }, new[] { "a", "b", "c" }, new[] { "a", "b", "c" }, 0)]
        [InlineData(new[] { "a", "b", "c" }, new[] { "c", "b", "a" }, new[] { "a", "b", "c" }, 0)]
        [InlineData(new[] { "a", "b", "c" }, new[] { "d", "e", "f" }, new[] { "a", "b", "c", "d", "e", "f" }, 3)]
        [InlineData(new[] { "a", "b", "c" }, new[] { "b", "d", "b", "e" }, new[] { "a", "b", "c", "b", "d", "e" }, 3)]
        [InlineData(new[] { "a", "b", "c" }, new[] { "d", "b", "b", "e" }, new[] { "a", "b", "c", "d", "b", "e" }, 3)]
        public void Test_AddMissing (string[] input, string[] add, string[] expected, int expectedAdded)
        {
            var list = input.ToList();
            int added = list.AddMissing(add);

            Assert.Equal(expectedAdded, added);
            Assert.Equal(expected, list);
        }

        [Theory]
        [InlineData("This is a string", new[] { "This is a string" })]
        [InlineData("This is a string\nAnd another string!", new[] { "This is a string", "And another string!" })]
        [InlineData("This is a string\r\nAnd another string!", new[] { "This is a string", "And another string!" })]
        [InlineData("   This is a string   \n   And another string!   ", new[] { "   This is a string   ", "   And another string!   " })]
        [InlineData("   This is a string   \r\n   And another string!   ", new[] { "   This is a string   ", "   And another string!   " })]
        public void Test_Lines (string input, string[] expected)
        {
            string[] lines = [.. input.Lines()];
            Assert.Equal(expected, lines);
        }

        [Theory]
        [InlineData("thisIsAString", "this Is A String")]
        [InlineData("ThisIsAString", "This Is A String")]
        [InlineData("ThisIsAString followed by more text. letsGo!", "This Is A String followed by more text. lets Go!")]
        [InlineData("ABCCat", "ABC Cat")]
        [InlineData("BOOK", "BOOK")]
        public void Test_SeparateWords (string input, string expected)
        {
            string words = input.SeparateWords();
            Assert.Equal(expected, words);
        }
    }
}