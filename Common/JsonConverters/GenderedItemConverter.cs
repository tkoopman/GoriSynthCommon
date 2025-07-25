using Mutagen.Bethesda.Plugins.Records;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.JsonConverters
{
    /// <summary>
    ///     Converter for <see cref="GenderedItem" /> implementations.
    /// </summary>
    public class GenderedItemConverter : JsonConverter
    {
        /// <summary>
        ///     When writing if both Female and Male values are the same, write a single value
        ///     instead of an object with both.
        /// </summary>
        public bool WriteSimplified { get; set; } = false;

        /// <inheritdoc />
        public override bool CanConvert (Type objectType)
        {
            var exploded = objectType.Explode(2);
            return exploded.Length == 2 && exploded[0] == typeof(GenderedItem<>);
        }

        /// <inheritdoc />
        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var exploded = objectType.Explode(2);
            var type = exploded[1];
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var jObject = JObject.Load(reader);
                    var male = jObject.Property("Male", StringComparison.OrdinalIgnoreCase);
                    var female = jObject.Property("Female", StringComparison.OrdinalIgnoreCase);

                    if (male is not null && female is not null)
                    {
                        object? maleValue = serializer.Deserialize(male.Value.CreateReader(), type);
                        object? femaleValue = serializer.Deserialize(female.Value.CreateReader(), type);

                        return Activator.CreateInstance(objectType, maleValue, femaleValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    }

                    object objValue = serializer.Deserialize(jObject.CreateReader(), type) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    return Activator.CreateInstance(objectType, objValue, objValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");

                case JsonToken.Boolean:
                    bool value = reader.Value as bool? ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    return Activator.CreateInstance(objectType, value, value) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");

                case JsonToken.Null:
                    return null;

                case JsonToken.Integer:
                    long longValue = reader.Value as long? ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    if (type == typeof(int))
                    {
                        int intValue = checked((int)longValue);
                        return Activator.CreateInstance(objectType, intValue, intValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    }
                    else if (type == typeof(short))
                    {
                        short shortValue = checked((short)longValue);
                        return Activator.CreateInstance(objectType, shortValue, shortValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    }
                    else if (type == typeof(byte))
                    {
                        byte byteValue = checked((byte)longValue);
                        return Activator.CreateInstance(objectType, byteValue, byteValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    }

                    return Activator.CreateInstance(objectType, longValue, longValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");

                case JsonToken.Float:
                    double dblValue = reader.Value as double? ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    if (type == typeof(float))
                    {
                        float floatValue = checked((float)dblValue);
                        return Activator.CreateInstance(objectType, floatValue, floatValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    }

                    return Activator.CreateInstance(objectType, dblValue, dblValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");

                case JsonToken.String:
                    string strValue = reader.Value as string ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
                    return Activator.CreateInstance(objectType, strValue, strValue) ?? throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");

                default:
                    throw new JsonSerializationException($"Failed to create instance of {objectType.GetClassName()} with provided values.");
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

            var property = value.GetType().GetProperty(nameof(MaleFemaleGender.Female), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ?? throw new JsonSerializationException($"Type {value.GetType().GetClassName()} doesn't seem to be correct IGenderedItem<T> type");

            object? female = property.GetValue(value);

            property = value.GetType().GetProperty(nameof(MaleFemaleGender.Male), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ?? throw new JsonSerializationException($"Type {value.GetType().GetClassName()} doesn't seem to be correct IGenderedItem<T> type");

            object? male = property.GetValue(value);

            if (WriteSimplified && female is not null && Equals(female, male))
            {   // If both are the same but not null, write a single value
                serializer.Serialize(writer, female);
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName(nameof(MaleFemaleGender.Female));
            serializer.Serialize(writer, female);

            writer.WritePropertyName(nameof(MaleFemaleGender.Male));
            serializer.Serialize(writer, male);

            writer.WriteEndObject();
        }
    }
}