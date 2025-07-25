using System.Collections;

using Mutagen.Bethesda.Plugins;

using Newtonsoft.Json;

namespace CommonTests.JsonConverters
{
    public class FormKeyConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.FormKeyConverter()] },
            new() { Converters = [new Common.JsonConverters.FormKeyConverter() { WriteFormat = Common.JsonConverters.FormKeyConverter.Format.SKSEDefault }] },
            ];

        [Theory]
        [ClassData(typeof(Data_FormKeyConverter))]
        public void Test_FormKeyConverter (string jsonInput, FormKey expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<FormKey>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<FormKey>(serialized, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        public class Data_FormKeyConverter : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[]
                {
                    @"""0x123456:Skyrim.esm""", new FormKey("Skyrim.esm", 0x123456), new List<string>() { @"""123456:Skyrim.esm""", @"""0x123456~Skyrim.esm""" }
                };
                yield return new object?[]
                {
                    @"""0x1a3:Skyrim.esm""", new FormKey("Skyrim.esm", 0x1A3), new List<string>() { @"""0001A3:Skyrim.esm""", @"""0x1A3~Skyrim.esm""" }
                };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}