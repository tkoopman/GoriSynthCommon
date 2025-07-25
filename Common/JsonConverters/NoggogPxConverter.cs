using System.Collections;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Noggog;

namespace Common.JsonConverters
{
    /// <summary>
    ///     JSON converter for all the Noggog.P* structures
    /// </summary>
    public class NoggogPxConverter : JsonConverter
    {
        /// <summary>
        ///     What JSON type to write the value as.
        /// </summary>
        public enum WriteAs
        {
            /// <summary>
            ///     Array of values either [x,y] or [x,y,z].
            /// </summary>
            Array,

            /// <summary>
            ///     Convert to object with x,y and z properties.
            /// </summary>
            Object,
        }

        /// <summary>
        ///     Json type to write value as.
        /// </summary>
        public WriteAs WriteAsType { get; set; } = WriteAs.Array;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType)
            => objectType == typeof(P2Double)
            || objectType == typeof(P2Float)
            || objectType == typeof(P2Int)
            || objectType == typeof(P2Int16)
            || objectType == typeof(P2UInt8)
            || objectType == typeof(P3Double)
            || objectType == typeof(P3Float)
            || objectType == typeof(P3Int)
            || objectType == typeof(P3Int16)
            || objectType == typeof(P3UInt16)
            || objectType == typeof(P3UInt8);

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            bool hasZ = objectType.GetProperty("Z") is not null;
            var t = objectType.GetProperty("X")?.PropertyType ?? throw new JsonReaderException($"Unable to get 'X' property type from {objectType.GetClassName()}");

            object x, y, z = null!;

            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                    var listType = typeof(List<>).MakeGenericType(t);
                    var data = serializer.Deserialize(reader, listType) as IList ?? throw getException(objectType, hasZ);

                    x = data[0] ?? throw getException(objectType, hasZ);
                    y = data[1] ?? throw getException(objectType, hasZ);
                    if (hasZ)
                        z = data[2] ?? throw getException(objectType, hasZ);

                    break;

                case JsonToken.StartObject:
                    var jObject = JObject.Load(reader);

                    x = (jObject.GetValue("x", StringComparison.OrdinalIgnoreCase) is JValue x1 ? x1.Value : null) ?? throw getExceptionObj(objectType, hasZ);
                    y = (jObject.GetValue("y", StringComparison.OrdinalIgnoreCase) is JValue y1 ? y1.Value : null) ?? throw getExceptionObj(objectType, hasZ);
                    if (hasZ)
                        z = (jObject.GetValue("z", StringComparison.OrdinalIgnoreCase) is JValue z1 ? z1.Value : null) ?? throw getExceptionObj(objectType, hasZ);

                    break;

                case JsonToken.Null:
                    return null;

                default:
                    throw new JsonSerializationException($"Unable to read {objectType.GetClassName()}.");
            }

            x = Convert.ChangeType(x, t);
            y = Convert.ChangeType(y, t);

            if (hasZ)
            {
                z = Convert.ChangeType(z, t);
                var con = objectType.GetConstructor([t, t, t]) ?? throw new JsonSerializationException($"Unable to construct {objectType.GetClassName()}.");
                return con.Invoke([x, y, z]);
            }
            else
            {
                var con = objectType.GetConstructor([t, t]) ?? throw new JsonSerializationException($"Unable to construct {objectType.GetClassName()}.");
                return con.Invoke([x, y]);
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

            object x = value.GetType().GetProperty("X")?.GetValue(value) ?? throw new JsonSerializationException($"Unable to write {value.GetClassName()}. Missing 'X' property.");
            object y = value.GetType().GetProperty("Y")?.GetValue(value) ?? throw new JsonSerializationException($"Unable to write {value.GetClassName()}. Missing 'Y' property.");
            object? z = value.GetType().GetProperty("Z")?.GetValue(value);

            switch (WriteAsType)
            {
                case WriteAs.Array:
                    writer.WriteStartArray();
                    writer.WriteValue(x);
                    writer.WriteValue(y);
                    if (z is not null)
                        writer.WriteValue(z);
                    writer.WriteEndArray();
                    break;

                case WriteAs.Object:
                    writer.WriteStartObject();
                    writer.WritePropertyName("x");
                    serializer.Serialize(writer, x);
                    writer.WritePropertyName("y");
                    serializer.Serialize(writer, y);
                    if (z is not null)
                    {
                        writer.WritePropertyName("z");
                        serializer.Serialize(writer, z);
                    }

                    writer.WriteEndObject();
                    break;
            }
        }

        private static JsonSerializationException getException (Type objectType, bool hasZ)
                            => hasZ
             ? new JsonSerializationException($"Unable to read {objectType.GetClassName()}. Array requires 3 numbers [x, y, z]")
             : new JsonSerializationException($"Unable to read {objectType.GetClassName()}. Array requires 2 numbers [x, y]");

        private static JsonSerializationException getExceptionObj (Type objectType, bool hasZ)
            => hasZ
             ? new JsonSerializationException($"Unable to read {objectType.GetClassName()}. Json Object requires x, y and z values.")
             : new JsonSerializationException($"Unable to read {objectType.GetClassName()}. Json Object requires x and y values.");
    }
}