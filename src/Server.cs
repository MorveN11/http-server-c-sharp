using System.Net;
using System.Net.Sockets;
using System.Text;

const int HttpPort = 4221;

TcpListener server = new TcpListener(IPAddress.Any, HttpPort);

server.Start();

String getPath(String request)
{
    return request.Split(' ')[1];
}

async Task<bool> IsValidRequest(Socket socket)
{
    var buffer = new byte[1024];
    var received = await socket.ReceiveAsync(buffer);
    var request = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine(request);
    var path = getPath(request);
    return path == "/";
}

void EndConnection(Socket socket)
{
    socket.Shutdown(SocketShutdown.Both);
    socket.Dispose();
}

async Task SendResponse(Socket socket, string response)
{
    var buffer = Encoding.UTF8.GetBytes(response);
    await socket.SendAsync(buffer);
}

async Task ProcessOkResponse(Socket socket)
{
    var response = "HTTP/1.1 200 OK\r\n\r\n";
    await SendResponse(socket, response);
    EndConnection(socket);
}

async Task ProcessNotFoundResponse(Socket socket)
{
    var response = "HTTP/1.1 404 Not Found\r\n\r\n";
    await SendResponse(socket, response);
    EndConnection(socket);
}

while (true)
{
    var socket = await server.AcceptSocketAsync();
    bool isValidRequest = await IsValidRequest(socket);
    if (isValidRequest)
    {
        await ProcessOkResponse(socket);
    }
    else
    {
        await ProcessNotFoundResponse(socket);
    }
}
