/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SocketsCreationTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void SocketsCreation_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SocketsCreation.cs",
                new SocketsCreation(new TestAnalyzerConfiguration(null, "S4818")));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SocketsCreation_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SocketsCreation.vb",
                new SonarAnalyzer.Rules.VisualBasic.SocketsCreation(new TestAnalyzerConfiguration(null, "S4818")));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SocketsCreation_CS_RuleDisabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\SocketsCreation.cs",
                new SocketsCreation());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void SocketsCreation_VB_RuleDisabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\SocketsCreation.vb",
                new SonarAnalyzer.Rules.VisualBasic.SocketsCreation());
        }
    }
}

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

