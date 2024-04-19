using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Sockets
{
    public async Task Connect()
    {
        var hostEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 80);
        var buffer = new List<ArraySegment<byte>>();
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(hostEndPoint);                               // Noncompliant
        socket.Accept();                                            // Noncompliant
        socket.Receive(buffer, SocketFlags.Broadcast);              // Noncompliant
        socket.Send(buffer, SocketFlags.Broadcast);                 // Noncompliant
    }
}
