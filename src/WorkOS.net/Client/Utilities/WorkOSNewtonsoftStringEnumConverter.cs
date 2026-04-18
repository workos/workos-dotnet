// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Newtonsoft.Json converter for enums that respects
    /// <see cref="EnumMemberAttribute"/> values for serialization and is
    /// forward-compatible: unknown enum values deserialize to the enum's
    /// zero-valued member (by convention, <c>Unknown</c>) instead of throwing.
    ///
    /// Paired with <see cref="WorkOSStringEnumConverterFactory"/> (STJ) so both
    /// JSON stacks behave identically.
    /// </summary>
    public class WorkOSNewtonsoftStringEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var underlying = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return underlying.IsEnum;
        }

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            var underlying = Nullable.GetUnderlyingType(objectType) ?? objectType;

            if (reader.TokenType == JsonToken.Null)
            {
                if (underlying != objectType)
                {
                    return null;
                }

                return Enum.ToObject(underlying, 0);
            }

            var raw = reader.Value?.ToString();
            if (raw == null)
            {
                return Enum.ToObject(underlying, 0);
            }

            foreach (var field in underlying.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attr != null && attr.Value == raw)
                {
                    return field.GetValue(null);
                }

                if (field.Name == raw)
                {
                    return field.GetValue(null);
                }
            }

            // Forward-compat: unknown API enum values deserialize to the zero
            // member (conventionally named Unknown) instead of throwing.
            return Enum.ToObject(underlying, 0);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var type = value.GetType();
            var field = type.GetField(value.ToString()!);
            var attr = field?.GetCustomAttribute<EnumMemberAttribute>();
            writer.WriteValue(attr?.Value ?? value.ToString());
        }
    }
}
