using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    internal class Logger
    {
        public static Queue<string> LogQueue = new Queue<string>();
        public static readonly Thread logQueueThread = new Thread(() =>
        {
            while (true)
            {
                while (LogQueue.Count != 0)
                {
                    try
                    {
                        /*Task.Run(() => { if (LogQueue.Count != 0) try { */
                        PostLog(LogQueue.Dequeue());/*
                        } catch { Debug.WriteLine("Failed to post log"); } });*/
                    }
                    catch
                    {
                        Debug.WriteLine("Failed to post log");
                    }
                }
                Thread.Sleep(250);
            }
        });
        public static void Start()
        {
            logQueueThread.IsBackground = true;
            logQueueThread.Priority = ThreadPriority.Lowest;
            logQueueThread.Start();
        }
        private static readonly Uri postaddr = new Uri("http://thearcanebrony.net/Log/MCClone/Push.php");
        public static void PostLog(string Log)
        {
            if (Debugger.IsAttached) Debug.WriteLine(Log);
            if (DataStore.Logging)
            {
                WebRequest request = WebRequest.Create(postaddr);
                request.Method = "POST";
                string postData = Log;
                byte[] byteArray = Encoding.UTF8.GetBytes(DateTime.Now + ": "+postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Debug.WriteLine(responseFromServer);
                reader.Close();
                dataStream.Close();
                response.Close();
            }
#if CLIENT
            //if (DataStore.activityViewer != null) DataStore.activityViewer.Dispatcher.Invoke(() => { DataStore.activityViewer.LogBox.AppendText(Log + "\n"); });
#endif
        }
    }
}
