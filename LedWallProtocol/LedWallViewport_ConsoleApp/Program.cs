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
using Accord.Video;

namespace LedWallViewport_ConsoleApp
{
    class Program
    {
        public const int LedsPerStrip = 170;
        public const int StripCount = 112;
        public const int FrameRate = 30;
        public const int FrameInterval = (1000 / FrameRate);

        public static System.Windows.Forms.Screen CapturedScreen = System.Windows.Forms.Screen.AllScreens[1];
        
        private static LedWallDriver _ledWall;

        private static Color[,] grid = new Color[StripCount, LedsPerStrip];
        
        private static AsyncVideoSource avs;

        static void Main(string[] args)
        {
            _ledWall = new LedWallHardwareDriver(LedsPerStrip, StripCount);
            
            avs = new AsyncVideoSource(new ScreenCaptureStream(CapturedScreen.Bounds, FrameInterval));
            avs.NewFrame += Avs_NewFrame;
            avs.Start();

            Console.ReadKey();
            avs.SignalToStop();
        }

        private static void Avs_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap output = new Bitmap(eventArgs.Frame, new Size(LedsPerStrip, StripCount));
            Bmp2Grid(grid, output);
            _ledWall.SetWall(grid);
        }

        private static void Bmp2Grid(Color[,] grid, Bitmap bmp, bool darken = false)
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
