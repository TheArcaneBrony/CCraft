using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
        static bool running = true, flying = false, focussed = true;
        static string progress = "", log = "";
        static int renderDistance = 128 * 2, centerX, centerY, threadCount = 1;
        static double brightness = 1 / 1, cxv = 0, cyv = 0, czv = 0, cx = 0, cy = 100, cz = 0, clx = 0, cly = 0, clxt = 0, clyt = 0;
        static List<Block> bsarr = new List<Block>();
        static Player player;
        
        public static void GenerateChunk(double x, double z)
        {

            /*Console.SetCursorPosition((int)x+16, (int)z + 16);
            Console.Write("█");
            log += ("S: " + Thread.CurrentThread.Name + "\n");*/
            threadCount++;
            try
            {
                for (double xt = 0; xt < 16; xt++)
                {
                    double bx = 16 * x + xt;
                    for (double zt = 0; zt < 16; zt += 1)
                    {
                        double bz = 16 * z + zt, by = (int)(Math.Sin(DegToRad(bx)) * 10 + Math.Sin(DegToRad(bz)) * 10);
                        Block blk = new Block(bx, by, bz);
                        if (!bsarr.Contains(blk))
                        {
                            lock (bsarr)
                            {
                                bsarr.Add(blk);
                            }
                        }
                        
                    }
                    
                }
            }
            catch (Exception)
            {

                throw;
            }
           
            log += ("X: " + Thread.CurrentThread.Name + "\n");
            threadCount--;
        }

        public MainWindow() : base(1280, 720, GraphicsMode.Default, "♥ Fikou/Emma ♥", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
            
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4/* 0.9f*/, Width / (float)Height, 1.0f, 64000f);
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
            Thread worldGen = new Thread(() =>
            {
                for (double x = -16; x < 16; x += 1)
                {
                    Thread worldGen2 = new Thread(() =>
                    {
                        double xt = x;
                        for (double z = -16; z < 16; z++)
                        {
                            
                            //Console.Title = x + "";

                           // while (threadCount > 1) Thread.Sleep(100);
                            Thread.Sleep(13);
                            var success = false;
                            while (!success)
                            {
                                Thread childThread = new Thread(() => GenerateChunk(xt, z));
                                childThread.Name = x + " - " + z; childThread.Start();
                                success = childThread.Join(10000);

                            }
                        }
                    });
                    while (threadCount > 9) Thread.Sleep(100);

                    worldGen2.Start();
                    worldGen2.Join(130);
                }
                VSync = VSyncMode.Off;
            });
            worldGen.Start();
            player = new Player(cx, cy, cz);
            progress = "";
            Thread.Sleep(1);
            Thread kbdLogic = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                //    Console.Write(log);
                    log = "";
                    if (!flying) cyv -= 0.005;
                    if (cyv < -0.45) cyv = -0.45;
                    if (bsarr.Contains(new Block((int)cx, (int)cy, (int)cz)) && cyv < 0) cyv = 0;
                    if (!flying) cy += cyv;
                    HandleKeyboard();
                    player = new Player(cx, cy, cz);
                    player.CPos = new Vector3((float)cx, (float)cy + 2, (float)cz);
                    player.CFPt = new Vector3((float)(cx + Math.Cos(DegToRad(clx)) * 360), (float)(cy + 2 + Math.Sin(DegToRad(cly)) * 360) * 2, (float)(cz + Math.Sin(DegToRad(clx)) * 360));
                    Thread.Sleep(1000 / 120);
                    if (running == false) break;
                }
                Exit();
            }));
            kbdLogic.Start();
            Thread.Sleep(10);
            Thread consoleInput = new Thread(() =>
            {
                while (true)
                {
                    string input = Console.ReadLine();
                    string command = input.Split(' ')[0];
                    string[] args = input.Remove(0, input.Split(' ')[0].Length+1).Split(' ');
                    Console.WriteLine("\"" + command + "\" " + "\"" + args[0] + "\" " + "\"" + args[1] + "\" " + "\"" + args[2] + "\" ");
                    switch(command)
                    {
                        case "tp":
                            cx = double.Parse(args[0]);
                            cy = double.Parse(args[1]);
                            cz = double.Parse(args[2]);
                            break;
                        default:
                            break;
                    }
                    if (running == false) break;
                }
            });
            consoleInput.Start();
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
                Thread.Sleep(100);
                Exit();
                Environment.Exit(0);
            }
            if (!keyState.IsKeyDown(Key.LControl))
            {
                if (keyState.IsKeyDown(Key.W))
                {
                    cx += Math.Cos(DegToRad(clx));
                    cz += Math.Sin(DegToRad(clx));
                }
                if (keyState.IsKeyDown(Key.A))
                {
                    cx -= Math.Cos(DegToRad(clx + 90));
                    cz -= Math.Sin(DegToRad(clx + 90));
                }
                if (keyState.IsKeyDown(Key.S))
                {
                    cx -= Math.Cos(DegToRad(clx));
                    cz -= Math.Sin(DegToRad(clx));
                }
                if (keyState.IsKeyDown(Key.D))
                {
                    cx += Math.Cos(DegToRad(clx + 90));
                    cz += Math.Sin(DegToRad(clx + 90));
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
                clx = clxt - 180;
              //  Console.Title = $"RX: {DegToRad(clx)} | RY: {DegToRad(cly)}";
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"FPS: {1f / e.Time:0} ({(e.Time * 1000 + "00000000000").Substring(0, 5)} ms) ({threadCount} thr) | {cx}/{cy}/{cz} | {clx}/{cly} | {bsarr.Count}";
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
            //GL.Color3();
           /* for (double x = 0; x < 100; x += 0.001) {
                var y = Math.Sinh(x);
                GL.Color3(x / 100, Math.Sin(x), Math.Cos(x));
              //  GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
                GL.Vertex3(x, y, 0);
                
            }*/
            GL.End();
            GL.Begin(PrimitiveType.Points);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3(player.CFPt);
            GL.End();
            SwapBuffers();
        }
        static void dot(double x, double y)
        {
            double z = 100;
            GL.Begin(PrimitiveType.Points);
            GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
            GL.Vertex3(0.5f + x, 1.0f + y, 0.5f + z);
            GL.End();
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
        public static double DegToRad(double rad)
        {
            return (Math.PI/180)*rad;
            
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
