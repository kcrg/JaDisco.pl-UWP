using Newtonsoft.Json;
using PureWebSockets;
using System;
using System.Diagnostics;
using System.Net.WebSockets;

namespace Jadisco.Api
{
    public class JadiscoApi
    {
        private readonly PureWebSocket webSocket;
        private static readonly string WebSocketUrl = "wss://api.pancernik.info/notifier";
        private Data.ApiMessage localStorage = null;

        public event Action<Data.Topic> OnTopicChanged;
        public event Action<long> OnStreamViewersChanged;
        public event Action<Data.Service> OnStreamWentOnline;
        public event Action<Data.Service> OnStreamWentOffline;

        public Data.Streamer[] Streamers => localStorage?.Data.Streamers;
        public Data.Topic Topic => localStorage?.Data.Topic;
        public Data.Stream Stream => localStorage?.Data.Stream;

        public JadiscoApi()
        {
            try
            {
                PureWebSocketOptions socketOptions = new PureWebSocketOptions()
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
            Data.ApiMessage data = JsonConvert.DeserializeObject<Data.ApiMessage>(message);

            switch (data.Type)
            {
                case "status":
                    {
                        localStorage = data;

                        OnTopicChanged?.Invoke(data.Data.Topic);
                        OnStreamViewersChanged?.Invoke(data.Data.Stream.Viewers);

                        foreach (Data.Service stream in data.Data.Stream.Services)
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
                        // wait for status
                        if (localStorage is null)
                        {
                            break;
                        }

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
                            // invoke viewers change event only when stream is online
                            if (localStorage.Data.Stream.Status == true && localStorage.Data.Stream.Viewers != data.Data.Stream.Viewers)
                            {
                                OnStreamViewersChanged?.Invoke(data.Data.Stream.Viewers);

                                localStorage.Data.Stream.Viewers = data.Data.Stream.Viewers;
                            }
                            else if (localStorage.Data.Stream.Status == false)
                            {
                                localStorage.Data.Stream.Viewers = 0;
                            }

                            if (data.Data.Stream.Services != null)
                            {
                                for (int i = 0; i < data.Data.Stream.Services.Length; i++)
                                {
                                    Data.Service localService = localStorage.Data.Stream.Services[i];
                                    Data.Service remoteService = data.Data.Stream.Services[i];

                                    if (localService.Status != remoteService.Status)
                                    {
                                        if (remoteService.Status == true)
                                        {
                                            OnStreamWentOnline?.Invoke(remoteService);
                                        }
                                        else
                                        {
                                            OnStreamWentOffline?.Invoke(remoteService);
                                        }
                                    }
                                }

                                localStorage.Data.Stream.Status = data.Data.Stream.Status;
                                localStorage.Data.Stream.Services = data.Data.Stream.Services;

                                if (localStorage.Data.Stream.Status == true)
                                {
                                    localStorage.Data.Stream.OnlineAt = data.Data.Stream.OnlineAt;
                                }
                                else
                                {
                                    localStorage.Data.Stream.OfflineAt = data.Data.Stream.OfflineAt;
                                }
                            }
                        }

                        if (data.Data.Streamers != null)
                        {
                            localStorage.Data.Streamers = data.Data.Streamers;
                        }
                    }
                    break;
                case "ping":
                    {
                        webSocket.Send(JsonConvert.SerializeObject("{type:'pong'}"));
                    }
                    break;
            }
        }
    }
}