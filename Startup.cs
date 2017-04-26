using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Principal;

namespace webapi
{
    public class Startup
    {
        private ConcurrentDictionary<IPrincipal, WebSocket> _WebSockets = new ConcurrentDictionary<IPrincipal, WebSocket>();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.Use(async (http, next) =>
            {
                if (http.Request.Path == "/ws")
                {
                    if (http.WebSockets.IsWebSocketRequest)
                    {
                        var websocket = await http.WebSockets.AcceptWebSocketAsync();
                        _WebSockets.AddOrUpdate(http.User, websocket, (p, ws) => websocket);

                        await Echo(http, websocket);

                        WebSocket ows;
                        if (_WebSockets.TryRemove(http.User, out ows)) {
                            if (ows != websocket) {
                                // O_o Benutzer verbindet sich schon wieder.
                                // Zurück mit der Verbindung.
                                _WebSockets.AddOrUpdate(http.User, ows, (p, ws) => ows);
                            }
                        }
                    }
                    else
                    {
                        http.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
            app.UseMvc();
        }

        private async Task Echo(HttpContext context, WebSocket websocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                foreach (var ws in _WebSockets.Values)
                {
                    if (ws.State != WebSocketState.Open) continue;
                    await ws.SendAsync(
                        new ArraySegment<byte>(buffer, 0, result.Count),
                        result.MessageType,
                        result.EndOfMessage,
                        CancellationToken.None);
                }

                result = await websocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);
            }

            await websocket.CloseAsync(
                result.CloseStatus.Value,
                result.CloseStatusDescription,
                CancellationToken.None);
        }
    }
}
