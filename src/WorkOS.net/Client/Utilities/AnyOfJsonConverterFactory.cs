// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// System.Text.Json converter factory that mirrors
    /// <see cref="AnyOfJsonConverter"/>: serializes the inner value of an
    /// <see cref="AnyOf{T1,T2}"/> / <see cref="AnyOf{T1,T2,T3}"/> directly and
    /// deserializes by trying each type parameter in declaration order.
    /// </summary>
    public class AnyOfJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            var def = typeToConvert.GetGenericTypeDefinition();
            return def == typeof(AnyOf<,>) || def == typeof(AnyOf<,,>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(AnyOfConverterImpl<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private class AnyOfConverterImpl<TAnyOf> : JsonConverter<TAnyOf>
            where TAnyOf : class
        {
            public override TAnyOf? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                            return (TAnyOf?)InvokeImplicit(typeToConvert, arg, converted);
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
                return (TAnyOf?)InvokeImplicit(typeToConvert, fallbackType, fallbackValue);
            }

            public override void Write(Utf8JsonWriter writer, TAnyOf value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                var valueProp = value.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                var inner = valueProp?.GetValue(value);
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

            private static object? InvokeImplicit(Type anyOfType, Type argType, object? value)
            {
                var op = anyOfType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name == "op_Implicit"
                        && m.ReturnType == anyOfType
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == argType);

                if (op != null)
                {
                    return op.Invoke(null, new[] { value });
                }

                var ctor = anyOfType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    binder: null,
                    types: new[] { typeof(object) },
                    modifiers: null);
                if (ctor != null)
                {
                    return ctor.Invoke(new[] { value });
                }

                throw new JsonException(
                    $"AnyOfJsonConverterFactory: cannot construct {anyOfType.Name} from {argType.Name}.");
            }
        }
    }
}
