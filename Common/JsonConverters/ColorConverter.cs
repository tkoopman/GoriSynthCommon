using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using static Common.JsonConverters.ColorConverter;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Convert <a href="https://docs.microsoft.com/dotnet/api/system.drawing.color">Color</a>
    ///     values to and from JSON. Supported formats:
    ///
    ///     - String: The name of the color (e.g., "Red", "Blue", etc.) or a ARGB hex value (e.g.,
    ///       "#FF0000" of "#FFFF0000" for red).
    ///     - Integer: The ARGB value as an integer (e.g., -65536 for red).
    ///     - Array: An array of integers representing the color components, either [R, G, B] or [A,
    ///       R, G, B]. (e.g., [255, 0, 0] or [255, 255, 0, 0] for red).
    /// </summary>
    /// <param name="writeAs">When writing color which format to use.</param>
    public partial class ColorConverter (WriteAs writeAs = WriteAs.String) : JsonConverter
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
        public override bool CanConvert (Type objectType) => objectType == typeof(Color);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    int c = unchecked((int)(long)(reader.Value ?? throw new JsonSerializationException("Unable to read color")));
                    return Color.FromArgb(c);

                case JsonToken.StartArray:
                    var data = serializer.Deserialize<List<short>>(reader);
                    if (data is null || data.Count < 3 || data.Count > 4)
                        throw new JsonSerializationException("Unable to read object bounds. Array requires 3 or 4 numbers [A,R,G,B] or [R,G,B]");

                    if (data.Count == 3)
                    {
                        return Color.FromArgb(
                                data[0],
                                data[1],
                                data[2]);
                    }

                    return Color.FromArgb(
                            data[0],
                            data[1],
                            data[2],
                            data[3]);

                case JsonToken.String:
                    string str = (string)(reader.Value ?? throw new JsonSerializationException("Unable to read color"));

                    var argb = HexRegex().Match(str);
                    if (argb.Success)
                    {
                        return Color.FromArgb(
                            argb.Groups.ContainsKey("a") && argb.Groups["a"].Success ? byte.Parse(argb.Groups["a"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture) : byte.MaxValue,
                            byte.Parse(argb.Groups["r"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                            byte.Parse(argb.Groups["g"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                            byte.Parse(argb.Groups["b"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    }

                    var color = Color.FromName(str);

                    if (color.IsKnownColor)
                        return color;
                    break;
            }

            throw new JsonSerializationException("Unable to read color");
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not Color color)
                throw new JsonSerializationException($"{value?.GetType().GetClassName() ?? "null"} is not of type Color");

            switch (WriteAsType)
            {
                case WriteAs.Array:
                    writer.WriteStartArray();
                    if (color.A != byte.MaxValue)
                        writer.WriteValue(color.A);

                    writer.WriteValue(color.R);
                    writer.WriteValue(color.G);
                    writer.WriteValue(color.B);
                    writer.WriteEndArray();
                    break;

                case WriteAs.Int:
                    writer.WriteValue(color.ToArgb());
                    break;

                case WriteAs.String:
                    if (color.IsKnownColor)
                        writer.WriteValue(color.Name);
                    else if (color.A != byte.MaxValue)
                        writer.WriteValue($"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
                    else
                        writer.WriteValue($"#{color.R:X2}{color.G:X2}{color.B:X2}");
                    break;
            }
        }

        [GeneratedRegex(@"^(?:#|0x)(?'a'[0-9A-Fa-f]{2})?(?'r'[0-9A-Fa-f]{2})(?'g'[0-9A-Fa-f]{2})(?'b'[0-9A-Fa-f]{2})$")]
        private static partial Regex HexRegex ();
    }
}