using MCClone.Client;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MCClone
{
    public class MainWindow : GameWindow
    {
        public static List<ModData> Mods = new List<ModData>();
        public static bool running = true, focussed = true, logger = true;
        public static int renderDistance = 8, centerX, centerY, RenderErrors = 0, RenderedChunks = 0, LoadedMods = 0;
        public static double rt = 0;
        public static World world = new World("Debug")
        {
            // Name = "DebugTestWorld"
        };
        public static float sensitivity = .1f, brightness = 1;

        public string Username = "DebugUser";


        //Modernization - 6/6/2020
        private int _vertexBufferObject;

        private static int _vertexArrayObject;
        private static Shader _shader;

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };


        public MainWindow() : base(640, 480, GraphicsMode.Default, "The Arcane Brony#9669's Minecraft Clone", GameWindowFlags.Default, DisplayDevice.Default, 0, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += $" | GL Ver: {GL.GetString(StringName.Version)} | Version: {DataStore.Ver}";
            VSync = VSyncMode.On;

        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4/* 0.9f*/, Width / (float)Height, 1.0f, 6400f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            foreach (ModData mod in Mods)
            {
                if (mod.OnResize != null)
                {
                    mod.OnResize.Invoke(mod.Instance, new object[] { e });
                }
            }
        }

        private readonly Stopwatch frameTime = new Stopwatch();
        public Stopwatch Time = new Stopwatch();
        [MTAThread]
        protected override void OnLoad(EventArgs e)
        {
            frameTime.Start();
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory + "\\Mods"))
            {
                ModData mod = new ModData();
                try
                {
                    Assembly DLL = Assembly.LoadFile(file);
                    Type theType = DLL.GetType("MCClone.Mod");
                    object c = Activator.CreateInstance(theType);
                    mod.Instance = c;
                    try
                    {
                        mod.OnLoad = theType.GetMethod("OnLoad");
                        mod.OnLoad.Invoke(c, Array.Empty<object>());
                    }
                    catch { }
                    try
                    {
                        mod.OnRenderFrame = theType.GetMethod("OnRenderFrame");
                    }
                    catch { }
                    try
                    {
                        mod.OnUpdateFrame = theType.GetMethod("OnUpdateFrame");
                    }
                    catch { }
                    try
                    {
                        mod.OnResize = theType.GetMethod("OnResize");
                    }
                    catch { }
                    Mods.Add(mod);
                    LoadedMods++;
                }
                catch { }
            }

            if (Util.GetGameArg("world") != "null") { world.Name = Util.GetGameArg("world"); }
            Console.WriteLine($"Logged in as {Util.GetGameArg("username")} with password {Util.GetGameArg("password")}\n");

            /*centerX = (ushort)(ClientRectangle.Width);
            centerY = (ushort)(ClientRectangle.Height);

            Point center = PointToScreen(new Point(centerX, centerY));
            Mouse.SetPosition(center.X, center.Y);*/

            DataStore.Player = new Player(world)
            {
                Flying = true,
                Name = Util.GetGameArg("username")
            };
            Thread gameInit = new Thread(() =>
            {
                TerrainGen.Initialize(world.Dimensions[0]);
                while (true)
                {
                    Thread.Sleep(250);
                    TerrainGen.GenerateAround(DataStore.Player.CurrentDimension, (int)DataStore.Player.X, (int)DataStore.Player.Z, TerrainGen.renderDistance + TerrainGen.genDistance);
                }
                /*while (false)
                {
                    Thread.Sleep(2000);
                    int lctu = (int)(Math.Pow((renderDistance * unloadDistance * 2), 2)),
                    lctg = (int)(Math.Pow((renderDistance * genDistance * 2), 2));
                    // Console.Title = lctu + " " + lctg + " " + brightness;
                    // if (RenderedChunks < renderDistance * renderDistance)
                    if (true || world.Chunks.Count < lctg)
                    {
                        Time.Restart();
                        TerrainGen.GenTerrain();
                        *//*for (int x = (int)(DataStore.Player.X / 16 - renderDistance * genDistance); x < DataStore.Player.X / 16 + renderDistance * genDistance; x++)
                        {
                            for (int z = (int)(DataStore.Player.Z / 16 - renderDistance * genDistance); z < DataStore.Player.Z / 16 + renderDistance * genDistance; z++)
                            {
                                if (!world.Chunks.ContainsKey((x, z)))
                                {
                                    TerrainGen.GetChunk(x, z);
                                }
                            }
                            Thread.Sleep(100);
                        }*//*
                        Logger.LogQueue.Enqueue($"Generating new chunks took {Math.Round(Time.ElapsedTicks / 10000d, 4)} ms");
                    }
                    if (true || world.Chunks.Count > lctu)
                    {
                        Time.Restart();
                        int _ = world.Chunks.Count;
                        (int, int)[] ch = new (int, int)[_];
                        world.Chunks.Keys.CopyTo(ch, 0);
                        foreach ((int X, int Z) in ch)
                        {
                            if (X < (int)(DataStore.Player.X / 16 - renderDistance * unloadDistance)
                            || X > (int)(DataStore.Player.X / 16 + renderDistance * unloadDistance)
                            || Z < (int)(DataStore.Player.Z / 16 - renderDistance * unloadDistance)
                            || Z > (int)(DataStore.Player.Z / 16 + renderDistance * unloadDistance)
                            )
                            {
                                // world.Chunks.Remove((X, Z));
                            }
                        }
                        Logger.LogQueue.Enqueue($"Unloading chunks took {Math.Round(Time.ElapsedTicks / 10000d, 4)} ms {lctu}");
                    }
                }
            */
            });
            Thread logThread = new Thread(() =>
            {
                while (logger)
                {
                    Logger.Log("DIAGNOSTIC",
                        $"Ver: {DataStore.Ver}\n" +
                        $"FPS: {Math.Round(1f / RenderTime, 5)} ({Math.Round(RenderTime * 1000, 5)} ms)\n" +
                        $"Windows version: {Environment.OSVersion}\n" +
                        $".NET version: {Environment.Version}\n" +
                        $"OpenGL version: {GL.GetString(StringName.Version)} ({GL.GetString(StringName.Vendor)}) RENDERER: {GL.GetString(StringName.Renderer)}\n" +
                        DataStore.SystemInfo.CPU +
                        DataStore.SystemInfo.GPU +
                        $"Ingame Name: {Util.GetGameArg("username")}\n" +
                        $"Windows Username: {Environment.UserName}\n" +
                        $"Windows Network Name: {Environment.MachineName}\n" +
                        $"Process Working Set: {Math.Round((Environment.WorkingSet / (double)(1024 * 1024)), 4)} MB ({Environment.WorkingSet} B)\n" +
                        $"Player Pos: {DataStore.Player.X}/{DataStore.Player.Y}/{DataStore.Player.Z}\n" +
                        $"Camera angle: {DataStore.Player.LX}/{DataStore.Player.LY}\n" +
                        $"Render Errors: {RenderErrors}\n" +
                        $"Rendered Chunks: {RenderedChunks}/D:{world.Dimensions[DataStore.Player.DimensionId].Chunks.Count}/W:{world.ChunkCount}"
                        );
                    Thread.Sleep(1000);
                }
            });
            Thread consoleInput = new Thread(() =>
            {
                Logger.Start();
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
                                DataStore.Player.X = double.Parse(args[0]);
                                DataStore.Player.Y = double.Parse(args[1]);
                                DataStore.Player.Z = double.Parse(args[2]);
                                break;
                            case "render":
                                renderDistance = int.Parse(args[0]);
                                break;
                            case "debug":

                                break;
                            case "tpdim":
                                DataStore.Player.DimensionId = int.Parse(args[0]);
                                break;
                            default:
                                Console.WriteLine($"Invalid command: {command}");
                                break;
                        }
                        if (running == false)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Invalid command or arguments: {command}");
                    }
                }
            });
            logThread.Name = "Logging";
            logThread.IsBackground = true;
            logThread.Priority = ThreadPriority.Lowest;
            logThread.Start();
            consoleInput.Name = "Console input";
            consoleInput.IsBackground = true;
            consoleInput.Priority = ThreadPriority.Lowest;
            consoleInput.Start();
            gameInit.Name = "Game init";
            gameInit.IsBackground = true;
            gameInit.Priority = ThreadPriority.AboveNormal;
            gameInit.Start();

            Thread.CurrentThread.Name = "Main thread";
            DataStore.Threads.AddRange(new Thread[] { Thread.CurrentThread, gameInit, consoleInput, logThread, Logger.logQueueThread });

             GL.Enable(EnableCap.DepthTest);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Input.HandleInput();
            //   Input.Tick();
            GL.ClearColor(0.1f * brightness, 0.5f * brightness, 0.7f * brightness, 0.9f);
            Title = $"MC Clone {DataStore.Ver} | FPS: {Math.Round(1000 / rt, 2)} ({Math.Round(rt, 2)} ms) C: {RenderedChunks}/D:{world.Dimensions[DataStore.Player.DimensionId].Chunks.Count}/W:{world.ChunkCount} E: {RenderErrors} | {DataStore.Player.X}/{DataStore.Player.Y}/{DataStore.Player.Z} : {DataStore.Player.LX}/{DataStore.Player.LY} | {Math.Round((double)Process.GetCurrentProcess().PrivateMemorySize64 / 1048576, 2)} MB | Mods: {LoadedMods} | Log#: {Logger.LogQueue.Count}"; //{Math.Round(vol * 100, 0)}
            //Title = $"MCC {Math.Round(1000 / rt, 2)}FPS {Math.Round((double)Process.GetCurrentProcess().PrivateMemorySize64 / 1048576, 2)}MB";

            foreach (ModData mod in Mods)
            {
                if (mod.OnResize != null)
                {
                    mod.OnUpdateFrame.Invoke(mod.Instance, new object[] { e });
                }
            }
        }

        //   List<Chunk> crq2 = new List<Chunk>();
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rt = frameTime.ElapsedTicks / 10000d;
            frameTime.Restart();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(DataStore.Player.CPos, DataStore.Player.CFPt, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            
            RenderWorld();

            if (Mods.Count > 0)
            {
                foreach (ModData mod in Mods)
                {
                    if (mod.OnRenderFrame != null)
                    {
                        mod.OnRenderFrame.Invoke(mod.Instance, new object[] { e });
                    }
                }
            }
            SwapBuffers();
        }
        
        private static void RenderWorld()
        {
            
            RenderedChunks = 0;
           

            GL.Begin(PrimitiveType.Quads);
            try
            {

                //Stack<Chunk> chunks = new Stack<Chunk>(world.Chunks.Values.ToList().FindAll((ch) => true || CliUtil.ShouldRenderChunk(ch)));
                Stack<Chunk> chunks = new Stack<Chunk>(world.Dimensions[DataStore.Player.DimensionId].Chunks.Values);
                foreach (Chunk ce in chunks) { if (CliUtil.ShouldRenderChunk(ce) && ce != null && ce.Blocks != null) foreach (Block bl in ce.Blocks.Values) { RenderCube(ce, bl); } };

                /*foreach (Chunk cch in chunks)
                {
                    try
                    {
                        foreach (Block bl in cch.Blocks.Values.ToList())
                        {
                            RenderCube(world, cch, bl);
                        }
                        RenderedChunks++;
                    }
                    catch
                    {
                        RenderErrors++;
                    }
                }*/
            }
            catch (Exception err)
            {
                Logger.PostLog("Exception: " + err.Message + " @ " + err.Source);
            }
            _shader.Use();
            GL.End();
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Quads, 0, 3);
            
        }
        static void Dot(double x, double y, double z)
        {
            GL.Begin(PrimitiveType.Points);
            GL.Color3(brightness, brightness, 0);
            GL.Vertex3(0.5f + x, 1.0f + y, 0.5f + z);
            GL.End();
        }
        private static void RenderCube(Chunk chunk, Block block)
        {
            int x = block.X + 16 * chunk.X;
            int y = block.Y;
            int z = block.Z + 16 * chunk.Z;


            if (DataStore.Player.Y + 1.3 > y)
            {
                //top
                GL.Color3(255, 255, 0);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(x, 1 + y, 1 + z);
            }
            else
            {
                //bottom
                GL.Color3(255, 255, 255);
                GL.Vertex3(x, y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (DataStore.Player.Z < z)
            {
                //left
                GL.Color3(255, 0, 0);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, y, z);
                GL.Vertex3(x, y, z);
            }
            else
            {
                //right
                GL.Color3(255, 0.5 * 255, 0);
                GL.Vertex3(x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
            }
            if (DataStore.Player.X < x)
            {
                //front
                GL.Color3(0, 255, 255);
                GL.Vertex3(x, 1 + y, z);
                GL.Vertex3(x, 1 + y, 1 + z);
                GL.Vertex3(x, y, 1 + z);
                GL.Vertex3(x, y, z);
            }
            else
            {
                //back
                GL.Color3(0, 255, 255);
                GL.Vertex3(1 + x, 1 + y, z);
                GL.Vertex3(1 + x, 1 + y, 1 + z);
                GL.Vertex3(1 + x, y, 1 + z);
                GL.Vertex3(1 + x, y, z);
            }
        }
        public void SetGameStateMW()
        {
            CursorVisible = !focussed;
        }
        // Currently unused
        /*    private int CompileShaders()
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
            }*/
    }
}
