using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace MCClone.Server
{
    internal class Program
    {
        public static List<TcpClient> Clients = new List<TcpClient>();
        public static Stopwatch StopWatch = new Stopwatch();
        public static TcpListener ServerSocket = new TcpListener(IPAddress.Any, 13372);
        public static TcpClient ClientSocket;
        private static int _counter;

        private static void Main(string[] args)
        {
            Console.Title = "MCClone Server -=- " + DataStore.Ver;
            Directory.CreateDirectory($"Worlds");
            Directory.CreateDirectory($"Mods");
            Logger.Start();
            ServerSocket.Start();
            Logger.LogQueue.Enqueue($">> Server Started on port 13372 ({DataStore.Ver})");
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        while (!ServerSocket.Pending())
                        {
                            Thread.Sleep(2000);
                        }
                        ClientSocket = ServerSocket.AcceptTcpClient();
                        Clients.Add(ClientSocket);
                        HandleClient.StartClient(ClientSocket, _counter);
                        _counter++;
                        new SoundPlayer(@"C:\Windows\Media\Speech On.wav").Play();
                    }
                    catch (Exception e)
                    {
                        Logger.LogQueue.Enqueue(JsonConvert.SerializeObject(e.Data, Formatting.Indented));
                    }
                }
            }).Start();
        }
    }
    public class HandleClient
    {
        public static TcpClient _clientSocket;
        public static int _clNo;
        public static void StartClient(TcpClient inClientSocket, int cliNo)
        {
            _clientSocket = inClientSocket;
            _clNo = cliNo;
            var ctThread = new Thread(DoChat);
            ctThread.Start();
        }
        private static void DoChat()
        {
            DateTime joinTime = DateTime.Now;
            int requestCount = 0;
            int error = 0;
            string username = $"User_{new Random().Next(0, 1000)}";
            Logger.LogQueue.Enqueue($"Client #{_clNo} connected, IP: {_clientSocket.Client.RemoteEndPoint}");
            while (error < 1)
            {
                try
                {
                    requestCount = requestCount + 1;
                    string res = NetworkHelper.Receive(_clientSocket.GetStream());
                    Console.BackgroundColor = ConsoleColor.Red;
                    Logger.LogQueue.Enqueue($"{_clNo}: {res}");
                    string[] parts = res.Split(' ');
                    switch (parts[0])
                    {
                        case "login":
                            Logger.LogQueue.Enqueue($"Client {_clNo} has authenticated with {parts[1]}:{parts[2]}");
                            break;
                        case "getchunk":
                            Task.Run(() =>
                            {
                                string[] coords = parts[1].Split('.');
                                Chunk ch = TerrainGen.GetChunk(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]));
                                while (!ch.Finished) Thread.Sleep(5);
                                NetworkHelper.Send(_clientSocket.GetStream(), JsonConvert.SerializeObject(new SaveChunk(ch)));
                            });
                            break;
                        default:
                            Logger.LogQueue.Enqueue($"Received unknown command: {res}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogQueue.Enqueue("Exception occurred with a client: " + ex.StackTrace);
                    _clientSocket.Close();
                    error++;
                }
            }
        }
    }
    public class Client
    {
        public Client() { }
    }
}
