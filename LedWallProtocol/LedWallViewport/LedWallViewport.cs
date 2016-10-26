using LedWallProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LedWallViewport
{
    public partial class LedWallViewport : ServiceBase
    {
        public const int LedsPerStrip = 170;
        public const int StripCount = 112;

        public const int FrameRate = 30;
        public const int FrameDelay = 1000 / FrameRate;

        public System.Windows.Forms.Screen CapturedScreen = System.Windows.Forms.Screen.PrimaryScreen;

        private LedWallDriver _ledWall;
        BackgroundWorker bw;
        
        Bitmap[] frames = new Bitmap[2];
        int frame_idx = 0;

        Color[,] grid = new Color[StripCount, LedsPerStrip];

        public LedWallViewport()
        {
            InitializeComponent();
            _ledWall = new LedWallDriver(LedsPerStrip, StripCount);
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Bw_DoWork;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker self = sender as BackgroundWorker;

            while (!self.CancellationPending)
            {
                takeScreenshot(null);
                Thread.Sleep(FrameDelay);
            }
        }

        protected override void OnStart(string[] args)
        {
            bw.RunWorkerAsync();
        }

        protected override void OnStop()
        {
            bw.CancelAsync();
        }

        private void takeScreenshot(Object state)
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

        private void Bmp2Grid(Color[,] grid, Bitmap bmp, bool darken = false)
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
