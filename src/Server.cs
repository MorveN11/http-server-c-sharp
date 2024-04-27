using System.Net;
using System.Net.Sockets;
using System.Text;

const int HttpPort = 4221;

TcpListener server = new TcpListener(IPAddress.Any, HttpPort);

server.Start();

async Task ProcessRequest(Socket socket)
{
    var buffer = new byte[1024];
    var received = await socket.ReceiveAsync(buffer);
    var request = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine(request);
}

async Task ProcessResponse(Socket socket)
{
    var response = "HTTP/1.1 200 OK\r\n\r\n";
    var buffer = Encoding.UTF8.GetBytes(response);
    await socket.SendAsync(buffer);
    socket.Shutdown(SocketShutdown.Both);
    socket.Dispose();
}

while (true)
{
    var socket = await server.AcceptSocketAsync();
    await ProcessRequest(socket);
    await ProcessResponse(socket);
}
