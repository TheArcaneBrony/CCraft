using System.Reflection;

namespace MCClone
{
    public class DataStore
    {
        public static string Ver = "Alpha 0.08_01200";
        public static bool Multiplayer =
#if SERVER
            true,
#else
            false,
#endif
            Server =
#if SERVER
            true;
#else
            true;
#endif
    }
    public class ModData
    {
        public string Name { get; set; } = "Unnamed mod";
        public object Instance { get; set; } = null;
        public MethodInfo OnLoad { get; set; } = null;
        public MethodInfo OnResize { get; set; } = null;
        public MethodInfo OnUpdateFrame { get; set; } = null;
        public MethodInfo OnRenderFrame { get; set; } = null;
    }
}
