// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using OneOf;

    /// <summary>
    /// System.Text.Json converter factory that mirrors
    /// <see cref="OneOfJsonConverter"/>: serializes the inner value of a
    /// <c>OneOf&lt;T0, T1, ...&gt;</c> directly and deserializes by trying each
    /// type parameter in declaration order.
    /// </summary>
    public class OneOfJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IOneOf).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(OneOfConverterImpl<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private class OneOfConverterImpl<TOneOf> : JsonConverter<TOneOf>
            where TOneOf : class
        {
            public override TOneOf? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                using var doc = JsonDocument.ParseValue(ref reader);
                var element = doc.RootElement;
                var typeArgs = typeToConvert.GetGenericArguments();

                foreach (var arg in typeArgs)
                {
                    if (ElementMatchesType(element, arg))
                    {
                        try
                        {
                            var raw = element.GetRawText();
                            var converted = JsonSerializer.Deserialize(raw, arg, options);
                            return (TOneOf?)InvokeImplicit(typeToConvert, arg, converted);
                        }
                        catch (JsonException)
                        {
                            continue;
                        }
                    }
                }

                // Fallback to first type param.
                var fallbackType = typeArgs[0];
                var fallbackRaw = element.GetRawText();
                var fallbackValue = JsonSerializer.Deserialize(fallbackRaw, fallbackType, options);
                return (TOneOf?)InvokeImplicit(typeToConvert, fallbackType, fallbackValue);
            }

            public override void Write(Utf8JsonWriter writer, TOneOf value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                var oneOf = value as IOneOf;
                var inner = oneOf?.Value;
                if (inner == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                JsonSerializer.Serialize(writer, inner, inner.GetType(), options);
            }

            private static bool ElementMatchesType(JsonElement element, Type type)
            {
                var underlying = Nullable.GetUnderlyingType(type) ?? type;
                return element.ValueKind switch
                {
                    JsonValueKind.String => underlying == typeof(string) || underlying.IsEnum,
                    JsonValueKind.Number => IsNumericType(underlying),
                    JsonValueKind.True or JsonValueKind.False => underlying == typeof(bool),
                    JsonValueKind.Array => underlying.IsArray
                        || (underlying.IsGenericType
                            && typeof(System.Collections.IEnumerable).IsAssignableFrom(underlying)),
                    JsonValueKind.Object => !underlying.IsPrimitive && underlying != typeof(string),
                    JsonValueKind.Null => !underlying.IsValueType || Nullable.GetUnderlyingType(type) != null,
                    _ => false,
                };
            }

            private static bool IsNumericType(Type type) =>
                type == typeof(int) || type == typeof(long) || type == typeof(short)
                || type == typeof(byte) || type == typeof(double) || type == typeof(float)
                || type == typeof(decimal);

            private static object? InvokeImplicit(Type oneOfType, Type argType, object? value)
            {
                var op = oneOfType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "op_Implicit"
                        && m.ReturnType == oneOfType
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == argType);

                if (op != null)
                {
                    return op.Invoke(null, new[] { value });
                }

                throw new JsonException(
                    $"OneOfJsonConverterFactory: cannot construct {oneOfType.Name} from {argType.Name}.");
            }
        }
    }
}
