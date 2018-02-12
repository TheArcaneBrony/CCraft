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
        public static int renderDistance = 128 * 2, centerX, centerY, threadCount = 1;
        public static double brightness = 1;
        public static List<Chunk> chunkList = new List<Chunk>();
        public static List<int> dispLists = new List<int>();
        public static Player player;
        
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
            TerrainGen.GenTerrain(chunkList);
            player = new Player(0, 100, 0);
            progress = "";
            Thread.Sleep(1);
            Thread kbdLogic = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    log = "";
                    if (!player.Flying) player.YV -= 0.005;
                    if (player.YV < -0.45) player.YV = -0.45;
                  //  if (bsarr.Contains(new Block((int)cx, (int)cy, (int)cz)) && cyv < 0) cyv = 0;
                    if (!player.Flying) player.Y += player.YV;
                    Input.HandleKeyboard();
                    
                    player.CPos = new Vector3((float)player.X, (float)player.Y + 2, (float)player.Z);
                    player.CFPt = new Vector3((float)(player.X + Math.Cos(Util.DegToRad(player.LX)) * 360), (float)(player.Y + 2 + Math.Sin(Util.DegToRad(player.LY)) * 360) * 2, (float)(player.Z + Math.Sin(Util.DegToRad(player.LX)) * 360));
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
                            player.X = double.Parse(args[0]);
                            player.Y = double.Parse(args[1]);
                            player.Z = double.Parse(args[2]);
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
                player.LXt += x;
                player.LYt += y;
                if (player.LYt > 90) player.LYt = 90;
                if (player.LYt < -90) player.LYt = -90;
                player.LY = player.LYt;
                player.LX = player.LXt - 180;
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"FPS: {1f / e.Time:0} ({(e.Time * 1000 + "00000000000").Substring(0, 5)} ms) ({threadCount} th) | {player.X}/{player.Y}/{player.Z} | {player.LX}/{player.LY} | {chunkList.Count} | {dispLists.Count}";
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

            for (int i = 0; i < chunkList.Count; i++)
            {
                Chunk CurrChunk = chunkList[i];
             //   if (cx - renderDistance <= CurrChunk.X && CurrChunk.X <= cx + renderDistance && cz - renderDistance <= CurrChunk.Z && CurrChunk.Z <= cz + renderDistance)
                    for (int j = 0; i < CurrChunk.Blocks.Count; i++)
                {
                  /*  lock (chunkList)
                    {*/
                        Block block = CurrChunk.Blocks[j];
                      //  if (cx - renderDistance <= block.X && block.X <= cx + renderDistance && cz - renderDistance <= block.Z && block.Z <= cz + renderDistance)
                            RenderCube(new Block(CurrChunk.X * 16 + block.X, block.Y + CurrChunk.X, CurrChunk.Z * 16 + block.Z));
                    //Console.Write($"{CurrChunk.X} ");
                   // }
                    Thread.Sleep(0);
                }
            }
            
            /*lock (dispLists)
            {
                foreach (var list in dispLists)
                {

                    GL.CallList(list);
                }
            }*/

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
