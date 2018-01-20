using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MCClone
{
    class MainWindow : GameWindow
    {
        string progress = "";
        int renderDistance = 128;
        double brightness = 1 / 1;
        static Dictionary<Tuple<double, double>, Block> blockStorage = new Dictionary<Tuple<double, double>, Block>();
        bool focussed = true;
        int centerX, centerY;
        double cxv = 0, cyv = 0, czv = 0;

        public static void GenerateLine(Dictionary<Tuple<double, double>, Block> blockStorage, double xt)
        {
            for (double z = -100; z < 100; z += 1)
            {
                var blockPos = new Tuple<double, double>(xt, z);
                if (!blockStorage.ContainsKey(blockPos)) blockStorage.Add(blockPos, new Block());
                blockStorage[blockPos].Y = (int)(Math.Sin(xt / 20) + Math.Sin(z / 10)) % 10;

                //Console.WriteLine((xt + "").PadLeft(6) + " | " + (z + "").PadLeft(6));
                //Thread.Sleep(1);
                // Thread.Sleep(new TimeSpan(15000));
                //   progress = " | " + ((xt + "").PadLeft(6) + " | " + (z + "").PadLeft(6));
                // Console.Title = blockStorage.Count + progress;
            }
            //Thread.Sleep(15);
        }

        public MainWindow()
    : base(1280, // initial width
        720, // initial height
        GraphicsMode.Default,
        "♥ Fikou/Emma ♥",  // initial title
        GameWindowFlags.Default,
        DisplayDevice.Default,
        4, // OpenGL major version
        0, // OpenGL minor version
        GraphicsContextFlags.ForwardCompatible)
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
            VSync = VSyncMode.Off;
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(/*(float)Math.PI / 4*/ 0.9f, Width / (float)Height, 1.0f, 6400f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }
        [MTAThread]
        protected async override void OnLoad(EventArgs e)
        {
            CursorVisible = false;
            GL.Enable(EnableCap.DepthTest);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;
            Mouse.Move += Mouse_Move;
            Thread worldGen = new Thread(new ThreadStart(() =>
            {
                for (double x = -100; x < 100; x += 1)
                {
                    Console.Title = x + "";
                    Thread childThread = new Thread(new ThreadStart(() => GenerateLine(blockStorage, x)));
                    childThread.Start();
                    childThread.Join();
                }
            }));
            worldGen.Start();

            progress = "";
            Thread.Sleep(1);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            Task.Run(async () =>
            {
                cyv -= 0.005;
                if (cyv < -0.5) cyv = -0.45;
                var pos = new Tuple<double, double>((int)cx, (int)cz);
                if (blockStorage.ContainsKey(pos) && blockStorage[pos].Y > (cy - 2.1) && cyv < 0) cyv = 0;
                cy += cyv;
                HandleKeyboard();

            });
            GL.ClearColor(0.1f * (float)brightness, 0.5f * (float)brightness, 0.7f * (float)brightness, 0.5f);

        }
        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
            if (!keyState.IsKeyDown(Key.LControl))
            {
                if (keyState.IsKeyDown(Key.W)) cx += 0.1;
                if (keyState.IsKeyDown(Key.A)) cz -= 0.1;
                if (keyState.IsKeyDown(Key.S)) cx -= 0.1;
                if (keyState.IsKeyDown(Key.D)) cz += 0.1;
                /* if (keyState.IsKeyDown(Key.W)) cx += Math.Cos((clx / 360));
                 if (keyState.IsKeyDown(Key.A)) cz -= Math.Sin((clx / 360));
                 if (keyState.IsKeyDown(Key.S)) cx -= Math.Cos((clx / 360));
                 if (keyState.IsKeyDown(Key.D)) cz += Math.Sin((clx / 360));*/
                if (keyState.IsKeyDown(Key.ShiftLeft)) cy -= 0.1;
                if (keyState.IsKeyDown(Key.Space)) cyv = 0.1;
                if (keyState.IsKeyDown(Key.R))
                {
                    cx = 0; cy = 100; cz = 0;
                }
                if (keyState.IsKeyDown(Key.Q)) brightness -= 0.01;
                if (keyState.IsKeyDown(Key.E)) brightness += 0.01;
            }
            else
            {
                if (keyState.IsKeyDown(Key.W)) clx += 0.1;
                if (keyState.IsKeyDown(Key.S)) clx -= 0.1;
                if (keyState.IsKeyDown(Key.ShiftLeft)) cly -= 0.1;
                if (keyState.IsKeyDown(Key.Space)) cly += 0.1;
            }
        }
        private void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (focussed && !(e.X == centerX && e.Y == centerY))
            {
                int x = e.XDelta;
                int y = -(e.YDelta);
                Point center = new Point(centerX, centerY);
                Point mousePos = PointToScreen(center);
                OpenTK.Input.Mouse.SetPosition(mousePos.X, mousePos.Y);
                clx += x;
                cly += y;
                if (cly > 512) cly = 512;
                if (cly < -512) cly = -512;
                //clx %= 360;
            }
        }
        double cx = 0, cy = 100, cz = 0, clx = 0, cly = 0;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0} | {cx}/{cy}/{cz} | {clx}/{cly} {progress}";
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(new Vector3((float)cx, (float)cy + 2, (float)cz), new Vector3((float)(cx + Math.Cos((clx / 360)) * 360), (float)(cy + 2 + Math.Sin((cly / 360)) * 480) * 2, (float)(cz + Math.Sin((clx / 360)) * 360)), Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            Thread blockRender = new Thread(new ThreadStart(() =>
            {
                for (int x = (int)cx - renderDistance; x < (int)cx + renderDistance; x++)
                {
                    for (int z = (int)cz - renderDistance; z < (int)cz + renderDistance; z++)
                    {
                        var blockPos = new Tuple<double, double>((int)x, (int)z);
                        if (blockStorage.ContainsKey(blockPos))
                            renderCube(x, blockStorage[blockPos].Y, z);
                    }
                }
            }));
            blockRender.Start();
            

            GL.Begin(BeginMode.Points);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3((float)(cx + Math.Cos((clx / 360)) * 360), (float)(cy + 2 + Math.Sin((cly / 360)) * 480) * 2, (float)(cz + Math.Sin((clx / 360)) * 360));
            GL.End();
            SwapBuffers();
        }
        void renderCube(double x, double y, double z)
        {
            GL.Begin(BeginMode.Quads);
            //top
            GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
            GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
            //bottom
            GL.Color3(1.0f * brightness, 1.0f * brightness, 1.0f * brightness);
            GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);
            //left
            GL.Color3(1.0f * brightness, 0.0f * brightness, 0.0f * brightness);
            GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
            //right
            GL.Color3(1.0f * brightness, 0.5f * brightness, 0.0f * brightness);
            GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);
            //front
            GL.Color3(0.0f * brightness, 1.0f * brightness, 1.0f * brightness);
            GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
            GL.End();

        }
    }
    public class Block
    {
        public double Y { get; set; }
    }
}
