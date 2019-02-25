using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Media;
using System.Threading;

namespace MCClone.Server
{
    internal class Program
    {
        public static List<TcpClient> Clients = new List<TcpClient>();
        public static Stopwatch StopWatch = new Stopwatch();
        public static TcpListener ServerSocket = new TcpListener(IPAddress.Any, 8888);
        public static TcpClient ClientSocket;
        private static int _counter;

        private static void Main(string[] args)
        {
            Console.Title = "TheArcaneChat Server -=- v1.0.0";

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            ServerSocket.Start();
            Logger.LogQueue.Enqueue(" >> Server Started");
            //Visible = false;
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
                    catch (Exception)
                    {
                        //Logger.LogQueue.Enqueue(exception);
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
                // ignored
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
                Thread.Sleep(100);
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = _clientSocket.GetStream();
                    byte[] bytesFrom = new byte[_clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    string dataFromClient = Encoding.Unicode.GetString(bytesFrom).TrimEnd('\0');
                    Logger.LogQueue.Enqueue($"#{_clNo}: \"{dataFromClient}\"");
                    if (dataFromClient.StartsWith("\0CLIMSG\0"))
                    {
                        switch (dataFromClient.Replace("\0CLIMSG\0", ""))
                        {
                            case "exit":
                            //    Clients.Remove(_clientSocket);
                                _clientSocket.Close();
                                Program.BroadcastMessage($"Client { _clNo } logged off!");
                                break;
                        }

                    }
                    else
                    {
                        Program.BroadcastMessage($"{_clNo} - {username}: {dataFromClient}");
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
