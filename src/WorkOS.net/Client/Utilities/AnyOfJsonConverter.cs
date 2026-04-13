// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Newtonsoft.Json converter for <see cref="AnyOf{T1,T2}"/> and
    /// <see cref="AnyOf{T1,T2,T3}"/>. Serializes the underlying inner value
    /// directly (not wrapped in <c>{ "Value": ... }</c>) and, when reading,
    /// attempts to deserialize the incoming JSON into each type parameter in
    /// declaration order, picking the first that succeeds.
    /// </summary>
    public class AnyOfJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (!objectType.IsGenericType)
            {
                return false;
            }

            var def = objectType.GetGenericTypeDefinition();
            return def == typeof(AnyOf<,>) || def == typeof(AnyOf<,,>);
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

            var valueProp = value.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            var inner = valueProp?.GetValue(value);
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

        private static object? InvokeImplicit(Type anyOfType, Type argType, object? value)
        {
            // Use the implicit conversion operator defined on AnyOf<...> for argType.
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

            // Fallback: try to construct via reflection — there's a private ctor taking object?.
            var ctor = anyOfType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(object) },
                modifiers: null);
            if (ctor != null)
            {
                return ctor.Invoke(new[] { value });
            }

            throw new JsonSerializationException(
                $"AnyOfJsonConverter: cannot construct {anyOfType.Name} from {argType.Name}.");
        }
    }
}
