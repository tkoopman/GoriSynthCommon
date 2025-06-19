using System.Collections;

using Common;

namespace CommonTests
{
    public class SafeAnyTests
    {
        [Theory]
        [ClassData(typeof(TestData))]
        public void SafeAny<TSource> (IEnumerable<TSource> list, bool expect)
            => Assert.Equal(expect, list.SafeAny());

        [Theory]
        [ClassData(typeof(TestDataPredicate))]
        public void SafeAnyPredicate<TSource> (IEnumerable<TSource>? list, Func<TSource, bool> predicate, bool expect) => Assert.Equal(expect, list.SafeAny(predicate));

        public class TestData : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[] { (List<int>?)null, false };
                yield return new object?[] { new List<int>(), false };
                yield return new object?[] { new List<int> { 1, 2, 3 }, true };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }

        public class TestDataPredicate : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                Func<int, bool> predicate = (x) => x > 0;
                yield return new object?[] { (List<int>?)null, predicate, false };
                yield return new object?[] { new List<int>(), predicate, false };
                yield return new object?[] { new List<int> { 1, 2, 3 }, predicate, true };
                yield return new object?[] { new List<int> { -1, -2, -3 }, predicate, false };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}