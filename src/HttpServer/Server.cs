using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpServer.Constants;
using HttpServer.Enums;

namespace HttpServer
{
    public class Server
    {
        private TcpListener server = new TcpListener(IPAddress.Any, HttpConstants.HttpPort);

        private string GetPath(string request)
        {
            return request.Split("\r\n")[0].Split(' ')[1];
        }

        private HttpRequestPath GetRequestPath(string path)
        {
            if (path == HttpConstants.RootPath)
            {
                return HttpRequestPath.Root;
            }

            if (path == HttpConstants.UserAgentPath)
            {
                return HttpRequestPath.UserAgent;
            }

            if (path.StartsWith(HttpConstants.EchoPath))
            {
                return HttpRequestPath.Echo;
            }

            return HttpRequestPath.NotFound;
        }

        private async Task<string> ProcessRequest(Socket socket)
        {
            var buffer = new byte[1024];
            var received = await socket.ReceiveAsync(buffer);
            var request = Encoding.UTF8.GetString(buffer, 0, received);
            return request;
        }

        private void EndConnection(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Dispose();
        }

        private async Task SendResponse(Socket socket, string response)
        {
            var buffer = Encoding.UTF8.GetBytes(response);
            await socket.SendAsync(buffer);
        }

        private async Task ProcessOkResponse(Socket socket)
        {
            var response = "HTTP/1.1 200 OK\r\n\r\n";
            await SendResponse(socket, response);
            EndConnection(socket);
        }

        private String GetEcho(String path)
        {
            return path.Substring(HttpConstants.EchoPath.Length);
        }

        private async Task ProcessEchoResponse(Socket socket, string path)
        {
            var echo = GetEcho(path);
            var response = String.Format(
                "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {0}\r\n\r\n{1}",
                echo.Length,
                echo
            );
            await SendResponse(socket, response);
            EndConnection(socket);
        }

        private string getUserAgent(string request)
        {
            return request
                .Split("\r\n")
                .Where(x => x.StartsWith("User-Agent"))
                .FirstOrDefault("")
                .Split(" ")[1];
        }

        private async Task ProcessUserAgentResponse(Socket socket, string request)
        {
            var userAgent = getUserAgent(request);
            var response = String.Format(
                "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {0}\r\n\r\n{1}",
                userAgent.Length,
                userAgent
            );
            await SendResponse(socket, response);
            EndConnection(socket);
        }

        private async Task ProcessNotFoundResponse(Socket socket)
        {
            var response = "HTTP/1.1 404 Not Found\r\n\r\n";
            await SendResponse(socket, response);
            EndConnection(socket);
        }

        public async Task Start()
        {
            server.Start();
            Console.WriteLine("Server listening on port {0}", HttpConstants.HttpPort);
            while (true)
            {
                var socket = await server.AcceptSocketAsync();
                var request = await ProcessRequest(socket);
                var path = GetRequestPath(GetPath(request));
                switch (path)
                {
                    case HttpRequestPath.Root:
                        await ProcessOkResponse(socket);
                        break;
                    case HttpRequestPath.Echo:
                        await ProcessEchoResponse(socket, GetPath(request));
                        break;
                    case HttpRequestPath.UserAgent:
                        await ProcessUserAgentResponse(socket, request);
                        break;
                    case HttpRequestPath.NotFound:
                        await ProcessNotFoundResponse(socket);
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            new Server().Start().Wait();
        }
    }
}
