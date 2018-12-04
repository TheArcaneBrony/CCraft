using NAudio.Wave;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MCClone
{
    class Util
    {
        public static double DegToRad(double deg) => (Math.PI * deg) / 180;
        public static string GetGameArg(string key)
        {
            int test = Array.FindIndex(Program.args, (String s) => { if (s == $"-{key}") return true; else return false; });
            if (test != -1)
                return Program.args[test + 1];
            else return "null";
        }
        public static bool ShouldRenderChunk(Chunk ch)
        {
            bool s1=false, s2=false;
            int tx = ((int)MainWindow.world.Player.X / 16);
                int tz = ((int)MainWindow.world.Player.Z / 16);
            if (tx + MainWindow.renderDistance > ch.X && tx - MainWindow.renderDistance < ch.X )
                if (tz + MainWindow.renderDistance > ch.Z && tz - MainWindow.renderDistance < ch.Z)
                s1=true;
             /*if(MainWindow.world.Player == null)
             {

             }*/

            s2 = true;
            return s1 && s2;
        }

    }
    public class MyAudioWrapper
    {
        [DllImport("winmm.dll", EntryPoint = "waveOutGetVolume")]
        public static extern void GetWaveVolume(IntPtr devicehandle, out int Volume);

    }
    class PCMPlayer
    {
        private WaveFormat waveFormat=new WaveFormat(8000,32,1);
    private BufferedWaveProvider bufferedWaveProvider;

    public PCMPlayer()
    {
        bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
    }

    public void AddSamples(byte[] array)
    {
        bufferedWaveProvider.AddSamples(array, 0, array.Length);
        WaveOut waveout = new WaveOut();
        waveout.Init(bufferedWaveProvider);

        waveout.Play();
    }
}
}
