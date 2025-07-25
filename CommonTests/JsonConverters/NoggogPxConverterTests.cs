using System.Collections;

using Newtonsoft.Json;

using Noggog;

namespace CommonTests.JsonConverters
{
    public class NoggogPxConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.NoggogPxConverter()] },
            new() { Converters = [new Common.JsonConverters.NoggogPxConverter() { WriteAsType = Common.JsonConverters.NoggogPxConverter.WriteAs.Object }] },
            ];

        [Theory]
        [ClassData(typeof(Data_NoggogPxConverter))]
        public void Test_NoggogPxConverter (string jsonInput, object expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                object? result = JsonConvert.DeserializeObject(jsonInput, expected.GetType(), _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject(serialized, expected.GetType(), _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        public class Data_NoggogPxConverter : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[] { """{"Y":2,"X":1}""", new P2Int(1, 2), new List<string>() { """[1,2]""", """{"x":1,"y":2}""" } };
                yield return new object?[] { """{"Y":2,"X":1, "Z":3}""", new P3Int(1, 2, 3), new List<string>() { """[1,2,3]""", """{"x":1,"y":2,"z":3}""" } };
                yield return new object?[] { """{"Y":2,"X":1, "Z":3}""", new P3Float(1, 2, 3), new List<string>() { """[1.0,2.0,3.0]""", """{"x":1.0,"y":2.0,"z":3.0}""" } };
                yield return new object?[] { """{"Y":2.5,"X":1, "Z":3}""", new P3Float(1, 2.5f, 3), new List<string>() { """[1.0,2.5,3.0]""", """{"x":1.0,"y":2.5,"z":3.0}""" } };
                yield return new object?[] { """{"Y":2.5,"X":1, "Z":3}""", new P3Int(1, 2, 3), new List<string>() { """[1,2,3]""", """{"x":1,"y":2,"z":3}""" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}