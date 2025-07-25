using System.Collections;

using Mutagen.Bethesda.Plugins;

using Newtonsoft.Json;

namespace CommonTests.JsonConverters
{
    public class ModKeyConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.ModKeyConverter()] },
            ];

        [Theory]
        [ClassData(typeof(Data_ModKeyConverter))]
        public void Test_ModKeyConverter (string jsonInput, ModKey expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<ModKey>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<ModKey>(serialized, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        public class Data_ModKeyConverter : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[] { @"""Skyrim.esm""", new ModKey("Skyrim", ModType.Master), new List<string>() { @"""Skyrim.esm""" } };
                yield return new object?[] { @"""Skyrim.eSl""", new ModKey("Skyrim", ModType.Light), new List<string>() { @"""Skyrim.esl""" } };
                yield return new object?[] { @"""[Some] long MOD name.esp""", new ModKey("[Some] long MOD name", ModType.Plugin), new List<string>() { @"""[Some] long MOD name.esp""" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}