// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using OneOf;

    /// <summary>
    /// Newtonsoft.Json converter for <c>OneOf&lt;T0, T1, ...&gt;</c> types from
    /// the OneOf NuGet package. Serializes the underlying inner value directly
    /// (not wrapped in <c>{ "Value": ... }</c>) and, when reading, attempts to
    /// deserialize the incoming JSON into each type parameter in declaration
    /// order, picking the first that succeeds.
    /// </summary>
    public class OneOfJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IOneOf).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var token = JToken.Load(reader);
            var typeArgs = objectType.GetGenericArguments();

            foreach (var arg in typeArgs)
            {
                if (TokenMatchesType(token, arg))
                {
                    object? converted;
                    try
                    {
                        converted = token.ToObject(arg, serializer);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }

                    return InvokeImplicit(objectType, arg, converted);
                }
            }

            // Fallback: use the first type parameter.
            var fallbackType = typeArgs[0];
            var fallback = token.ToObject(fallbackType, serializer);
            return InvokeImplicit(objectType, fallbackType, fallback);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var oneOf = value as IOneOf;
            var inner = oneOf?.Value;
            serializer.Serialize(writer, inner);
        }

        private static bool TokenMatchesType(JToken token, Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return token.Type switch
            {
                JTokenType.String => underlying == typeof(string) || underlying.IsEnum,
                JTokenType.Integer => IsNumericType(underlying),
                JTokenType.Float => IsNumericType(underlying),
                JTokenType.Boolean => underlying == typeof(bool),
                JTokenType.Array => underlying.IsArray
                    || (underlying.IsGenericType
                        && typeof(System.Collections.IEnumerable).IsAssignableFrom(underlying)),
                JTokenType.Object => !underlying.IsPrimitive && underlying != typeof(string),
                JTokenType.Null => !underlying.IsValueType || Nullable.GetUnderlyingType(type) != null,
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

            throw new JsonSerializationException(
                $"OneOfJsonConverter: cannot construct {oneOfType.Name} from {argType.Name}.");
        }
    }
}
