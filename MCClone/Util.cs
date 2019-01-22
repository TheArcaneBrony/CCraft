using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCClone
{
    class Util
    {
        public static double DegToRad(double deg) => (Math.PI * deg) / 180;
        public static string GetGameArg(string key)
        {
            int ArgIndex = Array.FindIndex(Program.args, (String s) => { if (s == $"-{key}") return true; else return false; });
            if (ArgIndex != -1)
                return Program.args[ArgIndex + 1];
            else return "null";
        }
        public static bool ShouldRenderChunk(Chunk ch)
        {
            // return true;
            if (!(MainWindow.world.Player.X / 16 + MainWindow.renderDistance > ch.X && MainWindow.world.Player.X / 16 - MainWindow.renderDistance < ch.X)) return false;
            if (!(MainWindow.world.Player.Z / 16 + MainWindow.renderDistance > ch.Z && MainWindow.world.Player.Z / 16 - MainWindow.renderDistance < ch.Z)) return false;
            if (-90 - 45 <= MainWindow.world.Player.LX && MainWindow.world.Player.LX <= 90 + 45 && ch.X >= MainWindow.world.Player.X / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= 90 - 45 && MainWindow.world.Player.LX >= -180 + 45 && ch.Z >= MainWindow.world.Player.Z / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX <= 180 - 45 && MainWindow.world.Player.LX <= -90 + 45 && ch.X <= MainWindow.world.Player.X / 16)
            {
                return true;
            }
            else if (MainWindow.world.Player.LX >= -90 && MainWindow.world.Player.LX <= 0 && ch.X >= MainWindow.world.Player.X / 16)
            {
                // return true;
            }
            return false;
        }
        public static object deserializeToDictionaryOrList(string jo, bool isArray = false)
        {
            if (!isArray)
            {
                isArray = jo.Substring(0, 1) == "[";
            }
            if (!isArray)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
                var values2 = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> d in values)
                {
                    if (d.Value is JObject)
                    {
                        values2.Add(d.Key, deserializeToDictionaryOrList(d.Value.ToString()));
                    }
                    else if (d.Value is JArray)
                    {
                        values2.Add(d.Key, deserializeToDictionaryOrList(d.Value.ToString(), true));
                    }
                    else
                    {
                        values2.Add(d.Key, d.Value);
                    }
                }
                return values2;
            }
            else
            {
                var values = JsonConvert.DeserializeObject<List<object>>(jo);
                var values2 = new List<object>();
                foreach (var d in values)
                {
                    if (d is JObject)
                    {
                        values2.Add(deserializeToDictionaryOrList(d.ToString()));
                    }
                    else if (d is JArray)
                    {
                        values2.Add(deserializeToDictionaryOrList(d.ToString(), true));
                    }
                    else
                    {
                        values2.Add(d);
                    }
                }
                return values2;
            }
        }
        private Dictionary<string, object> deserializeToDictionary(string jo)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> d in values)
            {
                if (d.Value is JObject)
                {
                    values2.Add(d.Key, deserializeToDictionary(d.Value.ToString()));
                }
                else
                {
                    values2.Add(d.Key, d.Value);
                }
            }
            return values2;
        }
    }
    public class SystemUtils
    {
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
    }
}
