using Mutagen.Bethesda.Plugins;

using Newtonsoft.Json;

namespace Common.JsonConverters
{
    /// <summary>
    ///     ModKey JSOn Converter.
    /// </summary>
    public partial class ModKeyConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(ModKey);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return ModKey.Null;

                case JsonToken.String:
                    return ModKey.FromFileName(reader.Value?.ToString() ?? throw new JsonSerializationException($"Unexpected error when parsing ModKey"));

                default:
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing ModKey");
            }
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (value is not ModKey modKey)
                throw new JsonSerializationException($"Expected ModKey, got {value.GetClassName()}");

            if (modKey.IsNull)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(modKey.ToString());
        }
    }
}