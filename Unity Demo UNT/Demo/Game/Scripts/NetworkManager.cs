using Unt.Demo.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unt.Demo.Game
{
    public class NetworkManager : MonoBehaviour
    {
        public string IpAddress = "127.0.0.1";
        public ushort Port = 12700;

        [Header("HUD")]
        public int offsetX = 10;
        public int offsetY = 10;
        public Text PingText;

        public static Server Server;
        public static Client Client;

        private bool onliClient;
        private bool isPhone;
        
        private void Awake()
        {
            Server = new Server();
            Client = new Client();
        }

        private void Start()
        {
            Client.IsTick = true;
            Client.OnDisconnected += Disconnected;
        }

        private void FixedUpdate()
        {
            PingText.text = $"Ping: {Client.Ping}";

            Client.Tick();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(offsetX, offsetY, 225, 9999));

            if (!onliClient)
                HostGUI();

            if (!Server.IsRuning)
                ClientGUI();

            GUILayout.EndArea();
        }

        private void HostGUI()
        {
            if (!Server.IsRuning)
            {
                if (GUILayout.Button("Start Host"))
                {
                    Server.Start(Port);
                    Client.Connect("127.0.0.1", Port);
                }
            }
            else
            {
                if (GUILayout.Button("Stop Host"))
                {
                    Client.Disconnect();
                    Server.Stop();
                }
            }
        }

        private void ClientGUI()
        {
            if (Client.IsNoConnect)
            {
                if (GUILayout.Button("Connect"))
                {
                    onliClient = true;
                    Client.Connect(IpAddress, Port);
                }

                GUILayout.BeginHorizontal();
                IpAddress = GUILayout.TextField(IpAddress);

                try { Port = ushort.Parse(GUILayout.TextField(Port.ToString())); }   
                catch (System.Exception) { }
                
                GUILayout.EndHorizontal();

            }
            else if (Client.IsConnecting)
            {
                if (GUILayout.Button("Connecting Stop"))
                {
                    Client.Disconnect();
                    onliClient = false;
                }
            }
            else if (Client.IsConnected)
            {
                if (GUILayout.Button("Disconnect"))
                {
                    Client.Disconnect(true);
                }

            }
            else if (Client.IsDisconnecting)
            {
                if (GUILayout.Button("Disconnecting..."))
                {
                    
                }
            }
        }

        private void Disconnected()
        {
            onliClient = false;
        }

        private void OnApplicationQuit()
        {
            Server.Stop();
            Client.Disconnect();
        }
    }
}