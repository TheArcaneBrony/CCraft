using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace MCClone
{
    class MainWindow : GameWindow
    {
        static bool running = true;
        static int threadCount = 1;
        string progress = "";
        int renderDistance = 128 * 2;
        static double brightness = 1 / 1;
        static Dictionary<Tuple<double, double>, Block> blockStorage = new Dictionary<Tuple<double, double>, Block>();
        bool focussed = true;
        int centerX, centerY;
        double cxv = 0, cyv = 0, czv = 0;
        static bool flying = false;
        static List<Block> bsarr = new List<Block>();
        static Player player;

        public static void GenerateLine(Dictionary<Tuple<double, double>, Block> blockStorage, double x, double z)
        {
            Console.WriteLine("S: " + Thread.CurrentThread.Name);
            for (double xt = 0; xt < 16; xt++)
            {
                for (double zt = 0; zt < 16; zt += 1)
                {
                    if (running)
                    {
                        double bx = (16 * x) + xt, by = (int)(Math.Sin((16 * x + xt) / 20) * 10 + Math.Sin((16 * z + zt) / 30) * 10), bz = (16 * z) + zt;
                        lock (bsarr)
                        {
                            bsarr.Add(new Block(bx, by, bz));
                        }
                        //    Thread.Sleep(2);
                        //  var blockPos = new Tuple<double, double>((16 * x) + xt, (16 * z) + zt);
                        /* while (!blockStorage.ContainsKey(blockPos))
                         {
                             try
                             {
                                 // Console.Write("\nR: " + (16 * x) + xt + "/" + (16 * z) + zt);
                           //      blockPos = new Tuple<double, double>((16 * x) + xt, (16 * z) + zt);
                             //    if (!blockStorage.ContainsKey(blockPos)) blockStorage.Remove(blockPos);
                              //   blockStorage.Add(blockPos, new Block((16 * x) + xt, (int)(Math.Sin((16 * x + xt) / 20) * 10 + Math.Sin((16 * z + zt) / 30) * 10), (16 * z) + zt));
                                 //blockStorage[blockPos].Y = (int)(Math.Sin((16 * x + xt) / 20) * 2 + Math.Sin((16 * z + zt) / 30) * 5);
                                 //blockStorage[blockPos].Y = 2;
                             }
                             catch (Exception)
                             {
                                 Console.WriteLine("E: " + (16 * x) + xt + "/" + (16 * z) + zt + " " + blockStorage.ContainsKey(blockPos));
                                 //throw;
 //Thread.Sleep(100);
                             }

                             Thread.Sleep(0);
                         }*/
                        //Console.WriteLine((xt + "").PadLeft(6) + " | " + (z + "").PadLeft(6));
                        //Thread.Sleep(1);
                        // Thread.Sleep(new TimeSpan(15000));
                        //   progress = " | " + ((xt + "").PadLeft(6) + " | " + (z + "").PadLeft(6));
                        // Console.Title = blockStorage.Count + progress;
                    }


                }
                //Thread.Sleep(15);
            }
            Console.WriteLine("X: " + Thread.CurrentThread.Name);
            threadCount--;
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
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4/* 0.9f*/, Width / (float)Height, 1.0f, 6400f);
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
                for (double x = -8; x < 8; x += 1)
                {
                    for (double z = -8; z < 8; z++)
                    {
                        if (running)
                        {
                            Console.Title = x + "";
                            Thread childThread = new Thread(new ThreadStart(() => GenerateLine(blockStorage, x, z)));
                            childThread.Name = x + " - " + z;
                            while (threadCount > 6) Thread.Sleep(100);
                            threadCount++;
                            childThread.Start();
                            Thread.Sleep(10);
                        }
                    }
                    while (threadCount > 16) Thread.Sleep(1000);
                }
            }));
            worldGen.Start();
            player = new Player(cx, cy, cz);
            progress = "";
            Thread.Sleep(1);
            Thread kbdLogic = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (!flying) cyv -= 0.005;
                    if (cyv < -0.45) cyv = -0.45;
                    var pos = new Tuple<double, double>((int)cx, (int)cz);
                    if (blockStorage.ContainsKey(pos) && blockStorage[pos].Y > (cy - 2.1) && cyv < 0) cyv = 0;
                    if (!flying) cy += cyv;
                    HandleKeyboard();
                    player = new Player(cx, cy, cz);
                    player.CPos = new Vector3((float)cx, (float)cy + 2, (float)cz);
                    player.CFPt = new Vector3((float)(cx + Math.Cos(RadToDeg(clx)) * 360), (float)(cy + 2 + Math.Sin(RadToDeg(cly)) * 360) * 2, (float)(cz + Math.Sin(RadToDeg(clx)) * 360));
                    Thread.Sleep(1000 / 120);
                    if (running == false) break;
                }
                Exit();
            }));
            kbdLogic.Start();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.1f * (float)brightness * (float)e.Time, 0.5f * (float)brightness, 0.7f * (float)brightness, 0.0f);

        }
        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
            {
                running = false;
            }
            if (!keyState.IsKeyDown(Key.LControl))
            {
                if (keyState.IsKeyDown(Key.W))
                {
                    cx += Math.Cos(clxt);
                    cz += Math.Sin(clxt);
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    cx -= Math.Cos(clxt + 90);
                    cz -= Math.Sin(clxt + 90);
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    cx -= Math.Cos(clxt);
                    cz -= Math.Sin(clxt);
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    cx += Math.Cos(clxt + 90);
                    cz += Math.Sin(clxt + 90);
                }
                if (keyState.IsKeyDown(Key.ShiftLeft)) cy -= 0.1;
                if (keyState.IsKeyDown(Key.Space)) if (flying) cy += 0.1; else cyv = 0.1;
                if (keyState.IsKeyDown(Key.R))
                {
                    cx = 0; cy = 100 * 2; cz = 0;
                }
                if (keyState.IsKeyDown(Key.Q)) brightness -= 0.01;
                if (keyState.IsKeyDown(Key.E)) brightness += 0.01;
                if (keyState.IsKeyDown(Key.F)) flying = true;
            }
            else
            {
                var clxt = RadToDeg(clx);
                if (keyState.IsKeyDown(Key.F)) flying = false;
                if (keyState.IsKeyDown(Key.W)) cx += 0.1;
                if (keyState.IsKeyDown(Key.A)) cz -= 0.1;
                if (keyState.IsKeyDown(Key.S)) cx -= 0.1;
                if (keyState.IsKeyDown(Key.D)) cz += 0.1;
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
                clxt += x;
                clyt += y;
                if (clyt > 90) clyt = 90;
                if (clyt < -90) clyt = -90;
                cly = clyt;
              //  clxt %= 360;
                clx = clxt; // -180
            }
        }
        double cx = 0, cy = 100, cz = 0, clx = 0, cly = 0, clxt = 0, clyt = 0;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0} ({(e.Time * 1000 + "00000000000").Substring(0, 5)} ms)| {cx}/{cy}/{cz} | {clx}/{cly} | {threadCount} {progress}";
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(player.CPos, player.CFPt, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            
            /*for (int x = (int)cx - renderDistance; x < (int)cx + renderDistance; x++)
            {
                for (int z = (int)cz - renderDistance; z < (int)cz + renderDistance; z++)
                {
                    var blockPos = new Tuple<double, double>((int)x, (int)z);
                    if (blockStorage.ContainsKey(blockPos))
                    {
                        RenderCube(x, blockStorage[blockPos].Y, z);
                    }

                }
            }*/
            for (int i = 0; i < bsarr.Count; i++)
            {
                lock (bsarr)
                {
                    Block block = bsarr[i];
                    if (cx - renderDistance <= block.X && block.X <= cx + renderDistance && cz - renderDistance <= block.Z && block.Z <= cz + renderDistance)
                        RenderCube(block);
                }
            }
            RenderCube(new Block(0, 200, 0));

            GL.Begin(PrimitiveType.Points);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3((float)(cx + Math.Cos((clx / 360)) * 360), (float)(cy + 2 + Math.Sin((cly / 360)) * 480) * 2, (float)(cz + Math.Sin((clx / 360)) * 360));
            GL.End();
            SwapBuffers();
        }
        static void dot(double x, double z)
        {
            double y = 10;
            GL.Begin(PrimitiveType.Points);
            GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
            GL.Vertex3(0.5f + x, 1.0f + y, 0.5f + z);
            GL.End();
            GL.Begin(PrimitiveType.Quads);
        }
        static void RenderCube(Block block)
        {
            double x = block.X;
            double y = block.Y;
            double z = block.Z;
            GL.Begin(PrimitiveType.Quads);

            if (player.Y > y)
            {
                //top
                GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
                GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
            }
            else
            {
                //bottom
                GL.Color3(1.0f * brightness, 1.0f * brightness, 1.0f * brightness);
                GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);

            }

            if (player.Z < z)
            {
                //left
                GL.Color3(1.0f * brightness, 0.0f * brightness, 0.0f * brightness);
                GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);

            }
            else
            {
                //right
                GL.Color3(1.0f * brightness, 0.5f * brightness, 0.0f * brightness);
                GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);
            }
            if(player.X < x)
            {
                //front
                GL.Color3(0.0f * brightness, 1.0f * brightness, 1.0f * brightness);
                GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
            } else
            {
                //back
                GL.Color3(0.0f * brightness, 1.0f * brightness, 1.0f * brightness);
                GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            }
            GL.End();
        }
        public static double RadToDeg(double rad)
        {
            return Math.PI * rad / 180;
        }
    }
    public class Block
    {
        public Block() { }
        public Block(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
    public class Player
    {
        public Player() { }
        public Player(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Vector3 CPos { get; set; }
        public Vector3 CFPt { get; set; }
        
    }
    
}
