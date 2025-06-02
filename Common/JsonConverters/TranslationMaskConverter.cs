using Loqui;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.JsonConverters
{
    public class TranslationMaskConverter : JsonConverter
    {
        public override bool CanConvert (Type objectType) => objectType.IsClass && objectType.IsAssignableTo(typeof(ITranslationMask));

        public override object? ReadJson (JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var con = objectType.GetConstructor([typeof(bool), typeof(bool)]) ?? throw new JsonSerializationException($"Type {objectType.GetClassName()} does not have a public constructor that takes two boolean parameters.");
            bool onOverAll = (bool)(con.GetParameters()[1].DefaultValue ?? throw new JsonSerializationException($"Type {objectType.GetClassName()} does not have a public constructor that takes one boolean parameter."));

            switch (reader.TokenType)
            {
                case JsonToken.Boolean:
                    bool value = (bool)(reader.Value ?? throw new JsonSerializationException("Unable to read boolean value for TranslationMask"));
                    return con.Invoke([value, onOverAll]);

                case JsonToken.StartObject:

                    var jObject = JObject.Load(reader);
                    bool defaultOn = getBoolValue(jObject, "DefaultOn", true);

                    object mask = con.Invoke([defaultOn, onOverAll]);

                    // Set any included properties on the mask
                    foreach (var field in objectType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        if (field.IsInitOnly)
                            continue; // Skip readonly fields

                        var jToken = jObject.GetValue(field.Name, StringComparison.OrdinalIgnoreCase);
                        if (jToken is null)
                            continue;

                        if (field.FieldType == typeof(bool))
                        {
                            if (jToken.Type != JTokenType.Boolean)
                                throw new JsonSerializationException($"Expected boolean value for '{field.Name}' but found {jToken.Type}.");
                            field.SetValue(mask, jToken.Value<bool>());
                            continue;
                        }

                        if (!field.FieldType.IsAssignableTo(typeof(ITranslationMask)))
                            continue;

                        if (jToken.Type is JTokenType.Boolean or JTokenType.Object)
                        {
                            object? pValue = serializer.Deserialize(jToken.CreateReader(), field.FieldType);
                            field.SetValue(mask, pValue);
                            continue;
                        }

                        throw new JsonSerializationException($"Expected boolean or object value for '{field.Name}' but found {jToken.Type}.");
                    }

                    return mask;

                default:
                    throw new JsonSerializationException($"Invalid token type {reader.TokenType} found for TranslationMask.");
            }
        }

        public override void WriteJson (JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

        private static bool getBoolValue (JObject jObject, string name, bool defaultValue, bool defaultInsteadOfException = false)
        {
            var token = jObject.GetValue(name, StringComparison.OrdinalIgnoreCase);
            return token is null
                        ? defaultValue
                        : token.Type != JTokenType.Boolean
                        ? defaultInsteadOfException
                            ? defaultValue
                            : throw new JsonSerializationException($"Expected boolean value for '{name}' but found {jObject[name]?.Type}.")
                        : token.Value<bool>();
        }
    }
}