using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MCClone
{
    class NetworkHelper
    {
        public static void Send(NetworkStream networkStream, string str)
        {
            var outStream = Encoding.Unicode.GetBytes($"{str}\b\b");
            networkStream.Write(outStream, 0, outStream.Length);
        }
        public static string Receive(NetworkStream networkStream)
        {
            string res = "";
            do
            {
                byte[] bytesFrom = new byte[2];
                networkStream.Read(bytesFrom, 0, 2);
                res += Encoding.Unicode.GetString(bytesFrom);
            } while (!res.Contains("\b\b"));
            return res.Replace("\b\b","");
        }
    }
}
