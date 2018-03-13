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
        public static double DegToRad(double deg)
        {
            return (Math.PI / 180) * deg;
        }
        public static string GetGameArg(string key)
        {
            int test = Array.FindIndex(Program.args, (String s) => { if (s == $"-{key}") return true; else return false; });
            if (test != -1)
                return Program.args[test + 1];
            else return "null";
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
