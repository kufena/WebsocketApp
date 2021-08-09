using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceURI = new Uri("ws://localhost:5000/send");
                var cTs = new CancellationTokenSource();
                cTs.CancelAfter(TimeSpan.FromSeconds(120));
                try
                {
                    await client.ConnectAsync(serviceURI, cTs.Token);
                    var n = 0;
                    while (client.State == WebSocketState.Open)
                    {
                        Console.WriteLine("enter message:");
                        string s = Console.ReadLine();

                        await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(s)), 
                            WebSocketMessageType.Text, 
                            true,
                            cTs.Token);
                        var responseBuffer = new byte[1024];
                        var offset = 0;
                        var packet = 1024;
                        while(true)
                        {
                            var byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                            var response = await client.ReceiveAsync(byteReceived, cTs.Token);
                            var responseMsg = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                            Console.WriteLine(responseMsg);

                            if (response.EndOfMessage)
                                break;
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
