using System.Collections;

using Newtonsoft.Json;

using Noggog;

namespace CommonTests.JsonConverters
{
    public class MemorySliceByteConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.MemorySliceByteConverter()] },
            new() { Converters = [new Common.JsonConverters.MemorySliceByteConverter() { GroupSize = 1, GroupsPerString = 8 }] },
            new() { Converters = [new Common.JsonConverters.MemorySliceByteConverter() { WriteAsByteArray = true }] },
            ];

        [Theory]
        [ClassData(typeof(Data_MemorySliceByteConverter))]
        public void Test_MemorySliceByteConverter (string jsonInput, MemorySlice<byte> expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<MemorySlice<byte>>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<MemorySlice<byte>>(serialized, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        private class Data_MemorySliceByteConverter : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator ()
            {
                yield return new object[] { "[1,2,3,4,5]", new MemorySlice<byte>([1, 2, 3, 4, 5]), new List<string>() { """["01020304 05"]""", """["01 02 03 04 05"]""", "[1,2,3,4,5]" } };
                yield return new object[] { "[0xFF,2,3,4,5]", new MemorySlice<byte>([0xFF, 2, 3, 4, 5]), new List<string>() { """["FF020304 05"]""", """["FF 02 03 04 05"]""", "[255,2,3,4,5]" } };
                yield return new object[] { """["12345678",2,3,4,5]""", new MemorySlice<byte>([0x12, 0x34, 0x56, 0x78, 2, 3, 4, 5]), new List<string>() { """["12345678 02030405"]""", """["12 34 56 78 02 03 04 05"]""", "[18,52,86,120,2,3,4,5]" } };
                yield return new object[] { """["12345678",2,3,4,5,6]""", new MemorySlice<byte>([0x12, 0x34, 0x56, 0x78, 2, 3, 4, 5, 6]), new List<string>() { """["12345678 02030405 06"]""", """["12 34 56 78 02 03 04 05","06"]""", "[18,52,86,120,2,3,4,5,6]" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}