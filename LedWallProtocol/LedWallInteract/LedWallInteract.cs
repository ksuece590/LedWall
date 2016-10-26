using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LedWallProtocol;
using System.Threading;
using Accord.Video;
using Accord.Video.FFMPEG;
using System.Drawing.Imaging;
using System.Collections.Concurrent;
using System.IO;

namespace LedWallInteract
{
    public partial class LedWallInteract : Form
    {
        public const int LedsPerStrip = 170;

        public const int StripCount = 112;

        private LedWallDriver _ledWall;

        private BackgroundWorker bw;

        private AsyncVideoSource avs;

        private Color[] gamut = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
        private Color[] rainbow;

        List<TimeSpan> convTimes;

        public LedWallInteract()
        {
            InitializeComponent();
            convTimes = new List<TimeSpan>();

            // Initialize hardware driver
            _ledWall = new LedWallHardwareDriver(LedsPerStrip, StripCount);

            rainbow = new Color[LedsPerStrip];
            int steps = LedsPerStrip / gamut.Length;
            for(int i = 0; i < gamut.Length; i++)
            {
                Color[] iter = InterpolateColors(gamut[i], gamut[(i + 1)%gamut.Length], steps);
                Array.Copy(iter, 0, rainbow, i * steps, Math.Min(iter.Length, LedsPerStrip- (i* steps)));
            }

            bw = null;
            avs = null;
        }

        /// <summary>
        /// Start a background worker with the given function
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private bool startBackgroundWorker(DoWorkEventHandler func, object arg = null, ProgressChangedEventHandler pHandler=null, RunWorkerCompletedEventHandler cHandler=null)
        {
            if (bw != null)
                return false;

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += func;
            bw.RunWorkerAsync(arg);
            if (pHandler != null)
            {
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += pHandler;
            }
            if(cHandler != null)
                bw.RunWorkerCompleted += cHandler;
            return true;
        }

        /// <summary>
        /// Clean up background worker thread
        /// </summary>
        private void stopBackgroundWorker()
        {
            if (bw == null) return;

            if (bw.IsBusy)
                bw.CancelAsync();

            bw.Dispose();
            bw = null;
        }

        /// <summary>
        /// Set entire wall to a single color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            { return; }

            Color c = WhiteReduction(colorDialog.Color);
            txtColorDisplay.BackColor = c;
            setWallSolidColor(c);
        }

        /// <summary>
        /// Sets entire wall to a single color
        /// </summary>
        /// <param name="c"></param>
        private void setWallSolidColor(Color c)
        {
            Color[,] grid = new Color[StripCount, LedsPerStrip];
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j] = c;
                }
            }
            _ledWall.SetWall(grid);

            //TODO: use "set entire wall" message instead of entire grid
            //_ledWall.SetWall(c);
        }

        private void btnDoRainbow_Click(object sender, EventArgs e)
        {
            startBackgroundWorker(Bw_Rainbow);
        }

        private void Bw_Rainbow(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Color[,] grid = new Color[StripCount, LedsPerStrip];

            int rainbowIdx = 0;

            while (!worker.CancellationPending)
            {
                for (int i = 0; i < grid.GetLength(0); i++)
                {
                    for (int j = 0; j < grid.GetLength(1); j++)
                    {
                        grid[i, j] = WhiteReduction(rainbow[(j+rainbowIdx) % LedsPerStrip]);
                    }
                }
                rainbowIdx+=2;
                _ledWall.SetWall(grid);
                Thread.Sleep(33);
            }
        }

        private static Color[] InterpolateColors(Color start, Color end, int steps)
        {
            Color[] ret = new Color[steps];
            for (int i = 0; i < steps; i++)
            {
                int r = Interpolate(start.R, end.R, steps, i),
                    g = Interpolate(start.G, end.G, steps, i),
                    b = Interpolate(start.B, end.B, steps, i);
                Color c = Color.FromArgb(r, g, b);
                ret[i] = c;
            }
            return ret;
        }

        private static int Interpolate(int start, int end, int steps, int count)
        {
            float s = start, e = end, final = s + (((e - s) / steps) * count);
            return (int)final;
        }

        private void btnDoSlideshow_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Bitmap | *.bmp";
            if(openFileDialog.ShowDialog() != DialogResult.OK) { return; }

            Bitmap b = new Bitmap(openFileDialog.FileName);
            Color[,] grid = new Color[StripCount, LedsPerStrip];

            Bmp2Grid(grid, b);
            _ledWall.SetWall(grid);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (bw != null)
                stopBackgroundWorker();

            if (avs != null)
            {
                avs.Stop();
                avs = null;
            }
        }

        private void btnLoadVideo_Click(object sender, EventArgs e)
        {
            if (avs != null) return;

            openFileDialog.Filter = "MP4 | *.mp4";
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            
            avs = new AsyncVideoSource(new VideoFileSource(openFileDialog.FileName));
            avs.NewFrame += Avs_NewFrame;
            avs.Start();
        }

        private void Avs_NewFrame(object sender, NewFrameEventArgs e)
        {
            Color[,] frame = new Color[StripCount, LedsPerStrip];
            Bmp2Grid(frame, new Bitmap(e.Frame, new Size(LedsPerStrip, StripCount)));
            _ledWall.SetWall(frame);
        }

        //private void Bw_ReadVideo(object sender, DoWorkEventArgs e)
        //{
        //    // Extract path and ensure it exists
        //    string path = e.Argument as string;
        //    if (path == null || !File.Exists(path)) { return; }

        //    // Get the worker running this thread
        //    BackgroundWorker worker = sender as BackgroundWorker;

        //    // Create a thread-safe queue to hold video frames
        //    ConcurrentQueue<Color[,]> stream = new ConcurrentQueue<Color[,]>();

        //    // Prepare a BackgroundWorker to process the video for us
        //    BackgroundWorker videoReader = new BackgroundWorker();
        //    videoReader.WorkerSupportsCancellation = true;
        //    videoReader.DoWork += VideoReader_DoWork;

        //    // Open the video file
        //    VideoFileReader vfr = new VideoFileReader();
        //    vfr.Open(path);

        //    // Kick off processing thread
        //    videoReader.RunWorkerAsync(new object[] { vfr, stream });

        //    // Main loop variables
        //    Color[,] frame;
        //    long idealSleepTicks = (long)((1.0 / vfr.FrameRate) * System.Diagnostics.Stopwatch.Frequency);
        //    long delta = 0;
        //    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        //    // Wait for first frame
        //    while (stream.IsEmpty) Thread.Sleep(5);

        //    // Execute while not cancelled and frames are available
        //    while (!worker.CancellationPending && stream.TryDequeue(out frame))
        //    {
        //        sw.Restart();
        //        while (sw.ElapsedTicks < (idealSleepTicks - delta)) ;

        //        sw.Restart();
        //        _ledWall.SetWall(frame);
        //        sw.Stop();
        //        delta = sw.ElapsedTicks;
        //    }

        //    // If we've been killed before the processing thread has finished, kill it off too.
        //    if (videoReader.IsBusy)
        //        videoReader.CancelAsync();
        //}

        //private void VideoReader_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    // Get the worker running this thread
        //    BackgroundWorker worker = sender as BackgroundWorker;

        //    // Extract args
        //    object[] args = e.Argument as object[];
        //    VideoFileReader vfr = args[0] as VideoFileReader;
        //    ConcurrentQueue<Color[,]> stream = args[1] as ConcurrentQueue<Color[,]>;

        //    // Yank frames and convert them to Color grids
        //    for (int i = 0; i < vfr.FrameCount && !worker.CancellationPending; i++)
        //    {
        //        if (stream.Count > 100)
        //        {
        //            Thread.Sleep(15);
        //            continue;
        //        }

        //        Color[,] frame = new Color[StripCount, LedsPerStrip];
        //        Bitmap bmp = new Bitmap(vfr.ReadVideoFrame(), new Size(LedsPerStrip, StripCount));
        //        Bmp2Grid(frame, bmp);
        //        stream.Enqueue(frame);
        //    }
        //}

        private void Bw_ReadVideo(object sender, DoWorkEventArgs e)
        {
            // Extract path and ensure it exists
            string path = e.Argument as string;
            if (path == null || !File.Exists(path)) { return; }

            // Get the worker running this thread
            BackgroundWorker worker = sender as BackgroundWorker;

            // Open the video file
            VideoFileReader vfr = new VideoFileReader();
            vfr.Open(path);

            // Main loop variables
            Color[,] frame = new Color[StripCount, LedsPerStrip];
            long idealSleepTicks = (long)((1.0 / vfr.FrameRate) * System.Diagnostics.Stopwatch.Frequency);
            long sleepTicks = 0;
            long[] processTimes = new long[100];
            int idx = 0;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            int framesProcessed = 0;

            // Execute while not cancelled and frames are available
            do
            {
                sw.Restart();
                Bitmap bmp = new Bitmap(vfr.ReadVideoFrame(), new Size(LedsPerStrip, StripCount));
                Bmp2Grid(frame, bmp);
                _ledWall.SetWall(frame);
                sw.Stop();

                processTimes[idx++ % 100] = sw.ElapsedTicks;
                double avg = processTimes.Average();
                worker.ReportProgress((int)(framesProcessed * 100 / vfr.FrameCount), avg);

                sleepTicks = idealSleepTicks - (long)(avg * 1.22);
                //sw.Restart();
                //while (sw.ElapsedTicks < sleepTicks) ;
                Thread.Sleep(new TimeSpan(sleepTicks));

            } while (!worker.CancellationPending && ++framesProcessed < vfr.FrameCount);
        }

        private void Bmp2Grid(Color[,] grid, Bitmap bmp, bool darken=false)
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
                            grid[i, j] = ControlPaint.Dark(grid[i, j]);
                        }
                    }
                }
            }
            bmp.UnlockBits(pixelData);


            //for (int i = 0; i < grid.GetLength(0) && i < bmp.Height; i++)
            //{
            //    for (int j = 0; j < grid.GetLength(1) && j < bmp.Width; j++)
            //    {
            //        grid[i, j] = bmp.GetPixel(j, i);
            //    }
            //}
            //sw.Stop();
            //convTimes.Add(sw.Elapsed);

            //double d = convTimes.Average(t => t.TotalMilliseconds);
            //int x = 0;
        }

        private Color WhiteReduction(Color c)
        {
            if ((c.R == c.G) && (c.G == c.B) && (c.R > 128))
            {
                c = Color.FromArgb(128, 128, 128);
            }
            return c;
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            startBackgroundWorker(Bw_Sort);
        }

        private void Bw_Sort(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Color[,] grid = new Color[StripCount, LedsPerStrip];
            Color[] sort = { Color.Red, Color.Green, Color.Blue, Color.Orange };

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j] = sort[i % 4];
                }
            }

            while (!worker.CancellationPending)
            {
                _ledWall.SetWall(grid);
                Thread.Sleep(500);
            }
        }
    }
}
