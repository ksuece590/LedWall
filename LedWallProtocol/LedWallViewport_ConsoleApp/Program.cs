using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LedWallProtocol;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Diagnostics;

namespace LedWallViewport_ConsoleApp
{
    class Program
    {
        public const int LedsPerStrip = 170;
        public const int StripCount = 112;

        public const int FrameRate = 30;
        public const int FrameDelay = 1000 / FrameRate;

        public static System.Windows.Forms.Screen CapturedScreen = System.Windows.Forms.Screen.AllScreens[1];

        private static LedWallDriver _ledWall;

        private static Bitmap[] frames = new Bitmap[2];
        private static int frame_idx = 0;

        private static Color[,] grid = new Color[StripCount, LedsPerStrip];

        private static BackgroundWorker bw;

        static void Main(string[] args)
        {
            _ledWall = new LedWallHardwareDriver(LedsPerStrip, StripCount);
            //timer = new Timer(takeScreenshot, null, 0, FrameDelay);

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerAsync();

            while (true) Thread.Sleep(50);
        }

        private static void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker self = sender as BackgroundWorker;

            Stopwatch sw = new Stopwatch();

            while (!self.CancellationPending)
            {
                sw.Restart();
                takeScreenshot();
                sw.Stop();

                if((int)sw.ElapsedMilliseconds < FrameDelay)
                    Thread.Sleep(FrameDelay - (int)sw.ElapsedMilliseconds);
            }
        }

        private static void takeScreenshot()
        {
            //Create a new bitmap if necessary
            if (frames[frame_idx % frames.Length] == null)
            {
                frames[frame_idx % frames.Length] = new Bitmap(CapturedScreen.Bounds.Width, CapturedScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            }
            Bitmap frame = frames[frame_idx % frames.Length];
            
            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(frame);
            
            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(CapturedScreen.Bounds.X, CapturedScreen.Bounds.Y, 0, 0, CapturedScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            
            Bitmap output = new Bitmap(frame, new Size(LedsPerStrip, StripCount));
            Bmp2Grid(grid, output);
            
            _ledWall.SetWall(grid);
        }

        private static void Bmp2Grid(Color[,] grid, Bitmap bmp, bool darken = false)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                Bitmap conv = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
                using (var gr = Graphics.FromImage(conv))
                    gr.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                bmp = conv;
            }

            BitmapData pixelData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            unsafe
            {
                int* pData = (int*)pixelData.Scan0.ToPointer();
                for (int i = 0; i < bmp.Height; i++)
                {
                    for (int j = 0; j < bmp.Width; j++)
                    {
                        grid[i, j] = Color.FromArgb(*pData++);

                        if (darken)
                        {
                            grid[i, j] = System.Windows.Forms.ControlPaint.Dark(grid[i, j]);
                        }
                    }
                }
            }
            bmp.UnlockBits(pixelData);
        }
    }
}
