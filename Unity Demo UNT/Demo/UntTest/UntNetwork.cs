using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using Unt;

namespace Unt.Demo.Test
{
    public class UntNetwork : MonoBehaviour
    {
        [Header("Network")]
        public string IpAddress = "127.0.0.1";
        public ushort Port = 12700;

        public ushort ClientPing;

        [Header("HUD")]
        public int offsetX = 10;
        public int offsetY = 10;

        private NetServer server;
        private NetClient client;

        private void Start()
        {
            Log.Initialize(Debug.Log, Debug.LogWarning, Debug.LogError);

            server = new NetServer();
            server.OnHandler = ServerHandler;
            server.OnClientConnected = ServerClientConnected;
            server.OnClientDisconnected = ServerClientDisconnected;

            client = new NetClient();
            client.OnHandler = ClientHandler;
            client.OnConnected = ClientConnected;
            client.OnDisconnected = ClientDisconnected;
        }

        private void ServerHandler(byte[] data, int length, bool isReliable, EndPoint endPoint)
        {
            Debug.Log("(Server) Handler " + (isReliable == false ? NetChennel.Unreliable : NetChennel.Reliable) + " " + BitConverter.ToInt32(data, 0));
        }

        private void ServerClientConnected(EndPoint endPoint)
        {
            Debug.Log("(Server) Client Connected " + endPoint);
        }

        private void ServerClientDisconnected(EndPoint endPoint)
        {
            Debug.Log("(Server) Client Disconnected " + endPoint);
        }

        private void ClientHandler(byte[] data, int length, bool isReliable)
        {
            Debug.Log("(Client) Handler " + (isReliable == false ? NetChennel.Unreliable : NetChennel.Reliable) + " " + BitConverter.ToInt32(data, 0));
        }

        private void ClientConnected()
        {
            Debug.Log("(Client) Connected");
        }

        private void ClientDisconnected()
        {
            Debug.Log("(Client) Disconnected");
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(offsetX, offsetY, 225, 9999));
            
            Server();
            Client();

            GUILayout.EndArea();
        }

            private void Server()
            {
                if (!server.IsRuning)
                {
                    if (GUILayout.Button("Start Server"))
                    {
                        server.Start(Port);
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Server"))
                    {
                        server.Stop();
                    }

                    if (GUILayout.Button("SendAll Unreliable"))
                    {
                        server.SendAll(BitConverter.GetBytes(12345678), sizeof(int), false);
                    }

                    if (GUILayout.Button("SendAll Reliable"))
                    {
                        server.SendAll(BitConverter.GetBytes(12345678), sizeof(int), true);
                    }
                }
            }

            private void Client()
            {
                if (client.Status == Status.NoConnect)
                {
                    if (GUILayout.Button("Connect"))
                    {
                        client.Connect(IpAddress, Port);
                    }
                }
                else if (client.Status == Status.Connecting)
                {
                    if (GUILayout.Button("Stop Connecting"))
                    {
                        client.Stop();
                    }
                }
                else if (client.Status == Status.Connected)
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        client.Disconnect(true);
                    }

                    if (GUILayout.Button("Send Unreliable"))
                    {
                        client.Send(BitConverter.GetBytes(12345678), sizeof(int), false);
                    }

                    if (GUILayout.Button("Send Reliable"))
                    {
                        client.Send(BitConverter.GetBytes(12345678), sizeof(int), true);
                    }
                }
                else if (client.Status == Status.Disconnecting)
                {
                    if (GUILayout.Button("Disconnecting"))
                    {
                        
                    }
                }
            }

            private void FixedUpdate()
            {
                ClientPing = client.Ping;
            }

            private void OnApplicationQuit()
            {
                server?.Stop();
                client?.Stop();
            }
    }
}
