using System;
using OpenTK;

namespace MCClone
{
    public class Mod
    {
        public void OnLoad(EventArgs e)
        {
            Console.WriteLine("Test mod loaded!");
        }
        public void OnResize(EventArgs e)
        {
            Console.Write("Window Resized!");
        }
        public void OnRenderFrame(EventArgs e)
        {

        }
        public void OnUpdateFrame(FrameEventArgs e)
        {

        }
    }
}
