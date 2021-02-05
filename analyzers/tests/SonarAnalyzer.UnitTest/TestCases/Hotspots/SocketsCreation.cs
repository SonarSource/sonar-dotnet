namespace Tests.Diagnostics
{
    using System.Net.Sockets;

    public class TestSocket
    {
        // RSpec example: https://jira.sonarsource.com/browse/RSPEC-4944
        public static void Run()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Make sure that sockets are used safely here.}}

            // TcpClient and UdpClient simply abstract the details of creating a Socket
            TcpClient client = new TcpClient("example.com", 80);
//                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  {{Make sure that sockets are used safely here.}}

            UdpClient listener = new UdpClient(80);
//                               ^^^^^^^^^^^^^^^^^  {{Make sure that sockets are used safely here.}}
        }


        public void Tests(Socket socket, TcpClient tcp, UdpClient udp)
        {
            // Ok to call other methods and properties
            socket.Accept();
            var isAvailable = tcp.Available;
            udp.DontFragment = true;

            // Creating of subclasses is not checked
            new MySocket();
            new MyTcpClient();
            new MyUdpClient();
        }
    }

    public class MySocket : Socket
    {
        public MySocket() : base(new SocketInformation()) { }
    }
    public class MyTcpClient : TcpClient { }
    public class MyUdpClient : UdpClient { }
}
