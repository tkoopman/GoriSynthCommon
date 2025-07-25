using System.Globalization;

using Newtonsoft.Json;

using Noggog;

using static Common.JsonConverters.PercentConverter;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Converts a <see cref="Percent" /> from JSON. Percent can be represented as a double (0.0
    ///     to 1.0) or a string with a percent sign (e.g., "50%").
    /// </summary>
    public class PercentConverter (WriteAs writeAs = WriteAs.String) : JsonConverter
    {
        /// <summary>
        ///     What JSON type to write the percentage as.
        /// </summary>
        public enum WriteAs
        {
            /// <summary>
            ///     #%
            /// </summary>
            String,

            /// <summary>
            ///     0.0 to 1.0 as a float.
            /// </summary>
            Float,
        }

        /// <summary>
        ///     What JSON type to write as.
        /// </summary>
        public WriteAs WriteAsType { get; set; } = writeAs;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(Percent);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    double p = Convert.ToDouble(reader.Value ?? throw new JsonSerializationException("Unable to read percent"));
                    return new Percent(p);

                case JsonToken.String:
                    string str = (string)(reader.Value ?? throw new JsonSerializationException("Unable to read percent"));
                    bool signed = str.EndsWith('%');
                    double pd = double.Parse(str.TrimEnd('%'), CultureInfo.InvariantCulture);
                    if (signed)
                        pd /= 100;
                    return new Percent(pd);

                default:
                    throw new JsonSerializationException("Unable to read percent");
            }
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not Percent percent)
                throw new JsonSerializationException($"{value?.GetType().GetClassName() ?? "null"} is not of type Percent");

            switch (WriteAsType)
            {
                case WriteAs.String:
                    writer.WriteValue(percent.ToString("0.####"));
                    break;

                case WriteAs.Float:
                    writer.WriteValue(percent.Value);
                    break;
            }
        }
    }
}