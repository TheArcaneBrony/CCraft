using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace MCClone.Client.UI
{
    /// <summary>
    /// Interaction logic for ActivityViewer.xaml
    /// </summary>
    public partial class ActivityViewer : Window
    {

        private static Bitmap chunkMap, chunkView = new Bitmap(16, 16);

        Graphics chunkMapG, chunkViewG;
        public ActivityViewer()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;

            Thread UIThr = new Thread(() =>
            {
                int idgb = 0;
                while (true)
                {
                    //  try
                    {
                        Dispatcher.Invoke((() =>
                        {
                            chunkMap = new Bitmap((int)ChunkMap.Width, (int)ChunkMap.Width);
                            chunkMapG = Graphics.FromImage(chunkMap);
                            chunkViewG = Graphics.FromImage(chunkView);

                            ThreadList.Items.Clear();
                            ThreadStateList.Items.Clear();
                            ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;

                            for (int i = 0; i < DataStore.Threads.Count; i++)
                            {
                                try
                                {
                                    Thread thread = DataStore.Threads[i];
                                    ThreadStateList.Items.Add($"{thread.ThreadState}");
                                    ThreadList.Items.Add($"{thread.Name} ({thread.ManagedThreadId})");
                                }
                                catch (NullReferenceException)
                                {

                                    ThreadStateList.Items.Add($"?");
                                    ThreadList.Items.Add($"?");
                                }
                            }
                            ThreadLabel.Content = $"Active threads: {ThreadList.Items.Count} ({currentThreads.Count})";
                        }));
                        if (MainWindow.world.Dimensions[DataStore.Player.WorldID].Chunks.Count > 1)
                        {
                            List<Chunk> chunks = MainWindow.world.Dimensions[DataStore.Player.WorldID].Chunks.Values.ToList();
                            Player plr = DataStore.Player;
                            (int X, int Y) cpos = Util.PlayerPosToChunkPos(plr.pos),
                            tcpos = TranslateCoordinate(chunkMap, cpos.X, cpos.Y);
                            if (chunks.Count > 2)
                            {
                                for (int i = 0; i < chunks.Count; i++)
                                {
                                    Chunk ch = chunks[i];
                                    (int X, int Y) pos = TranslateCoordinate(chunkMap, ch.X, ch.Z);
                                    chunkMapG.DrawRectangle(new Pen(Color.FromArgb(255, 0, 128, 0), 1), pos.X, pos.Y, 1, 1);
                                    chunkMapG.DrawRectangle(new Pen(Color.FromArgb(255, 255, 0, 0), 1), tcpos.X, tcpos.Y, 1, 1);
                                }
                                try
                                {
                                    if (MainWindow.world.Dimensions[DataStore.Player.WorldID].Chunks.ContainsKey(cpos))
                                    {
                                        Chunk cch = MainWindow.world.Dimensions[DataStore.Player.WorldID].Chunks[cpos];
                                        List<Block> blocks = cch.Blocks.Values.ToList();
                                        for (int i = 0; i < blocks.Count; i++)
                                        {
                                            Block bl = blocks[i];
                                            chunkViewG.DrawRectangle(new Pen(Color.FromArgb(255, 0, Math.Min(16 * bl.Y,255) , 0), 1), bl.X, bl.Z, 1, 1);
                                        }
                                    }
                                }
                                catch { /*MainWindow.world.Dimensions[DataStore.Player.WorldID].Chunks.Add(cpos, */TerrainGen.GetChunk(cpos.X, cpos.Y);/*);*/ throw; }
                            }
                            Dispatcher.Invoke(() => CurrentChunk.Content = $"Current chunk: {cpos.X}/{cpos.Y} ({MainWindow.world.Dimensions[DataStore.Player.WorldID].Chunks.ContainsKey(cpos)})");
                            Dispatcher.Invoke(() => ChunkMap.Source = BitmapToImageSource(chunkMap));
                            Dispatcher.Invoke(() => ChunkView.Source = BitmapToImageSource(chunkView));

                        }
                        Dispatcher.Invoke(() => Title = "MC Clone Activity Viewer | " + idgb++);
                    }
                    //catch (Exception) { throw; }
                    Thread.Sleep(500);
                }
            });
            new Thread(() => { while (true) if (!UIThr.IsAlive) UIThr.Start(); }).Start();
        }
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
        (int, int) TranslateCoordinate(Bitmap bmp, int x, int y)
        {
            return (bmp.Width / 2 + x, bmp.Height / 2 + y);
        }
    }
}
