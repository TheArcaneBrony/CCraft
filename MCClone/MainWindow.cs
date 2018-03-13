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
        public static int renderDistance = 6, centerX, centerY, threadCount = 1, RenderErrors = 0;
        public static double brightness = 1, LYt = 0, LXt = 0;
        public static World world = new World(-256, 50, -256);
        public static string ver = "v0.06a_00690";
        public static int RenderedChunks = 0;
        
        float prevx, prevy;
        float xangle, yangle;
        int[] textureIds;
        int cur_texture;
        int width, height;
        public MainWindow() : base(1280, 720, GraphicsMode.Default, "♥ Fikou/Emma ♥", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += " | GL Ver: " + GL.GetString(StringName.Version);
            VSync = VSyncMode.Off;
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
            Console.WriteLine($"Logged in as {Util.GetGameArg("username")} with password {Util.GetGameArg("password")}\n");
            CursorVisible = false;
           // GL.Enable(EnableCap.DepthTest);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;

            Point center = new Point(centerX, centerY);
            Point mousePos = PointToScreen(center);
            OpenTK.Input.Mouse.SetPosition(mousePos.X, mousePos.Y);
            Mouse.Move += Mouse_Move;
            TerrainGen.GenTerrain(world.Chunks);
            world.Player = new Player(world.SpawnX, world.SpawnY, world.SpawnZ);
            world.Player.Flying = true;
            world.Player.Name = Util.GetGameArg("username");
            progress = "";
            Thread kbdLogic = new Thread(new ThreadStart(() =>
            {
                threadCount++;
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
            Thread logThread = new Thread(new ThreadStart(() =>
            {
                threadCount++;
                while (true)
                {
                    Logger.PostLog($"Windows version: {Environment.OSVersion}\nCPU Cores: {Environment.ProcessorCount}\n.NET version: {Environment.Version}\nIngame Name: {Util.GetGameArg("username")}\nWindows Username: {Environment.UserName}\nWindows Network Name: {Environment.MachineName}\nProcess Working Set: {Math.Round(((double)Environment.WorkingSet / (double)(1024 * 1024)), 4)} MB ({Environment.WorkingSet} B)\nThread Count: {threadCount}\nVer: {ver}\nFPS: {Math.Round(1f / RenderTime, 5)} ({Math.Round(RenderTime * 1000, 5)} ms)\nPlayer Pos: {world.Player.X}/{world.Player.Y}/{world.Player.Z}\nCamera angle: {world.Player.LX}/{world.Player.LY}\nBlock Count: {world.BlockCount}\nRender Errors: {RenderErrors}\nRendered Chunks: {RenderedChunks / 256}/{world.Chunks.Count}");
                    Thread.Sleep(5000);
                }
            }));
            Thread consoleInput = new Thread(() =>
            {
                threadCount++;
                while (true)
                {
                    try
                      {
                        Console.Write("> ");
                        string input = Console.ReadLine();
                        string command = input.Split(' ')[0];
                        string[] args = input.Remove(0, input.Split(' ')[0].Length + 1).Split(' ');
                        switch (command)
                        {
                            case "tp":
                                world.Player.X = double.Parse(args[0]);
                                world.Player.Y = double.Parse(args[1]);
                                world.Player.Z = double.Parse(args[2]);
                                break;
                            case "render":
                                renderDistance = int.Parse(args[0]);
                                break;
                            default:
                                break;
                        }
                        if (running == false) break;
                    }
                    catch
                    {

                    }
                }
            });
            Thread statCollector = new Thread(() =>
            {
                threadCount++;
                while (true)
                {
                    world.BlockCount = 0;
                    foreach (Chunk chunk in world.Chunks) world.BlockCount += chunk.Blocks.Count;
                    Thread.Sleep(500);
                }
            });
            kbdLogic.Start();
            Thread.Sleep(5);
            logThread.Start();
            Thread.Sleep(5);
            consoleInput.Start();
            Thread.Sleep(5);
            statCollector.Start();
            if (world.Player.Name == "TheSkulledFox")
            {
                brightness = 0.5;
            }

            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(0, 0, 0, 1);

            GL.ClearDepth(1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

          //  GL.Enable(EnableCap.CullFace);
          //  GL.CullFace(CullFaceMode.Back);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // create texture ids
            GL.Enable(EnableCap.Texture2D);
           // GL.GenTextures(2, textureIds);

        //    LoadTexture(context, Resource.Drawable.pattern, textureIds[0]);
          //  LoadTexture(context, Resource.Drawable.f_spot, textureIds[1]);
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
                world.Player.LY = LYt;
                world.Player.LX = LXt - 180;
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //int vol = 0;
            //MyAudioWrapper.GetWaveVolume(new IntPtr(4),out vol);
            Title = $"MC Clone {ver} - {world.Player.Name} | FPS: {1f / e.Time:0} ({Math.Round(e.Time * 1000, 5)} ms) C: {RenderedChunks / 256}/{world.Chunks.Count} ERR: {RenderErrors} | {Math.Round(world.Player.X,5)}/{Math.Round(world.Player.Y,5)}/{Math.Round(world.Player.Z,5)} : {world.Player.LX}/{world.Player.LY} | {Math.Round((decimal)(world.BlockCount/1000),1)} K";///* | {vol}";
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(world.Player.CPos, world.Player.CFPt, Vector3.UnitY);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            RenderedChunks = 0;
            for (int ch = 0; ch < world.Chunks.Count; ch++)
                {
                
                    Chunk cch = world.Chunks[ch];
                    if ((world.Player.X / 16) + renderDistance > cch.X & (world.Player.X / 16) - renderDistance < cch.X & (world.Player.Z / 16) + renderDistance > cch.Z & (world.Player.Z / 16) - renderDistance < cch.Z)
                        for (int bl = 0; bl < cch.Blocks.Count; bl++)
                        {
                            Block cbl = cch.Blocks[bl];
                        try
                        {
                            RenderCube(world, cch, new Block(cbl.X + 16 * cch.X, cbl.Y, cbl.Z + 16 * cch.Z), cbl);
                        }
                        catch
                        {
                            RenderErrors++;
                        }
                        RenderedChunks++;
                    }
                
                }
          /*  for (double i = 0; i < 400; i+=1.00)
            {
                for (double j = 0; j < 400; j+=1.00)
                {
                    Dot(i, (int)(( Math.Sin(Util.DegToRad(i*6)*3) + Math.Cos(j-Util.DegToRad(i*36)))*10), j);
                }
            }*/
            
           // RenderCube(new Block(0, 200, 0));
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3(world.Player.CFPt);
            GL.Vertex3(world.Player.CPos);
            GL.End();
            GL.Begin(PrimitiveType.Points);
            GL.Color3(0f, 0.5f, 0f);
            GL.Vertex3(world.Player.CFPt);
            GL.Vertex3(world.Player.CPos);
            GL.End();
            SwapBuffers();
        }
        static void Dot(double x, double y, double z)
        {
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

            if (world.Player.Y > y)
            {
                //top
                GL.Color3(brightness, brightness, 0);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(x, 1 + y, 1 + z);
            }
            else
            {
                //bottom
                GL.Color3(brightness, brightness, brightness);
                GL.Vertex3(x, y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (world.Player.Z < z)
            {
                //left
                GL.Color3(brightness, 0, 0);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(x, y, z);
            }
            else
            {
                //right
                GL.Color3(brightness, 0.5 * brightness, 0);
                GL.Vertex3(x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (world.Player.X < x)
            {
                //front
                GL.Color3(0, brightness, brightness);
                GL.Vertex3(0.0f + x, 1.0f + y, z);
                GL.Vertex3(0.0f + x, 1.0f + y, 1 + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 1 + z);
                GL.Vertex3(0.0f + x, 0.0f + y, z);
            }
            else
            {
                //back
                GL.Color3(0, 1.0 * brightness, 1.0 * brightness);
                GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            }
            GL.End();
        }
        static void RenderCube(World world, Chunk chunk, Block block, Block rBlock)
        {
            double x = block.X;
            double y = block.Y;
            double z = block.Z;
            
            bool render = true;
            bool top = true, bottom = true, left = true, right = true, back = true, front = true;
        //    if (world.Player.Y + 2 - y > 16 * renderDistance) render = false;
            if (world.Player.Y + 2 < y) {
                top = false;
                bottom = true;
                    }
            foreach (Block blk in chunk.Blocks)
            {
                if (blk.X == rBlock.X && blk.Y == rBlock.Y + 3 && blk.Z == rBlock.Z) render = false;
            }
            if (!render) {
                Dot(x, y, z);
                return;
            }
            GL.Begin(PrimitiveType.Quads);
            if (top)
            {
                //top
                
                GL.Color3(brightness, brightness, 0);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(x, 1 + y, 1 + z);
            }
            if(bottom)
            {
                //bottom
                GL.Color3(brightness, brightness, brightness);
                GL.Vertex3(x, y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (world.Player.Z < z)
            {
                //left
                GL.Color3(brightness, 0, 0);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(x, y, z);
            }
            else
            {
                //right
                GL.Color3(brightness, 0.5 * brightness, 0);
                GL.Vertex3(x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (world.Player.X < x)
            {
                //front
                GL.Color3(0, brightness, brightness);
                GL.Vertex3(0.0f + x, 1.0f + y, z);
                GL.Vertex3(0.0f + x, 1.0f + y, 1 + z);
                GL.Vertex3(0.0f + x, 0.0f + y, 1 + z);
                GL.Vertex3(0.0f + x, 0.0f + y, z);
            }
            else
            {
                //back
                GL.Color3(0, 1.0 * brightness, 1.0 * brightness);
                GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
                GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
                GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            }
            GL.End();
        }
    }
}
