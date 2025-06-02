using Common;
using Common.JsonConverters;

using Loqui;

using Newtonsoft.Json;

namespace CommonTests
{
    public class CommonTests
    {
        [Theory]
        [ClassData(typeof(TranslationMaskConverter_TestData))]
        public void TranslationMaskConverter_Tests (string json, ITranslationMask expected)
        {
            var converter = new TranslationMaskConverter();

            // Test boolean input
            object? mask = JsonConvert.DeserializeObject(json, expected.GetType(),
                new JsonSerializerSettings()
                {
                    Converters = [converter],
                }
                );

            Assert.Equal(expected, mask as ITranslationMask, new TranslationMaskConverter_Comparer());
        }

        [Fact]
        public void UVDictionary_BasicTests ()
        {
            var dict = new UVDictionary<string, int>()
            {
                { "a", 1 },
                { "b", 2 },
                { "c", 3 },
            };

            Assert.Equal(3, dict.Count);
            Assert.Equal(1, dict["a"]);
            Assert.Equal(2, dict["b"]);
            Assert.Equal(3, dict["c"]);

            Assert.Equal("a", dict.GetKey(1));
            Assert.Equal("b", dict.GetKey(2));
            Assert.Equal("c", dict.GetKey(3));

            var swapDict = dict.Swap();
            Assert.Equal(3, swapDict.Count);
            Assert.Equal("a", swapDict[1]);
            Assert.Equal("b", swapDict[2]);
            Assert.Equal("c", swapDict[3]);

            Assert.Equal(1, swapDict.GetKey("a"));
            Assert.Equal(2, swapDict.GetKey("b"));
            Assert.Equal(3, swapDict.GetKey("c"));

            Assert.True(dict.ContainsKey("a"));
            Assert.True(dict.ContainsValue(1));

            _ = Assert.Throws<KeyNotFoundException>(() => dict["d"]);
            _ = Assert.Throws<KeyNotFoundException>(() => dict.GetKey(4));

            _ = Assert.Throws<ArgumentException>(() => dict.Add("a", 4));

            dict.Add("d", 4);
            Assert.Equal(4, dict.Count);
            Assert.Equal(4, dict["d"]);
            Assert.Equal("d", dict.GetKey(4));

            Assert.Equal(4, swapDict.Count);
            Assert.Equal("d", swapDict[4]);
            Assert.Equal(4, swapDict.GetKey("d"));

            Assert.True(swapDict.Remove(2));
            Assert.False(swapDict.Remove(5));

            Assert.False(dict.ContainsKey("b"));
        }
    }
}