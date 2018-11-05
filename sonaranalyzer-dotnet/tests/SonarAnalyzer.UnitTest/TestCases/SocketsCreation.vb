Imports System.Net.Sockets

Namespace Tests.TestCases
    Public Class Sockets

        ' RSpec example: https://jira.sonarsource.com/browse/RSPEC-4996
        Public Shared Sub Run()
            Dim socket As Socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
'                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Make sure that sockets are used safely here.}}

            ' TcpClient And UdpClient simply abstract the details of creating a Socket
            Dim client As TcpClient = New TcpClient("example.com", 80)
'                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ {{Make sure that sockets are used safely here.}}

            Dim listener As UdpClient = New UdpClient(80)
'                                       ^^^^^^^^^^^^^^^^^ {{Make sure that sockets are used safely here.}}
        End Sub

        Public Sub Tests(socket As Socket, tcp As TcpClient, udp As UdpClient)
            ' Ok to call other methods And properties
            socket.Accept()
            Dim isAvailable = tcp.Available
            udp.DontFragment = True

            'Creating of subclasses ís not checked
            Dim x As Object = New MySocket()
            x = New MyTcpClient()
            x = New MyUdpClient()
        End Sub
    End Class

    Public Class MySocket
        Inherits Socket
        Sub New()
            MyBase.New(Nothing)
        End Sub
    End Class

    Friend Class MyTcpClient
        Inherits TcpClient
    End Class

    Public Class MyUdpClient
        Inherits UdpClient
    End Class

End Namespace
