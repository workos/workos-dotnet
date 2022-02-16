[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("WorkOSTests")]

namespace WorkOS
{
    using Newtonsoft.Json;

    public class EnrollFactorOptions : BaseOptions
    {
        internal EnrollFactorOptions(string type)
        {
            this.Type = type;
        }

        /// <summary>
        /// The type of factor to enroll.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; }
    }
}
