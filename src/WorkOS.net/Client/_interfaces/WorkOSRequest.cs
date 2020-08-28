namespace WorkOS
{
    using System.Net.Http;

    public class WorkOSRequest
    {
        public HttpMethod Method { get; set; }

        public BaseOptions Options { get; set; }

        public string Path { get; set; }
    }
}
