using System.Collections;

using Newtonsoft.Json;

using Noggog;

namespace CommonTests.JsonConverters
{
    public class PercentConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.PercentConverter()] },
            new() { Converters = [new Common.JsonConverters.PercentConverter() { WriteAsType = Common.JsonConverters.PercentConverter.WriteAs.Float }] },
            ];

        [Theory]
        [ClassData(typeof(Data_PercentConverter))]
        public void Test_PercentConverter (string jsonInput, Percent expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<Percent>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<Percent>(serialized, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        public class Data_PercentConverter : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator ()
            {
                yield return new object[] { "0", new Percent(0), new List<string>() { @"""0%""", "0.0" } };
                yield return new object[] { "0.50", new Percent(0.50), new List<string>() { @"""50%""", "0.5" } };
                yield return new object[] { "0.99", new Percent(0.99), new List<string>() { @"""99%""", "0.99" } };
                yield return new object[] { "0.2025", new Percent(0.2025), new List<string>() { @"""20.25%""", "0.2025" } };
                yield return new object[] { "1", new Percent(1), new List<string>() { @"""100%""", "1.0" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}