
using System.Diagnostics;
using System.IO;

namespace MCClone
{
    class Console2
    {
        public static void Init()
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = "/k @echo off",
                WindowStyle = ProcessWindowStyle.Normal
            };

            Process p = Process.Start(psi);
            StreamWriter sw = p.StandardInput;
            StreamReader sr = p.StandardOutput;

            sw.WriteLine("Hello world!");
            sr.Close();
        }
    }
}
