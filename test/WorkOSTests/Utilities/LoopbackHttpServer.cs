// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A real HTTP/1.1 server on a loopback port, for the cases a stubbed handler cannot
    /// prove: redirects are followed (or not) by the real handler, never by a stub. It
    /// answers one request per connection and records what it received.
    /// Linked into the provider test projects as well as this one.
    /// </summary>
    internal sealed class LoopbackHttpServer : IDisposable
    {
        private readonly TcpListener listener;
        private readonly Func<LoopbackRequest, LoopbackResponse> respond;
        private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
        private readonly List<LoopbackRequest> requests = new List<LoopbackRequest>();

        internal LoopbackHttpServer(Func<LoopbackRequest, LoopbackResponse> respond)
        {
            this.respond = respond;
            this.listener = new TcpListener(IPAddress.Loopback, 0);
            this.listener.Start();
            this.BaseUrl = $"http://127.0.0.1:{((IPEndPoint)this.listener.LocalEndpoint).Port}";
            _ = Task.Run(this.AcceptAsync);
        }

        /// <summary>The server's origin, for example <c>http://127.0.0.1:51234</c>.</summary>
        internal string BaseUrl { get; }

        /// <summary>Every request received so far, in order.</summary>
        internal IReadOnlyList<LoopbackRequest> Requests
        {
            get
            {
                lock (this.requests)
                {
                    return this.requests.ToArray();
                }
            }
        }

        public void Dispose()
        {
            this.shutdown.Cancel();
            this.listener.Stop();
            this.shutdown.Dispose();
        }

        private static async Task<LoopbackRequest?> ReadRequestAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new MemoryStream();
            var chunk = new byte[4096];
            var headerEnd = -1;
            while (headerEnd < 0)
            {
                var read = await stream.ReadAsync(chunk, cancellationToken);
                if (read == 0)
                {
                    return null;
                }

                buffer.Write(chunk, 0, read);
                headerEnd = IndexOfHeaderEnd(buffer.GetBuffer(), (int)buffer.Length);
            }

            var head = Encoding.ASCII.GetString(buffer.GetBuffer(), 0, headerEnd).Split("\r\n");
            var requestLine = head[0].Split(' ');
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 1; i < head.Length; i++)
            {
                var colon = head[i].IndexOf(':');
                if (colon > 0)
                {
                    headers[head[i].Substring(0, colon).Trim()] = head[i].Substring(colon + 1).Trim();
                }
            }

            var bodyStart = headerEnd + 4;
            var length = headers.TryGetValue("Content-Length", out var declared) ? int.Parse(declared) : 0;
            while (buffer.Length - bodyStart < length)
            {
                var read = await stream.ReadAsync(chunk, cancellationToken);
                if (read == 0)
                {
                    break;
                }

                buffer.Write(chunk, 0, read);
            }

            var body = Encoding.UTF8.GetString(buffer.GetBuffer(), bodyStart, (int)Math.Min(length, buffer.Length - bodyStart));
            return new LoopbackRequest(requestLine[0], requestLine[1], headers, body);
        }

        private static int IndexOfHeaderEnd(byte[] bytes, int length)
        {
            for (var i = 0; i + 3 < length; i++)
            {
                if (bytes[i] == '\r' && bytes[i + 1] == '\n' && bytes[i + 2] == '\r' && bytes[i + 3] == '\n')
                {
                    return i;
                }
            }

            return -1;
        }

        private static async Task WriteResponseAsync(Stream stream, LoopbackResponse response, CancellationToken cancellationToken)
        {
            var body = Encoding.UTF8.GetBytes(response.Body);
            var head = new StringBuilder();
            head.Append("HTTP/1.1 ").Append(response.Status).Append(" Status\r\n");
            foreach (var header in response.Headers)
            {
                head.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
            }

            head.Append("Content-Length: ").Append(body.Length).Append("\r\n");
            head.Append("Connection: close\r\n\r\n");

            await stream.WriteAsync(Encoding.ASCII.GetBytes(head.ToString()), cancellationToken);
            await stream.WriteAsync(body, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private async Task AcceptAsync()
        {
            while (!this.shutdown.IsCancellationRequested)
            {
                TcpClient connection;
                try
                {
                    connection = await this.listener.AcceptTcpClientAsync(this.shutdown.Token);
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }

                _ = Task.Run(() => this.ServeAsync(connection));
            }
        }

        private async Task ServeAsync(TcpClient connection)
        {
            using (connection)
            {
                try
                {
                    var stream = connection.GetStream();
                    var request = await ReadRequestAsync(stream, this.shutdown.Token);
                    if (request == null)
                    {
                        return;
                    }

                    lock (this.requests)
                    {
                        this.requests.Add(request);
                    }

                    await WriteResponseAsync(stream, this.respond(request), this.shutdown.Token);
                }
                catch (Exception ex) when (ex is IOException || ex is OperationCanceledException || ex is SocketException || ex is ObjectDisposedException)
                {
                    // The client went away, or the server is shutting down.
                }
            }
        }
    }

    /// <summary>A request the loopback server received.</summary>
    internal sealed class LoopbackRequest
    {
        internal LoopbackRequest(string method, string target, IReadOnlyDictionary<string, string> headers, string body)
        {
            this.Method = method;
            this.Target = target;
            this.Headers = headers;
            this.Body = body;
        }

        /// <summary>The HTTP method, for example <c>GET</c>.</summary>
        internal string Method { get; }

        /// <summary>The request target exactly as sent: path and query, still percent-encoded.</summary>
        internal string Target { get; }

        /// <summary>The path part of <see cref="Target"/>.</summary>
        internal string Path => this.Target.Split('?')[0];

        /// <summary>The query part of <see cref="Target"/>, without the question mark and still percent-encoded.</summary>
        internal string Query => this.Target.Contains('?') ? this.Target.Substring(this.Target.IndexOf('?') + 1) : string.Empty;

        /// <summary>The request headers, by case-insensitive name.</summary>
        internal IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>The request body.</summary>
        internal string Body { get; }
    }

    /// <summary>What the loopback server answers with.</summary>
    internal sealed class LoopbackResponse
    {
        internal LoopbackResponse(int status, string body = "", IReadOnlyDictionary<string, string>? headers = null)
        {
            this.Status = status;
            this.Body = body;
            this.Headers = headers ?? new Dictionary<string, string>();
        }

        /// <summary>The HTTP status code.</summary>
        internal int Status { get; }

        /// <summary>The response body.</summary>
        internal string Body { get; }

        /// <summary>Extra response headers.</summary>
        internal IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>A redirect to another URL.</summary>
        internal static LoopbackResponse Redirect(int status, string location)
        {
            return new LoopbackResponse(status, string.Empty, new Dictionary<string, string> { ["Location"] = location });
        }
    }
}
