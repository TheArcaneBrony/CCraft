using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace MCClone
{
    internal class Logger
    {
        public static Queue<string> LogQueue = new Queue<string>();
        private static Thread logQueueThread = new Thread(() =>
        {
            while (true)
            {
                if (Logger.LogQueue.Count != 0)
                {
                    while (Logger.LogQueue.Count != 0)
                    {
                        Logger.PostLog(Logger.LogQueue.Dequeue());
                    }
                }
                Thread.Sleep(150);
#if CLIENT
                Console.Title = $"{TerrainGen.runningThreads}/{TerrainGen.runningThreads + TerrainGen.waitingthreads} ({TerrainGen.waitingthreads} waiting) | A: {TerrainGen.threads.FindAll((Thread thr) => thr.ThreadState == ThreadState.Running).Count} W: {TerrainGen.threads.FindAll((Thread thr) => thr.ThreadState == ThreadState.WaitSleepJoin).Count} F: {TerrainGen.threads.FindAll((Thread thr) => thr.ThreadState == ThreadState.Stopped).Count} T: {TerrainGen.threads.Count}";
#endif
            }
        });
        public static void Start()
        {
            logQueueThread.IsBackground = true;
            logQueueThread.Priority = ThreadPriority.Lowest;
            logQueueThread.Start();
        }
        public static void PostLog(string Log)
        {
            if (true || Environment.MachineName != "TheArcaneBrony")
            {
                WebRequest request = WebRequest.Create("http://thearcanebrony.net/Log/MCClone/Push.php");
                request.Method = "POST";
                string postData = Log;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
            }
#if DEBUG
            Debug.WriteLine(Log);
#endif
#if SERVER
            Console.WriteLine(Log);
#endif
        }
    }
}
