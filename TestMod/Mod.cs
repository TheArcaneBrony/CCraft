using System;

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
        public void OnUpdateFrame(FrameUpdateArgs e)
        {
            Console.WriteLine($"{MainWindow.world.Name}");
        }
    }
}
