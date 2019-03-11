using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Logger.Start();

            ServerSocket.Start();
            Logger.LogQueue.Enqueue($">> Server Started on port 13372 ({DataStore.Ver})");
            new Thread(() =>
            {
                //serverSocket.Server.Listen(1000);
                while (true)
                {
                    try
                    {
                        while (!ServerSocket.Pending())
                        {
                            Thread.Sleep(5);
                        }

                        ClientSocket = ServerSocket.AcceptTcpClient();
                        //Logger.LogQueue.Enqueue($" >> Client No: {_counter++} started!");
                        Clients.Add(ClientSocket);
                        HandleClient.StartClient(ClientSocket, _counter);
                        _counter++;
                        new Task(() =>
                        {
                            new SoundPlayer(@"C:\Windows\Media\Speech On.wav").Play();
                        }).Start();

                    }
                    catch (Exception e)
                    {
                        Logger.LogQueue.Enqueue(JsonConvert.SerializeObject(e.Data, Formatting.Indented));
                    }
                }

            }).Start();
        }
        public static void BroadcastMessage(string message)
        {
            try
            {
                /* var msgSend = new Task(() =>
                 {*/
                byte[] sendBytes = Encoding.Unicode.GetBytes("\0SRVMSG\0ping".Replace("\n", "\0MSGEND\0") + "\0MSGEND\0");
                List<TcpClient> tmpCli = Clients;

                int oldNumcli = Clients.Count;
                Console.Title = tmpCli.Count + "";
                try
                {
                    foreach (TcpClient client in tmpCli)
                    {
                        try
                        {

                            NetworkStream networkStream = client.GetStream();
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                        }
                        catch
                        {
                            Clients.Remove(client);
                        }
                    }
                }
                catch { }
                sendBytes = Encoding.Unicode.GetBytes(message.Replace("\n", "\0MSGEND\0") + "\0MSGEND\0");
                try
                {
                    foreach (TcpClient client in tmpCli)
                    {
                        TcpClient cl = client;
                        try
                        {
                            NetworkStream networkStream = cl.GetStream();
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                        }
                        catch
                        {
                            Clients.Remove(cl);
                        }
                    }
                }
                catch (Exception)
                {
                    //  Logger.LogQueue.Enqueue(e);
                }
                if (Clients.Count < oldNumcli)
                {
                    Logger.LogQueue.Enqueue($"{oldNumcli - Clients.Count} client(s) have disconnected! Remaining: {Clients.Count}");
                }
                /* });
msgSend.Start();*/
                //Logger.LogQueue.Enqueue(" >> " + message);

            }
            catch
            {
                Logger.LogQueue.Enqueue("fuck this shit");
                // ignored
            }

            // InvokeQueue.Add(() => ConnectionCount.Text = $"Connected clients: {Clients.Count}");
        }
        public static void WhisperMessage(TcpClient Client, string Message)
        {
            try
            {
                byte[] sendBytes = Encoding.Unicode.GetBytes("*SYS*: " + Message.Replace("\n", "\0MSGEND\0") + "\0MSGEND\0");
                try
                {
                    NetworkStream networkStream = Client.GetStream();
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                }
                catch
                {
                    Clients.Remove(Client);
                    Logger.LogQueue.Enqueue("WTF? Whisper failed!");
                    //Program.BroadcastMessage("A client has logged off! (MessageTransmitFailedException!)");
                }
                Logger.LogQueue.Enqueue(" >> " + Message);
            }
            catch
            {
            }
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
                //Thread.Sleep(100);
                try
                {
                    requestCount = requestCount + 1;
                    string res = NetworkHelper.Receive(_clientSocket.GetStream()).Replace("\b\b","");
                    Console.BackgroundColor = ConsoleColor.Red;
                    Logger.LogQueue.Enqueue($"{_clNo}: {res}");
                    string[] parts = res.Split(' ');
                    switch (parts[0])
                    {
                        case "hello":
                            break;
                        case "getchunk":
                            string[] coords = parts[1].Split('.');
                            Debug.WriteLine($"|{(coords[0])}|, |{(coords[1])}|");
                            NetworkHelper.Send(_clientSocket.GetStream(), JsonConvert.SerializeObject(TerrainGen.GetChunk(Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]))));
                            break;
                        default:
                            Logger.LogQueue.Enqueue($"Received unknown command: {res}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw;
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
