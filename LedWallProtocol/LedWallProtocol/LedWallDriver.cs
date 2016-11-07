using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace LedWallProtocol
{
    public class LedWallDriver
    {
        public int Height { get; private set; }

        public int Width { get; private set; }

        public LedWallDriver(int width, int height)
        {
            this.Height = height;
            this.Width = width;
        }

        public virtual void SetWall(Color[,] wall)
        {
            throw new NotImplementedException();
        }

        public virtual void SetWall(Color c)
        {
            throw new NotImplementedException();
        }
    }

    public class LedWallHardwareDriver : LedWallDriver
    {
        private LedWallTeensyDriver[] _drivers;
        private int _driverCount;
        private int _nonNullDriverCount;

        private int _ledsPerSection;

        public const int BaudRate = 100000000;

        private Semaphore _sectionSent;
        private Semaphore _show;

        private LedMessageShow _showMsg;

        private const byte dataACK = 0xAB;
        private const byte displayACK = 0xCD;

        private BlockingCollection<Color[,]> frames;
        private BackgroundWorker bw;
        
        int addedfps;
        Stopwatch addedsw;

        public LedWallHardwareDriver(int width, int height) : base(width, height)
        {
            frames = new BlockingCollection<Color[,]>();
            _driverCount = height / LedWallTeensyDriver.StripsPerTeensy;
            _drivers = new LedWallTeensyDriver[_driverCount];

            _ledsPerSection = LedWallTeensyDriver.StripsPerTeensy * width;

            _showMsg = new LedMessageShow();

            _drivers[00] = new LedWallTeensyDriver("COM4", BaudRate);
            _drivers[01] = new LedWallTeensyDriver("COM3", BaudRate);
            _drivers[02] = new LedWallTeensyDriver("COM6", BaudRate);
            _drivers[03] = new LedWallTeensyDriver("COM7", BaudRate);
            _drivers[04] = new LedWallTeensyDriver("COM8", BaudRate);
            _drivers[05] = new LedWallTeensyDriver("COM9", BaudRate);
            _drivers[06] = new LedWallTeensyDriver("COM10", BaudRate);
            //_drivers[07] = new LedWallTeensyDriver("COM11", BaudRate);
            //_drivers[08] = new LedWallTeensyDriver("COM13", BaudRate);
            //_drivers[09] = new LedWallTeensyDriver("COM14", BaudRate);
            //_drivers[10] = new LedWallTeensyDriver("COM15", BaudRate);
            //_drivers[11] = new LedWallTeensyDriver("COM17", BaudRate);
            //_drivers[12] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[13] = new LedWallTeensyDriver("COMX", BaudRate);

            _nonNullDriverCount = _drivers.Where(d => d != null).Count();
            _sectionSent = new Semaphore(0, _nonNullDriverCount);
            _show = new Semaphore(0, _nonNullDriverCount);

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerAsync();

            addedfps = 0;
            addedsw = new Stopwatch();
            addedsw.Start();
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(string.Format("FPS = {0}", e.ProgressPercentage));
        }

        ~LedWallHardwareDriver()
        {
            frames.CompleteAdding();
            bw.CancelAsync();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker self = sender as BackgroundWorker;
            Color[,] frame;
            int fps = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (!self.CancellationPending)
            {
                try
                {
                    frame = frames.Take();
                }
                catch { break; }

                foreachDriver((idx, driver) =>
                {
                    Color[,] section = new Color[LedWallTeensyDriver.StripsPerTeensy, Width];
                    Array.Copy(frame, idx * _ledsPerSection, section, 0, _ledsPerSection);
                    driver.Send(new LedMessageSetWall(section), _sectionSent);
                    _show.WaitOne();
                    driver.Send(_showMsg);
                });

                // Wait for all sections to send before sending the ShowMessage
                foreach (int i in Enumerable.Range(0, _nonNullDriverCount))
                    _sectionSent.WaitOne();
                _show.Release(_nonNullDriverCount);

                fps += 1;

                if(sw.ElapsedMilliseconds >= 1000.0)
                {
                    self.ReportProgress(fps);
                    fps = 0;
                    sw.Restart();
                }
            }
        }

        private void foreachDriver(Action<int, LedWallTeensyDriver> f)
        {
            for (int i = 0; i < _driverCount; i++)
            {
                if (_drivers[i] == null)
                    continue;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    object[] args = o as object[];
                    LedWallTeensyDriver d = (LedWallTeensyDriver)args[1];
                    int idx = (int)args[0];
                    f(idx, d);
                }, new object[] { i, _drivers[i] });
            }
        }

        /// <summary>
        /// Assign specific colors to every single pixel on the wall
        /// </summary>
        /// <param name="wall">2-D array of colors</param>
        public override void SetWall(Color[,] wall)
        {
            if (wall == null) throw new ArgumentNullException();

            if (wall.GetLength(0) != Height) throw new ArgumentException();
            if (wall.GetLength(1) != Width) throw new ArgumentException();

            WallStatus.Instance.FullGrid = wall;

            frames.Add(wall);
            //foreachDriver(idx => { return _showMsg; });

            addedfps += 1;

            if (addedsw.ElapsedMilliseconds >= 1000.0)
            {
                Console.WriteLine(string.Format("Added FPS = {0}", addedfps));
                addedfps = 0;
                addedsw.Restart();
            }
        }

        /// <summary>
        /// Sets the entire wall to one color
        /// </summary>
        /// <param name="c">Color</param>
        public override void SetWall(Color c)
        {
            LedMessage m = new LedMessageSolidColor(c);
            foreachDriver((idx, driver) => driver.Send(m));
        }
    }
}
