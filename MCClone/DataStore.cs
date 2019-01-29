using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCClone
{
    class DataStore
    {
    }
    class Mod
    {
        public MethodInfo OnLoad { get; set; }
        public MethodInfo OnResize { get; set; }
        public MethodInfo OnUpdateFrame { get; set; }
        public MethodInfo OnRenderFrame { get; set; }
    }
}
