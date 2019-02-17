﻿using System.Reflection;

namespace MCClone
{
    internal class DataStore
    {
    }
    public class Mod
    {
        public string Name { get; set; } = "Unnamed mod";
        public object Instance { get; set; } = null;
        public MethodInfo OnLoad { get; set; } = null;
        public MethodInfo OnResize { get; set; } = null;
        public MethodInfo OnUpdateFrame { get; set; } = null;
        public MethodInfo OnRenderFrame { get; set; } = null;
    }
}
