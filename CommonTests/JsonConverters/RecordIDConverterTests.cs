using System.Collections;

using Common;

using Mutagen.Bethesda.Plugins;

using Newtonsoft.Json;

namespace CommonTests.JsonConverters
{
    public class RecordIDConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.RecordIDConverter() { FormIDToFormKeyConverter = (id) => (id.FullMasterIndex == 0x00) ? new FormKey("Skyrim.esm", id.FullId) : FormKey.Null }] },
            new() { Converters = [new Common.JsonConverters.RecordIDConverter() { FormKeyFormat = Common.JsonConverters.FormKeyConverter.Format.SKSEDefault }] },
            ];

        [Theory]
        [ClassData(typeof(Data_RecordIDConverter))]
        public void Test_RecordIDConverter (string jsonInput, List<RecordID> expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                var result = JsonConvert.DeserializeObject<RecordID>(jsonInput, _serializerSettings[i]);
                Assert.Equal(expected[int.Min(i, expected.Count - 1)], result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject<RecordID>(serialized, _serializerSettings[i]);
                Assert.Equal(expected[int.Min(i, expected.Count - 1)], result);
            }
        }

        public class Data_RecordIDConverter : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[] { @"""AEditorID""", new List<RecordID> { new("AEditorID") }, new List<string>() { @"""AEditorID""", @"""AEditorID""" } };
                yield return new object?[] { @"""0x123456:Skyrim.esm""", new List<RecordID> { new(new FormKey("Skyrim.esm", 0x123456)) }, new List<string>() { @"""123456:Skyrim.esm""", @"""0x123456~Skyrim.esm""" } };
                yield return new object?[] { @"""0x1a3:Skyrim.esm""", new List<RecordID> { new(new FormKey("Skyrim.esm", 0x1A3)) }, new List<string>() { @"""0001A3:Skyrim.esm""", @"""0x1A3~Skyrim.esm""" }, };
                yield return new object?[] { @"""123456~Skyrim.esm""", new List<RecordID> { new(new FormKey("Skyrim.esm", 0x123456)) }, new List<string>() { @"""123456:Skyrim.esm""", @"""0x123456~Skyrim.esm""" } };
                yield return new object?[] { @"""00123456""", new List<RecordID> { new(new FormKey("Skyrim.esm", 0x123456)), new FormID(0x00123456) }, new List<string>() { @"""123456:Skyrim.esm""", @"""0x00123456""" }, };
                yield return new object?[] { @"""Skyrim.esm""", new List<RecordID> { new((ModKey)"Skyrim.esm") }, new List<string>() { @"""Skyrim.esm""", @"""Skyrim.esm""" }, };

                yield return new object?[]
                {   // Not valid FormID as missing leading 00 but could be EditorID
                    @"""0x123456""", new List<RecordID> { new(IDType.Name, "0x123456") }, new List<string>() { @"""0x123456""", @"""0x123456""" },
                };

                yield return new object?[]
                {   // Invalid ModKey but also invalid EditorID
                    @"""Skyrim<.esm""", new List<RecordID> { new(IDType.Invalid, "Skyrim<.esm") }, new List<string>() { @"""Skyrim<.esm""", @"""Skyrim<.esm""" },
                };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}