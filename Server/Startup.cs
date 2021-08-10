using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            websocks = new ConcurrentBag<WebSocket>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
            app.UseWebSockets(wsOptions);
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/send")
                {
                    Console.WriteLine("NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW CONNECTION.");

                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            websocks.Add(webSocket);
                            await Send(context, webSocket);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            });
        }

        private static int counter = 0;
        private static ConcurrentBag<WebSocket> websocks;

        private async Task Send(HttpContext context, WebSocket webSocket)
        {
            int n = counter++;
            Console.WriteLine($"We're in SEND SEND SEND number {n}");

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                System.Threading.CancellationToken.None);
            if (result is not null)
            {
                while(!result.CloseStatus.HasValue)
                {
                    string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer,0,result.Count));
                    Console.WriteLine($"client says {msg}");
                    string resp = $"{DateTime.Now}:: " + msg;
                    foreach (var wb in websocks)
                    {
                        try
                        {
                            if (!wb.CloseStatus.HasValue)
                                await wb.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(resp)),
                                result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            ; // do something here - we want to remove it.  probably need a key.
                        }
                    }
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                    Console.WriteLine(result);
                }
            }
            await webSocket.CloseAsync(result.CloseStatus.Value,
                result.CloseStatusDescription,
                System.Threading.CancellationToken.None);
        }
    }
}
