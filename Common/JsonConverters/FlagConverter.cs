using Newtonsoft.Json;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Converts Enum values with Flag attributes from JSON
    /// </summary>
    public class FlagConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType.IsEnum;

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            objectType = objectType.GenericTypeArguments.FirstOrDefault(objectType);

            if (reader.TokenType == JsonToken.String)
                return Enum.Parse(objectType, serializer.Deserialize<string>(reader) ?? "", true);

            if (reader.TokenType == JsonToken.StartArray)
            {
                string[] flags = serializer.Deserialize<string[]>(reader) ?? [];
                return Enum.Parse(objectType, string.Join(',', flags), true);
            }

            throw new JsonReaderException("Invalid type found for Flag");
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}