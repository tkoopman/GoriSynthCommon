using System.Reflection;

using Newtonsoft.Json;

using static Common.JsonConverters.FlagConverter;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Converts Enum values with Flag attributes from JSON
    /// </summary>
    public class FlagConverter (WriteAs writeAs = WriteAs.String) : JsonConverter
    {
        /// <summary>
        ///     What JSON type to write the flags as.
        /// </summary>
        public enum WriteAs
        {
            /// <summary>
            ///     Array of Strings, each representing a set flag.
            /// </summary>
            Array,

            /// <summary>
            ///     Use ToString() on the enum value.
            /// </summary>
            String,

            /// <summary>
            ///     Write Enum as an integer value.
            /// </summary>
            Int,
        }

        /// <summary>
        ///     What JSON type to write as.
        /// </summary>
        public WriteAs WriteAsType { get; set; } = writeAs;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType.IsEnum && objectType.GetCustomAttributes<FlagsAttribute>().Any();

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            objectType = objectType.GenericTypeArguments.FirstOrDefault(objectType);

            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                    string[] flags = serializer.Deserialize<string[]>(reader) ?? [];
                    return Enum.Parse(objectType, string.Join(',', flags), true);

                case JsonToken.Integer:
                    return Enum.ToObject(objectType, reader.Value ?? 0);

                case JsonToken.String:
                    return Enum.Parse(objectType, serializer.Deserialize<string>(reader) ?? "", true);

                case JsonToken.Null:
                    return null;
            }

            throw new JsonReaderException("Invalid type found for Flag");
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            switch (WriteAsType)
            {
                case WriteAs.Array:
                    if (value is not Enum enumValue)
                    {
                        writer.WriteNull();
                        return;
                    }

                    string[] flags = enumValue.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    writer.WriteStartArray();

                    foreach (string flag in flags)
                        writer.WriteValue(flag);

                    writer.WriteEndArray();
                    break;

                case WriteAs.String:
                    if (value is null)
                        writer.WriteNull();
                    else
                        writer.WriteValue(value.ToString());

                    break;

                case WriteAs.Int:
                    writer.WriteValue(value is not null ? Convert.ToInt32(value) : 0);
                    break;
            }
        }
    }
}