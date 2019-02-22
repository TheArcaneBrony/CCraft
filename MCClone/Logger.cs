using System.Collections.Generic;
using System.Diagnostics;

namespace MCClone
{
    internal class Logger
    {
        public static Queue<string> LogQueue = new Queue<string>();
        public static void PostLog(string Log)
        {
            /*if (Environment.MachineName != "TheArcaneBrony")
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
                reader.Close();
                dataStream.Close();
                response.Close();
            }*/
#if DEBUG
            Debug.WriteLine(Log);
#endif
        }
    }
}
