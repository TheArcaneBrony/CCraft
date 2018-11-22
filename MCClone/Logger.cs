using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace MCClone
{
    class Logger
    {
        public static List<string> LogQueue = new List<string>();
        public static void PostLog(string Log)
        {
            /*
            if (Environment.MachineName != "TheArcaneBrony")
            {
                WebRequest request = WebRequest.Create("http://thearcanebrony.ddns.net/Log/MCClone/Push.php");
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
                //Console.Write(responseFromServer);
                reader.Close();
                dataStream.Close();
                response.Close();
            }*/

            Debug.WriteLine(Log);
        }
    }
}
