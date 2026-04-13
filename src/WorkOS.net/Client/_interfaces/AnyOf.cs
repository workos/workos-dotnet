// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;
    using STJS = System.Text.Json.Serialization;

    /// <summary>
    /// Represents a value that can be one of two types.
    /// </summary>
    /// <typeparam name="T1">The first possible type.</typeparam>
    /// <typeparam name="T2">The second possible type.</typeparam>
    [JsonConverter(typeof(AnyOfJsonConverter))]
    [STJS.JsonConverter(typeof(AnyOfJsonConverterFactory))]
    public class AnyOf<T1, T2>
    {
        private readonly object? value;

        private AnyOf(object? value) => this.value = value;

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        public object? Value => this.value;

        public static implicit operator AnyOf<T1, T2>(T1? value) => new AnyOf<T1, T2>(value);
        public static implicit operator AnyOf<T1, T2>(T2? value) => new AnyOf<T1, T2>(value);
    }

    /// <summary>
    /// Represents a value that can be one of three types.
    /// </summary>
    /// <typeparam name="T1">The first possible type.</typeparam>
    /// <typeparam name="T2">The second possible type.</typeparam>
    /// <typeparam name="T3">The third possible type.</typeparam>
    [JsonConverter(typeof(AnyOfJsonConverter))]
    [STJS.JsonConverter(typeof(AnyOfJsonConverterFactory))]
    public class AnyOf<T1, T2, T3>
    {
        private readonly object? value;

        private AnyOf(object? value) => this.value = value;

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        public object? Value => this.value;

        public static implicit operator AnyOf<T1, T2, T3>(T1? value) => new AnyOf<T1, T2, T3>(value);
        public static implicit operator AnyOf<T1, T2, T3>(T2? value) => new AnyOf<T1, T2, T3>(value);
        public static implicit operator AnyOf<T1, T2, T3>(T3? value) => new AnyOf<T1, T2, T3>(value);
    }
}
