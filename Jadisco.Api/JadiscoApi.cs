using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.WebSockets;

using Newtonsoft.Json;
using PureWebSockets;

namespace Jadisco.Api
{
    public class JadiscoApi
    {
        PureWebSocket webSocket;

        static readonly string WebSocketUrl = "wss://api.pancernik.info/notifier";

        Data.ApiMessage localStorage = null;

        public event Action<Data.Topic> OnTopicChanged;
        public event Action<long> OnStreamViewsChanged;
        public event Action<Data.Service> OnStreamWentOnline;
        public event Action<Data.Service> OnStreamWentOffline;

        public Data.Streamer[] Streamers { get { return localStorage?.Data.Streamers; } }
        public Data.Topic Topic { get { return localStorage?.Data.Topic; } }
        public Data.Stream Stream { get { return localStorage?.Data.Stream; } }

        public JadiscoApi()
        {
            try
            {
                var socketOptions = new PureWebSocketOptions()
                {
                    DebugMode = false,
                    MyReconnectStrategy = new ReconnectStrategy(2000, 4000, 20)
                };

                webSocket = new PureWebSocket(WebSocketUrl, socketOptions);
                webSocket.OnMessage += WebSocket_OnMessage;
                webSocket.OnOpened += WebSocket_OnOpened;
                webSocket.OnClosed += WebSocket_OnClosed;
                webSocket.OnError += WebSocket_OnError;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WS] Error: {ex.Message}");
            }
        }

        public void Connect()
        {
            webSocket.Connect();
            Debug.WriteLine($"[WS] Connecting to {WebSocketUrl}...");
        }

        private void WebSocket_OnOpened()
        {
            Debug.WriteLine("[WS] Connected!");
        }

        private void WebSocket_OnClosed(WebSocketCloseStatus reason)
        {
            Debug.WriteLine($"[WS] Disconnected! ({reason})");
        }

        private void WebSocket_OnError(Exception ex)
        {
            Debug.WriteLine($"[WS] Error: {ex.Message}");
        }

        private void WebSocket_OnMessage(string message)
        {
            var data = JsonConvert.DeserializeObject<Data.ApiMessage>(message);

            switch (data.Type)
            {
                case "status":
                {
                    localStorage = data;

                    OnTopicChanged?.Invoke(data.Data.Topic);
                    OnStreamViewsChanged?.Invoke(data.Data.Stream.Viewers);

                    foreach (var stream in data.Data.Stream.Services)
                    {
                        if (stream.Status == true)
                        {
                            OnStreamWentOnline?.Invoke(stream);
                        }
                    }
                }
                break;
                case "update":
                {
                    if (data.Data.Topic != null)
                    {
                        if (localStorage.Data.Topic.Id != data.Data.Topic.Id)
                        {
                            OnTopicChanged?.Invoke(data.Data.Topic);
                        }

                        localStorage.Data.Topic = data.Data.Topic;
                    }

                    if (data.Data.Stream != null)
                    {
                        if (localStorage.Data.Stream.Viewers != data.Data.Stream.Viewers)
                        {
                            OnStreamViewsChanged?.Invoke(data.Data.Stream.Viewers);
                        }

                        for (int i = 0; i < data.Data.Stream.Services.Length; i++)
                        {
                            var localService = localStorage.Data.Stream.Services[i];
                            var remoteService = data.Data.Stream.Services[i];

                            if (localService.Status != remoteService.Status)
                            {
                                if (remoteService.Status == true)
                                    OnStreamWentOnline?.Invoke(remoteService);
                                else
                                    OnStreamWentOffline?.Invoke(remoteService);
                            }
                        }

                        localStorage.Data.Stream = data.Data.Stream;
                    }

                    if (data.Data.Streamers != null)
                    {
                        localStorage.Data.Streamers = data.Data.Streamers;
                    }
                } break;
                case "ping":
                {
                    webSocket.Send(JsonConvert.SerializeObject("{type:'pong'}"));
                } break;
            }
        }
    }
}
