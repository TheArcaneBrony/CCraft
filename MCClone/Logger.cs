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
                //string log = "";
                    while (LogQueue.Count > 0)
                    {
                        try
                        {
                            PostLog(LogQueue.Dequeue().Replace("\n\n", "\n").TrimEnd());
                            /*Task.Run(() => { if (LogQueue.Count != 0) try { */
                           // log += LogQueue.Dequeue() + '\n';
                           // if (log.Length >= 1024) { PostLog(log.Replace("\n\n", "\n").TrimEnd()); log = ""; }
                            /*
                            } catch { Debug.WriteLine("Failed to post log"); } });*/
                        }
                        catch
                        {
                            Debug.WriteLine("Failed to post log");
                        }
                    }
                    //if (log.Length >= 5) PostLog(log.Replace("\n\n", "\n"));
                Thread.Sleep(250);
            }
        });
        public static void CompressLog()
        {
            string log = "";
            while (LogQueue.Count > 0)
            {
                try
                {
                    /*Task.Run(() => { if (LogQueue.Count != 0) try { */
                    log += LogQueue.Dequeue() + '\n';
                    if (log.Length >= 1024) { Log("LogCompressor", log.Replace("\n\n", "\n").TrimEnd()); log = ""; }
                    /*
                    } catch { Debug.WriteLine("Failed to post log"); } });*/
                }
                catch
                {
                    Debug.WriteLine("Failed to compress log");
                }
            }
            Log("LogCompressor", log.Replace("\n\n", "\n").TrimEnd());
        }
        static int maxSrcLength = 0;
        public static void Log(string source, string log)
        {
            //if (Debugger.IsAttached) Debug.WriteLine(log);
#if CLIENT
            if (DataStore.activityViewer != null) DataStore.activityViewer.Dispatcher.Invoke(() => { DataStore.activityViewer.LogBox.AppendText(log + "\n"); });
#endif
#if TERRAIN_GEN_TEST
            Debug.WriteLine(log);
#else
            if (source.Length > maxSrcLength) maxSrcLength = source.Length;
            if (LogQueue.Count >= 1024) CompressLog();
            while (LogQueue.Count >= 16950) Thread.Sleep(25);
            LogQueue.Enqueue($"[{source.PadRight(maxSrcLength)}] {DateTime.Now.TimeOfDay.Hours}:{(DateTime.Now.TimeOfDay.Minutes + "").PadLeft(2, '0')}:{(DateTime.Now.TimeOfDay.Seconds + "").PadLeft(2, '0')}: {log}");
#endif
        }
        public static void Start()
        {
            logQueueThread.IsBackground = true;
            logQueueThread.Priority = ThreadPriority.Lowest;
            logQueueThread.Start();
        }
        private static readonly Uri postaddr = new Uri("http://thearcanebrony.net/Log/MCClone/Push.php");
        public static void PostLog(string Log)
        {/*
            Thread thr = new Thread(async () =>
            {*/
                if (Log == null) Log = "Corrupted log entry?";

                if (DataStore.Logging)
                {
                    HttpWebRequest request = WebRequest.CreateHttp(postaddr);
                    request.Method = "POST";
                    byte[] byteArray = Encoding.UTF8.GetBytes(Log);
                    request.ContentLength = byteArray.Length;
                    Stream dataStream =  request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    /*WebResponse response = request.GetResponse();
                    dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    Debug.WriteLine(responseFromServer);
                    reader.Close();
                    dataStream.Close();
                    response.Close();*/
                }
            /*});
            thr.Name = "Log thread, " + Log.Replace("\n", "");
            thr.Start();
            DataStore.Threads.Add(thr);*/
        }
    }
}
