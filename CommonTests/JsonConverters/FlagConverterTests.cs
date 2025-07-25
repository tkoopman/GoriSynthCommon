using System.Collections;

using Newtonsoft.Json;

using Noggog;

namespace CommonTests.JsonConverters
{
    public class FlagConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.FlagConverter()], },
            new() { Converters = [new Common.JsonConverters.FlagConverter() { WriteAsType = Common.JsonConverters.FlagConverter.WriteAs.Int }] },
            new() { Converters = [new Common.JsonConverters.FlagConverter() { WriteAsType = Common.JsonConverters.FlagConverter.WriteAs.Array }] },
            ];

        [Flags]
        public enum TestFlags
        {
            None = 0,
            Flag1 = 1 << 0,
            Flag2 = 1 << 1,
            Flag3 = 1 << 2,
            All = Flag1 | Flag2 | Flag3
        }

        [Theory]
        [ClassData(typeof(Data_FlagConverter))]
        public void Test_FlagConverter (string jsonInput, TestFlags expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<TestFlags>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<TestFlags>(serialized, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        public class Data_FlagConverter : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator ()
            {
                yield return new object[] { @"""Flag1""", TestFlags.Flag1, new List<string> { @"""Flag1""", "1", """["Flag1"]""" } };
                yield return new object[] { @"""Flag1,Flag2""", TestFlags.Flag1 | TestFlags.Flag2, new List<string> { @"""Flag1, Flag2""", "3", """["Flag1","Flag2"]""" } };
                yield return new object[] { @"""Flag1,Flag2,Flag3""", TestFlags.All, new List<string> { @"""All""", "7", """["All"]""" } };
                yield return new object[] { """["Flag1","Flag2","Flag3"]""", TestFlags.All, new List<string> { @"""All""", "7", """["All"]""" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}