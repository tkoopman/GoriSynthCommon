using System.Collections;
using System.Drawing;

using Newtonsoft.Json;

namespace CommonTests.JsonConverters
{
    public class ColorConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.ColorConverter()] },
            new() { Converters = [new Common.JsonConverters.ColorConverter() { WriteAsType = Common.JsonConverters.ColorConverter.WriteAs.Int }] },
            new() { Converters = [new Common.JsonConverters.ColorConverter() { WriteAsType = Common.JsonConverters.ColorConverter.WriteAs.Array }] },
            ];

        [Theory]
        [ClassData(typeof(Data_ColorConverter))]
        public void Test_ColorConverter (string jsonInput, Color expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<Color>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<Color>(serialized, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        [Theory]
        [ClassData(typeof(Data_ColorConverter_Named))]
        public void Test_ColorConverter_Name (string jsonInput, List<Color> expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            Assert.Equal(2, expected.Count);

            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<Color>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected[0], result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<Color>(serialized, _serializerSettings[i]);
                if (i == 0)
                    Assert.Equal(expected[0], result);
                else
                    Assert.Equal(expected[1], result);
            }
        }

        public class Data_ColorConverter : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[]
                {
                    @"""#FF012345""", Color.FromArgb(0xFF, 0x01, 0x23, 0x45), new List<string>() { @"""#012345""", "-16702651", "[1,35,69]" }
                };
                yield return new object?[]
                {
                    @"""#FFFF0000""", Color.FromArgb(0xFF, 0xFF, 0x00, 0x00), new List<string>() { @"""#FF0000""", "-65536", "[255,0,0]" }
                };
                yield return new object?[]
                {
                    @"""0x00FF0000""", Color.FromArgb(0x00, 0xFF, 0x00, 0x00), new List<string>() { @"""#00FF0000""", "16711680", "[0,255,0,0]" }
                };
                yield return new object?[]
                {
                    @"""0xFF0000""", Color.FromArgb(0xFF, 0xFF, 0x00, 0x00), new List < string >() { @"""#FF0000""", "-65536", "[255,0,0]" }
                };
                yield return new object?[]
                {
                    @"""#FF0000""", Color.FromArgb(0xFF, 0xFF, 0x00, 0x00), new List < string >() { @"""#FF0000""", "-65536", "[255,0,0]" }
                };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }

        public class Data_ColorConverter_Named : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[]
                {
                    @"""red""", new List<Color>() { Color.FromName("Red"), Color.FromArgb(0xFF, 0xFF, 0x00, 0x00) }, new List<string>() { @"""Red""", "-65536", "[255,0,0]" }
                };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}