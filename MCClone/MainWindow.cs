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
        Dictionary<Tuple<double,double>, Block> blockStorage = new Dictionary<Tuple<double,double>, Block>();
        double[,] ceil = new double[100,100];
        bool focussed = true;
        int centerX, centerY;
        double cxv = 0, cyv = 0, czv = 0;
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

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 6400.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

        }
        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;

            GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
            centerX = ClientRectangle.Width / 2;
            centerY = ClientRectangle.Height / 2;
            Mouse.Move += Mouse_Move;
            for (double x = -100; x < 100; x += 1)
            {
                for (double z = -100; z < 100; z += 1)
                {
                    var blockPos = new Tuple<double, double>(x, z);
                    if (!blockStorage.ContainsKey(blockPos)) blockStorage.Add(blockPos, new Block());
                    blockStorage[blockPos].Y = new Random((int)x*256+(int)z*16).Next(-100,100);
                }
            }

        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            cyv -= 0.005;
            if (cyv < -0.25) cyv = -0.25;
            var pos = new Tuple<double, double>((int)cx, (int)cz);
            if (blockStorage.ContainsKey(pos) && blockStorage[pos].Y > (cy-2.1) && cyv < 0) cyv = 0;
            cy += cyv;


            
            HandleKeyboard();
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
                if (keyState.IsKeyDown(Key.ShiftLeft)) cy -= 0.1;
                if (keyState.IsKeyDown(Key.Space)) cyv = 0.1;
                if (keyState.IsKeyDown(Key.R))
                {
                    cx = 0; cy = 100; cz = 0;
                }
                    //cy += 0.1;
            } else
            {
                if (keyState.IsKeyDown(Key.W)) clx += 0.1;
                if (keyState.IsKeyDown(Key.A)) clz -= 0.1;
                if (keyState.IsKeyDown(Key.S)) clx -= 0.1;
                if (keyState.IsKeyDown(Key.D)) clz += 0.1;
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
                //Console.WriteLine(e.XDelta + " | " + e.YDelta);
                Point center = new Point(centerX, centerY);
                Point mousePos = PointToScreen(center);
                OpenTK.Input.Mouse.SetPosition(mousePos.X, mousePos.Y);
                clx += x;
                cly += y;
               // cly += Math.Sin((y / 55) * 360);
              //  if (clx > 1) clx = -1;
                // if (cly > 1) cly = -1;
                // if (cly < -1) cly = 1;
                if (cly > 480) cly = 480;
                if (cly < -480) cly = -480;

            }
            
            
        }

        double cx = 0, cy = 0, cz = 0,clx = 0.5, cly = 0, clz = 0.5;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            
            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0} | {cx}/{cy}/{cz} | {clx}/{cly}/{clz}";

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(new Vector3((float)cx, (float)cy+2, (float)cz), new Vector3((float)(cx + Math.Cos((clx / 360)) * 360), (float)(cy+2+ Math.Sin((cly/360))*480), (float)(cz + Math.Sin((clx / 360)) * 360)), Vector3.UnitY);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            foreach (var block in blockStorage)
            {
                renderCube(block.Key.Item1, block.Value.Y, block.Key.Item2);
            }
            for (double x = -10; x < 10; x+=1)
            {
                for (double z = -10; z < 10; z += 1)
                {
                    renderCube(x, blockStorage[new Tuple<double, double>(x, z)].Y, z);
                }
            }
            
            /* renderCube(0, 0, 0);
             renderCube(1, 0, 0);
             renderCube(1, 0, 1);
             renderCube(0, 0, 1);
             renderCube(-1, 0, 1);
             renderCube(-1, 0, 0);
             renderCube(-1, 0, -1);
             renderCube(1, 0, -1);*/


            GL.Begin(BeginMode.Points);
            GL.Color3(1f, 1f, 1f);
            GL.Vertex3(clx, cly, clz);
            GL.End();
            SwapBuffers();
        }
        void renderCube(double x, double y, double z)
        {
            var blockPos = new Tuple<double, double>(x, z);
            if (!blockStorage.ContainsKey(blockPos)) blockStorage.Add(blockPos, new Block());
            blockStorage[blockPos].Y = y;
            GL.Begin(BeginMode.Quads);
            //top
            GL.Color3(1.0f, 1.0f, 0.0f);
            GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 1.0f + y, 1.0f + z);
            //bottom
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 1.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 1.0f + z);
            //left
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(0.0f + x, 0.0f + y, 0.0f + z);
            //right
            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(.0f + x, 1.0f + y, 0.0f + z);
            GL.Vertex3(.0f + x, 0.0f + y, 0.0f + z);
            GL.Vertex3(1.0f + x, 0.0f + y, 0.0f + z);
            //front
            GL.Color3(0.0f, 1.0f, 1.0f);
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
        public int XPLevel { get; set; }
        public bool DoubleXP { get; set; }
        public int XPGoal { get; set; } = 1000;
    }
}
