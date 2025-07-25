using Newtonsoft.Json;

using static Common.JsonConverters.FormKeyConverter;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Read and Write RecordID values from JSON.
    /// </summary>
    public class RecordIDConverter : JsonConverter
    {
        /// <summary>
        ///     Method to use to try and convert FormID to FormKey.
        /// </summary>
        public SynthCommon.FormIDToFormKeyConverter? FormIDToFormKeyConverter { get; set; } = null;

        /// <summary>
        ///     Format to use when converting FormKey to string.
        /// </summary>
        public Format FormKeyFormat { get; set; } = Format.Default;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(RecordID);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            string? key = reader.Value?.ToString();
            return key is not null ? SynthCommon.ConvertToBethesdaID(key, FormIDToFormKeyConverter) : new RecordID();
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not RecordID recordID || (recordID.Type is IDType.Invalid && recordID.Invalid is null))
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(recordID.ToString(FormKeyFormat));
        }
    }
}