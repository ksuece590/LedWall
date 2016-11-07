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
using Accord.Video;

namespace LedWallViewport
{
    public partial class LedWallViewport : ServiceBase
    {
        public const int LedsPerStrip = 170;
        public const int StripCount = 112;
        public const int FrameRate = 30;
        public const int FrameInterval = (1000 / FrameRate) - 1;

        public System.Windows.Forms.Screen CapturedScreen = System.Windows.Forms.Screen.AllScreens[1];

        private LedWallDriver _ledWall;

        private Color[,] grid = new Color[StripCount, LedsPerStrip];

        private AsyncVideoSource avs;

        public LedWallViewport()
        {
            InitializeComponent();
        }

        private void Avs_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap output = new Bitmap(eventArgs.Frame, new Size(LedsPerStrip, StripCount));
            Bmp2Grid(grid, output);
            _ledWall.SetWall(grid);
        }

        protected override void OnStart(string[] args)
        {
            _ledWall = new LedWallHardwareDriver(LedsPerStrip, StripCount);
            avs = new AsyncVideoSource(new ScreenCaptureStream(CapturedScreen.Bounds, FrameInterval));
            avs.NewFrame += Avs_NewFrame;
            avs.Start();
        }

        protected override void OnStop()
        {
            avs.SignalToStop();
        }

        private void Bmp2Grid(Color[,] grid, Bitmap bmp, bool darken = false)
        {
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
