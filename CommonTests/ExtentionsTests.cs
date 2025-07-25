using System.Collections;

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
        [ClassData(typeof(Data_IsNullable))]
        public void Test_IsNullable (Type type, bool expected)
        {
            bool isNullable = type.IsNullable();
            Assert.Equal(expected, isNullable);
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
        [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, true)]
        [InlineData(new[] { 1, 2, 3 }, null, false)]
        [InlineData(null, new[] { 1, 2, 3 }, false)]
        [InlineData(null, null, true)]
        [InlineData(null, new int[] { }, false)]
        [InlineData(new string[] { }, null, false)]
        public void Test_SafeSequenceEqual<T> (T[]? first, T[]? second, bool expected) => Assert.Equal(expected, first.SafeSequenceEqual(second));

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, true)]
        [InlineData(new[] { 1, 2, 3 }, null, false)]
        [InlineData(null, new[] { 1, 2, 3 }, false)]
        [InlineData(null, null, true)]
        [InlineData(null, new int[] { }, true)]
        [InlineData(new string[] { }, null, true)]
        public void Test_SafeSequenceEqualNullEmpty<T> (T[]? first, T[]? second, bool expected) => Assert.Equal(expected, first.SafeSequenceEqualNullEmpty(second));

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

        private class Data_IsNullable : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator ()
            {
                yield return new object[] { typeof(int), false };
                yield return new object[] { typeof(int?), true };
                yield return new object[] { typeof(string), false };
                yield return new object[] { typeof(List<int>), false };
                yield return new object[] { typeof(List<int?>), false };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}