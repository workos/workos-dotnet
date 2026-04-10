namespace WorkOS
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// System.Text.Json converter factory for enums that reads
    /// <see cref="EnumMemberAttribute"/> values for serialization.
    /// </summary>
    public class WorkOSStringEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(WorkOSStringEnumConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    internal class WorkOSStringEnumConverter<T> : JsonConverter<T>
        where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attr != null && attr.Value == value)
                {
                    return (T)field.GetValue(null);
                }

                if (field.Name == value)
                {
                    return (T)field.GetValue(null);
                }
            }

            // Return Unknown sentinel (value 0) for unrecognized strings.
            // This is forward-compatible: new API enum values won't cause
            // deserialization failures.
            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var field = typeof(T).GetField(value.ToString());
            var attr = field?.GetCustomAttribute<EnumMemberAttribute>();
            writer.WriteStringValue(attr?.Value ?? value.ToString());
        }
    }
}
