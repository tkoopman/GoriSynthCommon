using System.Collections;

using Newtonsoft.Json;

namespace CommonTests.JsonConverters
{
    public class ListConverterTests
    {
        private readonly JsonSerializerSettings[] _serializerSettings =
            [
            new() { Converters = [new Common.JsonConverters.ListConverter<string>()] },
            new() { Converters = [new Common.JsonConverters.ListConverter<string>() { WriteAsOptions = Common.JsonConverters.ListConverter.WriteAsOption.EmptyListAsNull | Common.JsonConverters.ListConverter.WriteAsOption.SingleItemAsValue }] },
            ];

        private readonly Type[] Types =
            [
            typeof(List<string>),
            typeof(string[]),
            typeof(ICollection<string>),
            typeof(HashSet<string>),
            ];

        private readonly Type[] TypesNullable =
            [
            typeof(List<string?>),
            typeof(string?[]),
            typeof(ICollection<string?>),
            typeof(HashSet<string>),
            ];

        [Theory]
        [ClassData(typeof(Data_ListConverter))]
        public void Test_ListConverter (string jsonInput, List<string> expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            foreach (var type in Types)
            {
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
        }

        [Theory]
        [ClassData(typeof(Data_ListConverterNullable))]
        public void Test_ListConverterNullable (string jsonInput, List<string?> expected, List<string> jsonOutput)
        {
            Assert.Equal(_serializerSettings.Length, jsonOutput.Count);
            foreach (var type in TypesNullable)
            {
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
        }

        private class Data_ListConverter : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator ()
            {
                yield return new object[] { @"""Hi""", new List<string>() { "Hi" }, new List<string>() { """["Hi"]""", @"""Hi""" } };
                yield return new object[] { """["Hi"]""", new List<string>() { "Hi" }, new List<string>() { """["Hi"]""", @"""Hi""" } };
                yield return new object[] { """["Hi", "There"]""", new List<string>() { "Hi", "There" }, new List<string>() { """["Hi","There"]""", """["Hi","There"]""" } };
                yield return new object[] { "null", new List<string>(), new List<string>() { "[]", "null" } };
                yield return new object[] { "[null]", new List<string>() { null! }, new List<string>() { "[null]", "[null]" } }; // TODO: If I find a way to know the T in List<T> was marked or not as nullable, then this should change to []
                yield return new object[] { "[]", new List<string>(), new List<string>() { "[]", "null" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }

        private class Data_ListConverterNullable : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator ()
            {
                yield return new object[] { @"""Hi""", new List<string?>() { "Hi" }, new List<string>() { """["Hi"]""", @"""Hi""" } };
                yield return new object[] { """["Hi"]""", new List<string?>() { "Hi" }, new List<string>() { """["Hi"]""", @"""Hi""" } };
                yield return new object[] { """["Hi", "There"]""", new List<string?>() { "Hi", "There" }, new List<string>() { """["Hi","There"]""", """["Hi","There"]""" } };
                yield return new object[] { "null", new List<string?>(), new List<string>() { "[]", "null" } };
                yield return new object[] { "[null]", new List<string?>() { null }, new List<string>() { "[null]", "[null]" } };  // TODO: However this one would remain [null] because the T in List<T> is marked as nullable in Test method used.
                yield return new object[] { "[]", new List<string?>(), new List<string>() { "[]", "null" } };
            }

            IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
        }
    }
}