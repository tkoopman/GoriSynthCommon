using Mutagen.Bethesda.Plugins;

using Newtonsoft.Json;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Read and write FormKey values in JSON
    /// </summary>
    public class FormKeyConverter : JsonConverter
    {
        /// <summary>
        ///     When converting FormKey to string these flags can be used to change the format.
        /// </summary>
        [Flags]
        public enum Format
        {
            /// <summary>
            ///     Default format used by Mutagen and GSP. (i.e., "0001A3:MyMod.esm")
            /// </summary>
            Default = 0,

            /// <summary>
            ///     Add a hex prefix (0x) to string output.
            /// </summary>
            HexPrefix = 1 << 0,

            /// <summary>
            ///     Remove leading zeros to the FormID part of string output.
            /// </summary>
            RemoveLeadingZeros = 1 << 1,

            /// <summary>
            ///     Change the separator to a tilde (~).
            /// </summary>
            SeparatorTilde = 1 << 2,

            /// <summary>
            ///     Default format used in configuration files for SKSE plugins. (i.e.,
            ///     "0x1A3~MyMod.esm")
            /// </summary>
            SKSEDefault = HexPrefix | SeparatorTilde | RemoveLeadingZeros,
        }

        /// <summary>
        ///     Format to use when converting FormKey to string.
        /// </summary>
        public Format WriteFormat { get; set; } = Format.Default;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(FormKey);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            string? str = reader.Value?.ToString();

            return SynthCommon.TryConvertToBethesdaID(str, out var recordID) == IDType.FormKey
                ? recordID.FormKey
                : recordID.IsNull ? (object)FormKey.Null : throw new JsonSerializationException($"Invalid FormKey format: {str}");
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not FormKey formKey || formKey.IsNull)
                writer.WriteNull();
            else
                writer.WriteValue(formKey.ToString(WriteFormat));
        }
    }
}