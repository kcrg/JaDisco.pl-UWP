using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using PureWebSockets;

namespace JaDisco_UWP.Api
{
    public class JadiscoApi
    {
        PureWebSocket webSocket;

        public event Action<Data.Message> OnStatusChanged;

        public JadiscoApi()
        {
            try
            {
                var socketOptions = new PureWebSocketOptions()
                {
                    DebugMode = false,
                    MyReconnectStrategy = new ReconnectStrategy(2000, 4000, 20)
                };

                webSocket = new PureWebSocket("wss://api.pancernik.info/notifier", socketOptions);

                webSocket.OnMessage += WebSocket_OnMessage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WS] Error: {ex.Message}");
            }
        }

        public void Connect()
        {
            webSocket.Connect();
        }

        private void WebSocket_OnMessage(string message)
        {
            var obj = JsonConvert.DeserializeObject<Data.Message>(message);

            switch (obj.Type)
            {
                case "status":
                {
                    OnStatusChanged?.Invoke(obj);
                } break;
                /*case "update":
                {

                } break;*/
                case "ping":
                {
                    Debug.WriteLine("Sent pong!");
                    webSocket.Send(JsonConvert.SerializeObject("{type:'pong'}"));
                } break;
            }
        }
    }
}
