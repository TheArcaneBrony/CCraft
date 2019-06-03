using MCClone.Client;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCClone
{
    public class MainWindow : GameWindow
    {
        int[] textures;
        public static List<ModData> Mods = new List<ModData>();
        public static bool running = true, focussed = true, logger = true, multiPlayer = false;
        public static int renderDistance = 8, centerX, centerY, RenderErrors = 0, RenderedChunks = 0, LoadedMods = 0;
        public static double rt = 0, unloadDistance = 1.5, genDistance = 1.4;//1.4;
        public static World world = new World(0, 100, 0)
        {
            // Name = "DebugTestWorld"
        };
        public static float sensitivity = .1f, brightness = 1;
        public static List<Chunk> crq = new List<Chunk>();
        private readonly UI.DebugUI debugWindow = new UI.DebugUI();
        //private int _program;
        private int _vertexArray;

        public TcpClient ClientSocket = new TcpClient();
        public static NetworkStream _serverStream;
        public string Username = "DebugUser";

        public MainWindow() : base(1280, 720, GraphicsMode.Default, "The Arcane Brony#9669's Minecraft Clone", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += $" | GL Ver: {GL.GetString(StringName.Version)} | Version: {DataStore.Ver}";
            VSync = VSyncMode.Off;
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            centerX = ClientRectangle.Left + ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4/* 0.9f*/, Width / (float)Height, 1.0f, 64000f);
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
                        mod.OnLoad.Invoke(c, new object[] { });
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

            if (CliUtil.GetGameArg("world") != "null") { world.Name = CliUtil.GetGameArg("world"); }
            Console.WriteLine($"Logged in as {CliUtil.GetGameArg("username")} with password {CliUtil.GetGameArg("password")}\n");
            uint cres = 0;
            SystemUtils.NtSetTimerResolution(9000, true, ref cres);
            //CursorVisible = false;
            //_program = CompileShaders();

            //GL.GenVertexArrays(1, out _vertexArray);
            //GL.BindVertexArray(_vertexArray);
            centerX = (ushort)(ClientRectangle.Width / 2);
            centerY = (ushort)(ClientRectangle.Height / 2);
            
            Point center = PointToScreen(new Point(centerX, centerY));
            OpenTK.Input.Mouse.SetPosition(center.X, center.Y);

            world.Player = new Player(world.SpawnX, world.SpawnY, world.SpawnZ)
            {
                Flying = true,
                Name = CliUtil.GetGameArg("username")
            };

            TerrainGen.world = world;
            Thread gameInit = new Thread(() =>
            {
                TerrainGen.GenTerrain();
                while (true)
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
                        /*for (int x = (int)(world.Player.X / 16 - renderDistance * genDistance); x < world.Player.X / 16 + renderDistance * genDistance; x++)
                        {
                            for (int z = (int)(world.Player.Z / 16 - renderDistance * genDistance); z < world.Player.Z / 16 + renderDistance * genDistance; z++)
                            {
                                if (!world.Chunks.ContainsKey((x, z)))
                                {
                                    TerrainGen.GetChunk(x, z);
                                }
                            }
                            Thread.Sleep(100);
                        }*/
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
                            if (X < (int)(world.Player.X / 16 - renderDistance * unloadDistance)
                            || X > (int)(world.Player.X / 16 + renderDistance * unloadDistance)
                            || Z < (int)(world.Player.Z / 16 - renderDistance * unloadDistance)
                            || Z > (int)(world.Player.Z / 16 + renderDistance * unloadDistance)
                            )
                            {
                                world.Chunks.Remove((X, Z));
                            }
                        }
                        Logger.LogQueue.Enqueue($"Unloading chunks took {Math.Round(Time.ElapsedTicks / 10000d, 4)} ms {lctu}");
                    }
                }
            });
            Thread logThread = new Thread(() =>
            {
                while (logger)
                {
                    Logger.LogQueue.Enqueue(
                        $"Ver: {DataStore.Ver}\n" +
                        $"FPS: {Math.Round(1f / RenderTime, 5)} ({Math.Round(RenderTime * 1000, 5)} ms)\n" +
                        $"Windows version: {Environment.OSVersion}\n" +
                        $"CPU Cores: {Environment.ProcessorCount}\n" +
                        $".NET version: {Environment.Version}\n" +
                        $"Ingame Name: {CliUtil.GetGameArg("username")}\n" +
                        $"Windows Username: {Environment.UserName}\n" +
                        $"Windows Network Name: {Environment.MachineName}\n" +
                        $"Process Working Set: {Math.Round((Environment.WorkingSet / (double)(1024 * 1024)), 4)} MB ({Environment.WorkingSet} B)\n" +
                        $"Player Pos: {world.Player.X}/{world.Player.Y}/{world.Player.Z}\n" +
                        $"Camera angle: {world.Player.LX}/{world.Player.LY}\n" +
                        $"Render Errors: {RenderErrors}\n" +
                        $"Rendered Chunks: {RenderedChunks / 256}/{world.Chunks.Count}"
                        );
                    Thread.Sleep(10000);
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
                            case "debug":
                                debugWindow.Show();
                                debugWindow.UpdateUI("test string");
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

            // kbdLogic.Start();
            //Thread.Sleep(5);

            logThread.IsBackground = true;
            logThread.Priority = ThreadPriority.Lowest;
            logThread.Start();
            //Thread.Sleep(5);
            consoleInput.IsBackground = true;
            consoleInput.Priority = ThreadPriority.Lowest;
            consoleInput.Start();
            //Thread.Sleep(5);
            gameInit.IsBackground = true;
            gameInit.Priority = ThreadPriority.BelowNormal;
            gameInit.Start();
            Logger.Start();
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            GL.Enable(EnableCap.DepthTest);

            //  GL.ShadeModel(ShadingModel.Smooth);

            //GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);


            // create texture ids
            //  GL.Enable(EnableCap.Texture2D);
            /* GL.GenTextures(2, textureIds);

             LoadTexture(context, Resource.Drawable.pattern, textureIds[0]);
             LoadTexture(context, Resource.Drawable.f_spot, textureIds[1]);*/

            /*
            GL.Enable(EnableCap.Texture2D);
            Bitmap bitmap = new Bitmap("Resources/Textures/bedrock.png");

            GL.GenTextures(1, textures);
            GL.BindTexture(TextureTarget.Texture2D, 1);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            */
            Init(world.Player.Name);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Input.HandleInput();
            Input.Tick();
            GL.ClearColor(0.1f * brightness, 0.5f * brightness, 0.7f * brightness, 0.9f);
            Title = $"MC Clone {DataStore.Ver} | FPS: {Math.Round(1000 / rt, 2)} ({Math.Round(rt, 2)} ms) C: {crq.Count}/{world.Chunks.Count} E: {RenderErrors} | {world.Player.X}/{world.Player.Y}/{world.Player.Z} : {world.Player.LX}/{world.Player.LY} | {Math.Round((double)Process.GetCurrentProcess().PrivateMemorySize64 / 1048576, 2)} MB | {TerrainGen.runningThreads}/{TerrainGen.maxThreads} GT | Mods: {LoadedMods}"; //{Math.Round(vol * 100, 0)}
            foreach (ModData mod in Mods)
            {
                if (mod.OnResize != null)
                {
                    mod.OnUpdateFrame.Invoke(mod.Instance, new object[] { e });
                }
            }
        }

        List<Chunk> crq2 = new List<Chunk>();
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rt = frameTime.ElapsedTicks / 10000d;
            frameTime.Restart();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 modelview = Matrix4.LookAt(world.Player.CPos, world.Player.CFPt, Vector3.UnitY);

            GL.MatrixMode(MatrixMode.Modelview);
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
            GL.LoadMatrix(ref modelview);
            RenderedChunks = 0;


            //GL.Begin(PrimitiveType.Quads);
            try
            {
                crq2 = new List<Chunk>(world.Chunks.Values).FindAll((Chunk Ch) => { return CliUtil.ShouldRenderChunk(Ch); });
            }
            catch { }
            foreach (Chunk cch in crq2)
            {
                try
                {
                    foreach (Block bl in new List<Block>(cch.Blocks.Values))
                    {
                        RenderCube(world, cch, bl);
                    }
                    RenderedChunks++;
                }
                catch
                {
                    RenderErrors++;
                }
            }
            GL.VertexPointer(2, VertexPointerType.Float, Vector2.SizeInBytes, g_vertex_buffer_data.ToArray());
            GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes, g_color_buffer_data.ToArray());
            GL.DrawArrays(PrimitiveType.Quads, 0, g_vertex_buffer_data.Count);
            //GL.End();
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

            /*    GL.MatrixMode(MatrixMode.Projection);

                GL.LoadIdentity();


                GL.MatrixMode(MatrixMode.Modelview);

                GL.LoadIdentity();*/

            SwapBuffers();
        }
        /*static void Dot(double x, double y, double z)
        {
            GL.Begin(PrimitiveType.Points);
            GL.Color3(brightness, brightness, 0);
            GL.Vertex3(0.5f + x, 1.0f + y, 0.5f + z);
            GL.End();
        }*/
        static List<float> g_vertex_buffer_data = new List<float>();
        static List<float> g_color_buffer_data = new List<float>();
        private static void RenderCube(World world, Chunk chunk, Block block)
        {
            int x = block.X + 16 * chunk.X;
            int y = block.Y;
            int z = block.Z + 16 * chunk.Z;

            g_color_buffer_data.AddRange(new float[] { brightness, brightness, 0 });
            g_color_buffer_data.AddRange(new float[] { brightness, brightness, 0 });
            g_color_buffer_data.AddRange(new float[] { brightness, brightness, 0 });
            if (world.Player.Y + 1.3 > y)
            {
                //top

                g_vertex_buffer_data.AddRange(new float[]{ x, 1 + y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, 1 + y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, 1 + y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{x, 1 + y, 1 + z});
            }
            else
            {
                //bottom
                g_vertex_buffer_data.AddRange(new float[]{x, y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{x, y, 1 + z});
            }
            if (world.Player.Z < z)
            {
                //left
                g_vertex_buffer_data.AddRange(new float[]{x, 1 + y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, 1 + y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, y, z});
                g_vertex_buffer_data.AddRange(new float[]{x, y, z});
            }
            else
            {
                //right
                g_vertex_buffer_data.AddRange(new float[]{x, 1 + y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, 1 + y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{x, y, 1 + z});
            }
            if (world.Player.X < x)
            {
                //front
                g_vertex_buffer_data.AddRange(new float[]{x, 1 + y, z});
                g_vertex_buffer_data.AddRange(new float[]{x, 1 + y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{x, y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{x, y, z});
            }
            else
            {
                //back
                g_vertex_buffer_data.AddRange(new float[]{1 + x, 1 + y, z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, 1 + y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, y, 1 + z});
                g_vertex_buffer_data.AddRange(new float[]{1 + x, y, z});
            }
            /* if (world.Player.Y + 1.3 > y)
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
                 GL.Vertex3(x, 1 + y, z);
                 GL.Vertex3(x, 1 + y, 1 + z);
                 GL.Vertex3(x, y, 1 + z);
                 GL.Vertex3(x, y, z);
             }
             else
             {
                 //back
                 GL.Color3(0, brightness, brightness);
                 GL.Vertex3(1 + x, 1 + y, z);
                 GL.Vertex3(1 + x, 1 + y, 1 + z);
                 GL.Vertex3(1 + x, y, 1 + z);
                 GL.Vertex3(1 + x, y, z);
             }*/
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
        private void Connection()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    try
                    {
                        if (_serverStream == null) continue;
                        var inStream = new List<byte>();
                        var returndata = "";
                        while (!returndata.Contains("\0MSGEND\0"))
                        {
                            var inBytes = new byte[1];
                            _serverStream.Read(inBytes, 0, 1);
                            inStream.AddRange(inBytes);
                            returndata = Encoding.Unicode.GetString(inStream.ToArray());
                        }

                        returndata = returndata.Replace("\0MSGEND\0", "");

                        if (returndata.StartsWith("\0SRVMSG\0"))
                        {
                            switch (returndata.Replace("\0SRVMSG\0", ""))
                            {
                                case "ping":
                                    // Send("\0CLIMSG\0ping");
                                    break;
                                case "exit":
                                    break;
                            }

                        }
                        else
                        {

                        }

                        new Task(() =>
                        {
                            new SoundPlayer(@"C:\Windows\Media\Windows Default.wav").Play();
                        }).Start();
                        Console.WriteLine(returndata);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }


            }).Start();
        }
        public void Shutdown()
        {
            var outStream = Encoding.Unicode.GetBytes("\0CLIMSG\0exit");
            _serverStream.Write(outStream, 0, outStream.Length);
            _serverStream.Close(1000);
            Environment.Exit(0);
        }
        private void Send(string text)
        {
            try
            {
                var sendBytes = Encoding.Unicode.GetBytes("Test");
                _serverStream.Write(sendBytes, 0, sendBytes.Length);
            }
            catch
            {
                Logger.LogQueue.Enqueue("Connection failure, reconnecting~!");
                Init(Username);
            }
        }
        public void Init(string username)
        {
            if (multiPlayer)
            {
                Username = username;

                try
                {
#if DEBUG
                    try
                    {
                        ClientSocket.Connect("127.0.0.1", 13372);
                    }
                    catch
                    {
                        ClientSocket.Connect("thearcanebrony.net", 13372);
                    }
#else
                ClientSocket.Connect("thearcanebrony.ddns.net", 8888);
#endif
                    Logger.LogQueue.Enqueue($"Connected as {username}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Logger.LogQueue.Enqueue("Connection failed!");
                }
                _serverStream = ClientSocket.GetStream();
                NetworkHelper.Send(_serverStream, $"login {CliUtil.GetGameArg("username")} {CliUtil.GetGameArg("password")}");
            }
        }
    }
}
