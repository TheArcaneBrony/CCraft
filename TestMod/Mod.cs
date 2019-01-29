using System;

namespace MCClone
{
    public class Mod
    {
        public void OnLoad(EventArgs e)
        {
            Console.WriteLine("Test mod loaded!");
        }
        protected void OnResize(EventArgs e)
        {
            Console.Write("Window Resized!");
        }
    }
}
