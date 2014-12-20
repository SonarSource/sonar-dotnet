using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System.Diagnostics;

    using ZeroMQ;
    using System.Text;
    using  System;

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            StartSubscriber(null);
        }
        

        static public void StartSubscriber(object state)
        {
            using (var context = ZmqContext.Create())
            {
                using (ZmqSocket subscriber = context.CreateSocket(SocketType.SUB),
                    syncClient = context.CreateSocket(SocketType.REQ))
                {
                    subscriber.Connect("tcp://localhost:5561");
                    subscriber.Subscribe(Encoding.Unicode.GetBytes(""));

                    syncClient.Connect("tcp://localhost:5562");

                    //  - send a synchronization request
                    syncClient.Send("Hello", Encoding.Unicode);
                    //  - wait for synchronization reply
                    syncClient.Receive(Encoding.Unicode);

                    return;
                    int receivedUpdates = 0;
                    while (true)
                    {
                        var data = subscriber.Receive(Encoding.Unicode);
                        try
                        {
                            var elems = data.Split(',');


                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }

                    Console.WriteLine("Received {0} updates.", receivedUpdates);
                }
            }
        }
    }
}
