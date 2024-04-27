using System.Net;
using System.Net.Sockets;

const int HttpPort = 4221;

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, HttpPort);
server.Start();
server.AcceptSocket(); // wait for client
