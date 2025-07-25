using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Noggog;

namespace Common.JsonConverters
{
    /// <summary>
    ///     JSON Converter for <see cref="MemorySlice{T}" /> of <see cref="byte" />.
    /// </summary>
    public partial class MemorySliceByteConverter : JsonConverter
    {
        private int _lineSize = 4 * 8;
        private byte groupSize = 4;
        private byte groupsPerLine = 8;

        /// <summary>
        ///     If <see cref="WriteAsByteArray" /> is false, this is the number of bytes to output
        ///     as HEX between spaces.
        /// </summary>
        public byte GroupSize
        {
            get => groupSize;

            set
            {
                groupSize = value;
                _lineSize = value * GroupsPerString;
            }
        }

        /// <summary>
        ///     If <see cref="WriteAsByteArray" /> is false, this is the number of HEX digit groups
        ///     ( <see cref="GroupSize" /> ) per array entry.
        /// </summary>
        public byte GroupsPerString
        {
            get => groupsPerLine;

            set
            {
                groupsPerLine = value;
                _lineSize = GroupSize * value;
            }
        }

        /// <summary>
        ///     Write the <see cref="MemorySlice{T}" /> as a byte array instead of a string array of
        ///     HEX values.
        /// </summary>
        public bool WriteAsByteArray { get; set; } = false;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(MemorySlice<byte>);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            List<byte> bytes = [];

            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                    var data = serializer.Deserialize<JArray>(reader) ?? throw new JsonSerializationException("Unable to read byte array.");
                    if (data.Count == 0)
                        return new MemorySlice<byte>();

                    foreach (var value in data.Values())
                    {
                        switch (value.Type)
                        {
                            case JTokenType.Integer:
                                bytes.Add(value.Value<byte>());
                                break;

                            case JTokenType.String:
                                parseString(value.Value<string>() ?? string.Empty, bytes);
                                break;

                            default:
                                throw new JsonSerializationException($"Invalid token type for byte[]: {value.Type}");
                        }
                    }

                    break;

                case JsonToken.String:
                    parseString(reader.ReadAsString() ?? string.Empty, bytes);
                    break;

                default:
                    throw new JsonSerializationException($"Invalid Json object - {reader.TokenType}");
            }

            return new MemorySlice<byte>([.. bytes]);
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (value is not MemorySlice<byte> memorySlice)
                throw new JsonSerializationException($"Expected MemorySlice<byte>, got {value.GetType()}");

            if (WriteAsByteArray)
            {
                // Write as byte array
                writer.WriteStartArray();
                foreach (byte b in memorySlice)
                    writer.WriteValue(b);
                writer.WriteEndArray();
                return;
            }

            // Write as string array
            writer.WriteStartArray();
            int groupCount = 0;
            int lineCount = 0;
            var sb = new StringBuilder();
            foreach (byte b in memorySlice)
            {
                _ = sb.Append(b.ToString("X2"));

                if (++lineCount >= _lineSize)
                {
                    writer.WriteValue(sb.ToString());
                    _ = sb.Clear();
                    lineCount = 0;
                    groupCount = 0;
                }
                else if (++groupCount >= groupSize)
                {
                    _ = sb.Append(' ');
                    groupCount = 0;
                }
            }

            if (sb.Length > 0)
                writer.WriteValue(sb.ToString().TrimEnd());

            writer.WriteEndArray();
        }

        [GeneratedRegex(@"^(?:(?:0x)?(?'byte'[0-9a-zA-Z]{2})(?:\s)?)+\z", RegexOptions.Singleline | RegexOptions.Compiled)]
        private static partial Regex BytesRegex ();

        private static void parseString (string str, List<byte> bytes)
        {
            if (string.IsNullOrWhiteSpace(str))
                return;

            var matches = BytesRegex().Matches(str);
            foreach (Match match in matches)
            {
                if (!match.Success || !match.Groups.ContainsKey("byte"))
                    throw new JsonSerializationException($"Invalid byte string: {str}");

                foreach (Capture capture in match.Groups["byte"].Captures)
                {
                    if (byte.TryParse(capture.Value, System.Globalization.NumberStyles.HexNumber, null, out byte b))
                        bytes.Add(b);
                    else
                        throw new JsonSerializationException($"Invalid byte value: {capture.Value}");
                }
            }
        }
    }
}