using System.Collections;

using Mutagen.Bethesda.Plugins.Records;

using Newtonsoft.Json;

namespace CommonTests.JsonConverters
{
    public class GenderedItemConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.GenderedItemConverter()] },
            new() { Converters = [new Common.JsonConverters.GenderedItemConverter() { WriteSimplified = true }] },
            ];

        [Theory]
        [ClassData(typeof(Data_GenderedItemConverter))]
        public void Test_GenderedItemConverter (string jsonInput, Type type, object? expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            for (int i = 0; i < _serializerSettings.Length; i++)
            {
                object? result = JsonConvert.DeserializeObject(jsonInput, type, _serializerSettings[i]);
                Assert.Equal(expected, result);

                string serialized = JsonConvert.SerializeObject(result, _serializerSettings[i]);
                Assert.Equal(jsonOutput[i], serialized);

                result = JsonConvert.DeserializeObject(serialized, type, _serializerSettings[i]);
                Assert.Equal(expected, result);
            }
        }

        private class Data_GenderedItemConverter : IEnumerable<object?[]>
        {
            public IEnumerator<object?[]> GetEnumerator ()
            {
                yield return new object?[] { "true", typeof(GenderedItem<bool>), new GenderedItem<bool>(true, true), new List<string>() { """{"Female":true,"Male":true}""", "true" } };
                yield return new object?[] { """{"female":true,"male":true}""", typeof(GenderedItem<bool>), new GenderedItem<bool>(true, true), new List<string>() { """{"Female":true,"Male":true}""", "true" } };
                yield return new object?[] { """{"female":true,"male":false}""", typeof(GenderedItem<bool>), new GenderedItem<bool>(false, true), new List<string>() { """{"Female":true,"Male":false}""", """{"Female":true,"Male":false}""" } };
                yield return new object?[] { """{"female":"girl","male":"boy"}""", typeof(GenderedItem<string?>), new GenderedItem<string?>("boy", "girl"), new List<string>() { """{"Female":"girl","Male":"boy"}""", """{"Female":"girl","Male":"boy"}""" } };
                yield return new object?[] { """{"male":"boy","female":"girl"}""", typeof(GenderedItem<string?>), new GenderedItem<string?>("boy", "girl"), new List<string>() { """{"Female":"girl","Male":"boy"}""", """{"Female":"girl","Male":"boy"}""" } };
                yield return new object?[] { """{"male":null,"female":"NotNull"}""", typeof(GenderedItem<string?>), new GenderedItem<string?>(null, "NotNull"), new List<string>() { """{"Female":"NotNull","Male":null}""", """{"Female":"NotNull","Male":null}""" } };
                yield return new object?[] { """{"male":null,"female":null}""", typeof(GenderedItem<string?>), new GenderedItem<string?>(null, null), new List<string>() { """{"Female":null,"Male":null}""", """{"Female":null,"Male":null}""" } };
                yield return new object?[] { "null", typeof(GenderedItem<string?>), null, new List<string>() { "null", "null" } };
                yield return new object?[] { @"""A String""", typeof(GenderedItem<string?>), new GenderedItem<string?>("A String", "A String"), new List<string>() { """{"Female":"A String","Male":"A String"}""", @"""A String""" } };
                yield return new object?[] { "3", typeof(GenderedItem<int>), new GenderedItem<int>(3, 3), new List<string>() { """{"Female":3,"Male":3}""", "3" } };
                yield return new object?[] { "3.0", typeof(GenderedItem<double>), new GenderedItem<double>(3, 3), new List<string>() { """{"Female":3.0,"Male":3.0}""", "3.0" } };
                yield return new object?[] { "3.0", typeof(GenderedItem<float>), new GenderedItem<float>(3, 3), new List<string>() { """{"Female":3.0,"Male":3.0}""", "3.0" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}