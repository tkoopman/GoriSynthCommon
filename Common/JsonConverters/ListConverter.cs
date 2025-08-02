using Newtonsoft.Json;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Static class for ListConverter options.
    /// </summary>
    public class ListConverter
    {
        /// <summary>
        ///     Options for writing lists in JSON.
        /// </summary>
        [Flags]
        public enum WriteAsOption
        {
            /// <summary>
            ///     Default options.
            /// </summary>
            Default = 0,

            /// <summary>
            ///     Write empty list as null.
            /// </summary>
            EmptyListAsNull = 1 << 0,

            /// <summary>
            ///     Write single item list as value instead of array.
            /// </summary>
            SingleItemAsValue = 1 << 1
        }
    }

    /// <summary>
    ///     Reading and writing lists or arrays in JSON, but supports reading non JSON array entry
    ///     as single entry in to list / array.
    /// </summary>

    public class ListConverter<T> : JsonConverter
    {
        /// <summary>
        ///     Options for writing lists in JSON.
        /// </summary>
        public ListConverter.WriteAsOption WriteAsOptions { get; set; } = ListConverter.WriteAsOption.Default;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType) => objectType == typeof(List<T>) || objectType == typeof(HashSet<T>) || objectType == typeof(T[]) || objectType.IsAssignableFrom(typeof(List<T>)) || objectType.IsAssignableFrom(typeof(HashSet<T>));

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            ICollection<T> value = objectType.IsAssignableFrom(typeof(HashSet<T>)) ? new HashSet<T>() : new List<T>();

            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    break;

                case JsonToken.StartArray:
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                    {
                        if (reader.TokenType == JsonToken.Null)
                        {
                            value.Add(default!);
                            continue;
                        }

                        value.Add(serializer.Deserialize<T>(reader) ?? throw new JsonSerializationException("Unable to parse object."));
                    }

                    break;

                case JsonToken.StartObject:
                    value.Add(serializer.Deserialize<T>(reader) ?? throw new JsonSerializationException("Unable to parse object."));
                    break;

                case JsonToken.String:
                    value.Add(serializer.Deserialize<T>(reader) ?? throw new JsonSerializationException("Unable to parse object."));
                    break;

                default:
                    throw new JsonSerializationException($"Invalid Json object - {reader.TokenType}");
            }

#pragma warning disable IDE0305 // Simplify collection initialization
            return objectType.IsArray ? value.ToArray() : value;
#pragma warning restore IDE0305 // Simplify collection initialization
        }

        /// <inheritdoc />
        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (value is not IEnumerable<T> list)
                throw new JsonSerializationException($"Invalid type. {typeof(IEnumerable<T>).GetClassName()} expected but {value.GetType().GetClassName()} found.");

            if (!list.Any())
            {
                if (WriteAsOptions.HasFlag(ListConverter.WriteAsOption.EmptyListAsNull))
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }

                return;
            }

            if (list.Count() == 1 && WriteAsOptions.HasFlag(ListConverter.WriteAsOption.SingleItemAsValue) && list.First() is not null)
            {
                serializer.Serialize(writer, list.First());
                return;
            }

            writer.WriteStartArray();

            foreach (var item in list)
                serializer.Serialize(writer, item);

            writer.WriteEndArray();
        }
    }
}