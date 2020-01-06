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

        public static Bitmap chunkMap, chunkView = new Bitmap(16, 16);

        Graphics chunkMapG, chunkViewG;
        public ActivityViewer()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;



            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke((System.Action)(() =>
                        {
                            ActivityViewer.chunkMap = new Bitmap((int)(int)ChunkMap.Width, (int)(int)ChunkMap.Width);
                            chunkMapG = Graphics.FromImage(chunkMap);
                            chunkViewG = Graphics.FromImage(chunkView);

                            ThreadList.Items.Clear();
                            ThreadStateList.Items.Clear();
                            ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;

                            for (int i = 0; i < DataStore.Threads.Count; i++)
                            {
                                try
                                {
                                    Thread thread = DataStore.Threads[(int)i];
                                    ThreadStateList.Items.Add((object)$"{thread.ThreadState}");
                                    ThreadList.Items.Add((object)$"{thread.Name} ({thread.ManagedThreadId})");
                                }
                                catch (NullReferenceException)
                                {

                                    ThreadStateList.Items.Add((object)$"?");
                                    ThreadList.Items.Add((object)$"?");
                                }
                            }
                            ThreadLabel.Content = $"Active threads: {ThreadList.Items.Count} ({currentThreads.Count})";
                            ThreadList.Items.Add((object)"-- END OF THREAD LIST --");
                        }));
                        List<Chunk> chunks = MainWindow.world.Chunks.Values.ToList();
                        Player plr = MainWindow.world.Player;
                        (int X, int Y) cpos = ((int)(Math.Truncate(plr.X / 16)), (int)(Math.Truncate(plr.Z / 16))),
                        tcpos = TranslateCoordinate(chunkMap, cpos.X, cpos.Y);

                        for (int i = 0; i < chunks.Count; i++)
                        {
                            Chunk ch = chunks[i];
                            (int X, int Y) pos = TranslateCoordinate(chunkMap, ch.X, ch.Z);
                            chunkMapG.DrawRectangle(new Pen(Color.FromArgb(255, 0, 128, 0), 1), pos.X, pos.Y, 1, 1);
                            chunkMapG.DrawRectangle(new Pen(Color.FromArgb(255, 255, 0, 0), 1), tcpos.X, tcpos.Y, 1, 1);

                        }
                        Dispatcher.Invoke(() => ChunkMap.Source = BitmapToImageSource(chunkMap));

                        try
                        {
                            Dispatcher.Invoke(() => CurrentChunk.Content = $"Current chunk: {cpos.X}/{cpos.Y} ({MainWindow.world.Chunks.ContainsKey(cpos)})");
                            Chunk cch = MainWindow.world.Chunks[cpos];
                            List<Block> blocks = cch.Blocks.Values.ToList();
                            for (int i = 0; i < blocks.Count; i++)
                            {
                                Block bl = blocks[i];
                                chunkViewG.DrawRectangle(new Pen(Color.FromArgb(255, 0, 16 * bl.Y, 0), 1), bl.X, bl.Z, 1, 1);
                            }
                        }
                        catch { /*MainWindow.world.Chunks.Add(cpos, */TerrainGen.GetChunk(cpos.X, cpos.Y);/*);*/ }

                        Dispatcher.Invoke(() => ChunkView.Source = BitmapToImageSource(chunkView));
                    }
                    catch (Exception) { }
                    Thread.Sleep(500);
                }
            }).Start();
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
