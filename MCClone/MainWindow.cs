using System;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using CSCore.CoreAudioAPI;
using System.Collections.Generic;
using System.IO;

namespace MCClone
{
    class MainWindow : GameWindow
    {
        public static bool running = true, focussed = true, logger = true;
        public static string ver = "0.06a_01068";
        public static int renderDistance = 10, centerX, centerY, RenderErrors = 0, RenderedChunks = 0;
        public static double brightness = 1, rt = 0;
        public static World world = new World(0, 100, 0);
        public static float vol,sensitivity=.1f;
        public static List<Chunk> crq = new List<Chunk>();
        private int _program;

        private int _vertexArray;

        public MainWindow() : base(1280, 720, GraphicsMode.Default, "The Arcane Brony#9669's Minecraft Clone", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += $" | GL Ver: {GL.GetString(StringName.Version)} | Version: {ver}";
            VSync = VSyncMode.Off;
            Console.WriteLine(Title);
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
        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine($"Logged in as {Util.GetGameArg("username")} with password {Util.GetGameArg("password")}\n");
            CursorVisible = false;
            _program = CompileShaders();

            GL.GenVertexArrays(1, out _vertexArray);
            GL.BindVertexArray(_vertexArray);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;

            Point center = new Point(centerX, centerY);
            Point mousePos = PointToScreen(center);
            OpenTK.Input.Mouse.SetPosition(mousePos.X, mousePos.Y);

            world.Player = new Player(world.SpawnX, world.SpawnY, world.SpawnZ)
            {
                Flying = true,
                Name = Util.GetGameArg("username")
            };
            Directory.CreateDirectory($"Worlds/{world.Name}/ChunkData/");
            Thread gameInit = new Thread(() =>
            {
                TerrainGen.GenTerrain(world.Chunks);
                while (true){
                    Thread.Sleep(100);
                    if (RenderedChunks < renderDistance*renderDistance)
                        for (int x = world.Player.Xc - renderDistance; x < world.Player.Xc + renderDistance; x++) for (int z = world.Player.Zc - renderDistance; z < world.Player.Zc + renderDistance; z++)
                            {
                                if (world.Chunks.Find(chunk =>
                                {

                                    if (chunk.X == x && chunk.Z == z)
                                    {
                                        //Logger.LogQueue.Add($"CHUNK EXISTS @ {x}/{z} AND HAS {chunk.Blocks.Count} BLOCKS");
                                        return true;
                                    }

                                    return false;
                                }) == null) {
                                    // new Thread(() =>
                                    //  {

                                    // Logger.LogQueue.Add($"CHUNK GENERATING @ {x}/{z}");
                                    TerrainGen.GenChunk(world.Chunks, x, z);
                                    //}).Start();
                                }
                            }
            }
            });
            Thread kbdLogic = new Thread(() =>
            {
                while (true)
                {
                    Input.HandleInput();
                    Input.Tick();
                    Thread.Sleep(1000 / 120);
                    if (running == false) break;
                }
                Exit();
            });
            Thread logThread = new Thread(() =>
            {
                while (true) {
                    while (logger)
                    {
                        Logger.PostLog($"Windows version: {Environment.OSVersion}\nCPU Cores: {Environment.ProcessorCount}\n.NET version: {Environment.Version}\nIngame Name: {Util.GetGameArg("username")}\nWindows Username: {Environment.UserName}\nWindows Network Name: {Environment.MachineName}\nProcess Working Set: {Math.Round(((double)Environment.WorkingSet / (double)(1024 * 1024)), 4)} MB ({Environment.WorkingSet} B)\nVer: {ver}\nFPS: {Math.Round(1f / RenderTime, 5)} ({Math.Round(RenderTime * 1000, 5)} ms)\nPlayer Pos: {world.Player.X}/{world.Player.Y}/{world.Player.Z}\nCamera angle: {world.Player.LX}/{world.Player.LY}\nBlock Count: {world.BlockCount}\nRender Errors: {RenderErrors}\nRendered Chunks: {RenderedChunks / 256}/{world.Chunks.Count}");
                        Thread.Sleep(5000);
                    }
                    Thread.Sleep(5000);
                }
            });
            Thread consoleInput = new Thread(() =>
            {
                while (true)
                {
                    string command = "none";
                    string[] args;
                    try
                        {
                        Console.Write("> ");
                        string input = Console.ReadLine();
                        command = input.Split(' ')[0];
                        args = input.Remove(0, input.Split(' ')[0].Length + 1).Split(' ');
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
                                Console.WriteLine($"Invalid command: {command}");
                                break;
                        }
                        if (running == false) break;
                    }
                    catch
                    {
                        Console.WriteLine($"Invalid command or arguments: {command}");
                    }
                    Thread.Sleep(200);
                }
            });
            Thread statCollector = new Thread(() =>
            {
                while (true)
                {
                    world.BlockCount = 0;
                    lock (world.Chunks)
                    {
                        try
                        {
                            foreach (var chunk in world.Chunks) world.BlockCount += chunk.Blocks.Count;
                        }
                        catch (Exception)
                        {
                            RenderErrors++;
                           //throw;
                        }

                    }

                    crq = world.Chunks.FindAll(chunk =>
                    {
                        if (chunk == null) return false;
                        //int tx = world.Player.Xc;
                        //int tz = world.Player.Zc;
                        //if (tx + renderDistance > chunk.X & tx - renderDistance < chunk.X & tz + renderDistance > chunk.Z & tz - renderDistance < chunk.Z)
                        if (Util.ShouldRenderChunk(chunk)) return true;
                        return false;
                    });
                    while(Logger.LogQueue.Count > 0)
                    {
                        if (Logger.LogQueue.Count > 20) Logger.LogQueue.RemoveRange(20, Logger.LogQueue.Count-20);
                        Logger.PostLog(Logger.LogQueue[0] + $",LOG_REM={Logger.LogQueue.Count}"); Logger.LogQueue.RemoveAt(0);
                    }
                    Thread.Sleep(100);
                }
            });
            kbdLogic.Start();
            Thread.Sleep(5);
            logThread.Start();
            Thread.Sleep(5);
            consoleInput.Start();
            Thread.Sleep(5);
            statCollector.Start();
            Thread.Sleep(5);
            gameInit.Start();

           /* GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(0, 0, 0, 1);*/

            GL.ClearDepth(1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

          //  GL.Enable(EnableCap.CullFace);
          //  GL.CullFace(CullFaceMode.Back);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // create texture ids
            GL.Enable(EnableCap.Texture2D);
           /* GL.GenTextures(2, textureIds);

            LoadTexture(context, Resource.Drawable.pattern, textureIds[0]);
            LoadTexture(context, Resource.Drawable.f_spot, textureIds[1]);*/
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
           // vol = AudioMeterInformation.FromDevice(new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)).PeakValue;
            GL.ClearColor(0.1f * (float)brightness, 0.5f * (float)brightness, 0.7f * (float)brightness, 0.0f);
            Title = $"MC Clone {ver} | FPS: {1f / rt:0} ({Math.Round(rt * 1000, 2)} ms) C: {crq.Count}/{world.Chunks.Count} E: {RenderErrors} | {world.Player.Xa}/{world.Player.Ya}/{world.Player.Za} : {world.Player.LX}/{world.Player.LY} | {Math.Round((double)(world.BlockCount / 1000), 1)} K | {world.Player.Name}@{world.Name}"; //{Math.Round(vol * 100, 0)} |
        }

        public double _time;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rt = e.Time;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(world.Player.CPos, world.Player.CFPt, Vector3.UnitY);

           /* GL.UseProgram(_program);
            _time += e.Time;
            GL.VertexAttrib1(0, _time);
            Vector4 position;
            position.X = (float)Math.Sin(_time) * 0.5f;
            position.Y = (float)Math.Cos(_time) * 0.5f;
            position.Z = 0.0f;
            position.W = 1.0f;
            GL.VertexAttrib4(1, position);
            GL.DrawArrays(PrimitiveType.Points, 0, 1);
            GL.PointSize(10);*/

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);



            RenderedChunks = 0;
            //chunkList[ti].Blocks.FindAll(delegate (Block block) { return true; });

            foreach (Chunk cch in crq)
            {
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

                }
                RenderedChunks++;
            }
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3(world.Player.CFPt);
            GL.Vertex3(world.Player.CPos);
            GL.End();
           /* GL.Begin(PrimitiveType.Points);
            GL.Color3(0f, 0.5f, 0f);
            GL.Vertex3(world.Player.CFPt);
            GL.Vertex3(world.Player.CPos);
            GL.End();*/


            SwapBuffers();
        }
        static void Dot(double x, double y, double z)
        {
            GL.Begin(PrimitiveType.Points);
            GL.Color3(1.0f * brightness, 1.0f * brightness, 0.0f * brightness);
            GL.Vertex3(0.5f + x, 1.0f + y, 0.5f + z);
            GL.End();
        }
        static void RenderCube(World world, Chunk chunk, Block block, Block rBlock)
        {
            int x = block.X;
            int y = block.Y;
            int z = block.Z;

          //  bool render = true;
            bool top = true, left = true, front = true;
            //    if (world.Player.Y + 2 - y > 16 * renderDistance) render = false;

            if (world.Player.Xa > x) front = false;
            if (world.Player.Ya + 2 < y) top = false;
            if (world.Player.Za > z) left = false;

            /*    foreach (Block blk in chunk.Blocks)
                {
                    if (blk.X == rBlock.X && blk.Y == rBlock.Y + 3 && blk.Z == rBlock.Z) render = false;
                }*/
      /*      if(chunk.Blocks.Find(delegate (Block blck)
            {
                if (blck.X == block.X && blck.Z == block.Z && blck.Y > block.Y)
                    return true;
                return false;
            }) != null) render=false;*/
           /* if (!render) {
                Dot(x, y, z);
                return;
            }*/
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
            else
            {
                //bottom
                GL.Color3(brightness, brightness, brightness);
                GL.Vertex3(x, y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (left)
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
            if (front)
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
        private int CompileShaders()
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader,
            File.ReadAllText(@"Components\Shaders\vertexShader.vert"));
            GL.CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader,
            File.ReadAllText(@"Components\Shaders\fragmentShader.frag"));
            GL.CompileShader(fragmentShader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }
    }
}
