using System.Globalization;

using Newtonsoft.Json;

using Noggog;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Converts a <see cref="Percent" /> from JSON. Percent can be represented as a double (0.0
    ///     to 1.0) or a string with a percent sign (e.g., "50%").
    /// </summary>
    public class PercentConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(Percent);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:

                    //double p = reader.ReadAsDouble() ?? throw new JsonSerializationException("Unable to read percent");
                    double p = (double)(reader.Value ?? throw new JsonSerializationException("Unable to read percent"));
                    return new Percent(p);

                case JsonToken.String:

                    //string str = reader.ReadAsString() ?? throw new JsonSerializationException("Unable to read percent");
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
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}