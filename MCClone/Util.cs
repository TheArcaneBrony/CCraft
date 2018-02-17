using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
