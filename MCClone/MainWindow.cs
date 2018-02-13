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
        public static bool running = true, focussed = true;
        public static string progress = "", log = "";
        public static int renderDistance = 5, centerX, centerY, threadCount = 1;
        public static double brightness = 1, LYt = 0, LXt = 0;
        public static List<Chunk> chunkList = new List<Chunk>();
        public static List<int> dispLists = new List<int>();
        public static Player player;
        public MainWindow() : base(1280, 720, GraphicsMode.Default, "♥ Fikou/Emma ♥", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += " | GL Ver: " + GL.GetString(StringName.Version);
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
            TerrainGen.GenTerrain(chunkList);
            player = new Player(0, 200, 0);
            progress = "";
            Thread.Sleep(1);
            Thread kbdLogic = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    log = "";
                    Input.Tick();
                    Input.HandleKeyboard();
                    Thread.Sleep(1000 / 120);
                    if (running == false) break;
                }
                Exit();
            }));
            kbdLogic.Start();
            Thread consoleInput = new Thread(() =>
            {
                while (true)
                {
                    string input = Console.ReadLine();
                    string command = input.Split(' ')[0];
                    string[] args = input.Remove(0, input.Split(' ')[0].Length+1).Split(' ');
                    switch(command)
                    {
                        case "tp":
                            player.X = double.Parse(args[0]);
                            player.Y = double.Parse(args[1]);
                            player.Z = double.Parse(args[2]);
                            break;
                        case "render":
                            renderDistance = int.Parse(args[0]);
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
       
        private void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (focussed && !(e.X == centerX && e.Y == centerY))
            {
                int x = e.XDelta;
                int y = -(e.YDelta);
                Point center = new Point(centerX, centerY);
                Point mousePos = PointToScreen(center);
                OpenTK.Input.Mouse.SetPosition(mousePos.X, mousePos.Y);
                LXt += x;
                LYt += y;
                if (LYt > 90) LYt = 90;
                if (LYt < -90) LYt = -90;
                player.LY = LYt;
                player.LX = LXt - 180;
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"FPS: {1f / e.Time:0} ({(e.Time * 1000 + "00000000000").Substring(0, 5)} ms) ({threadCount} th) | {player.X}/{player.Y}/{player.Z} | {player.LX}/{player.LY} | {chunkList.Count} | {dispLists.Count}";
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(player.CPos, player.CFPt, Vector3.UnitY);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            for (int ch = 0; ch < chunkList.Count; ch++)
            {
                Chunk cch = chunkList[ch];
                if((player.X / 16)+renderDistance > cch.X & (player.X / 16) - renderDistance < cch.X & (player.Z / 16) + renderDistance > cch.Z & (player.Z / 16) - renderDistance < cch.Z)
                for (int bl = 0; bl < cch.Blocks.Count; bl++)
                {
                    Block cbl = cch.Blocks[bl];
                    RenderCube(new Block(cbl.X + 16 * cch.X, cbl.Y, cbl.Z + 16 * cch.Z));
                }
            }

            RenderCube(new Block(0, 200, 0));
            
            
            GL.Begin(PrimitiveType.Lines);
            //GL.Color3();
            /* for (double x = 0; x < 100; x += 0.001) {
                 var y = Math.Sinh(x);
                 GL.Color3(x / 100, Math.Sin(x), Math.Cos(x));
               //  GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
                 GL.Vertex3(x, y, 0);

             }*/
            GL.Vertex3(player.CPos);
            GL.Vertex3(player.CFPt);
            GL.End();
            GL.Begin(PrimitiveType.Points);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3(player.CFPt);
            GL.End();
            SwapBuffers();
        }
        static void Dot(double x, double y)
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
    }
    
    
}
