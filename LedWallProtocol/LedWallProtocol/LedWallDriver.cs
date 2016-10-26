using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;
using System.Threading;

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

        private int _ledsPerSection;

        public const int BaudRate = 921600;

        public LedWallHardwareDriver(int width, int height) : base(width, height)
        {
            _driverCount = height / LedWallTeensyDriver.StripsPerTeensy;
            _drivers = new LedWallTeensyDriver[_driverCount];

            _ledsPerSection = LedWallTeensyDriver.StripsPerTeensy * width;

            _drivers[00] = new LedWallTeensyDriver("COM4",  BaudRate);
            _drivers[01] = new LedWallTeensyDriver("COM3",  BaudRate);
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
            //_drivers[14] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[15] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[16] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[17] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[18] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[19] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[20] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[21] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[22] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[23] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[24] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[25] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[26] = new LedWallTeensyDriver("COMX", BaudRate);
            //_drivers[27] = new LedWallTeensyDriver("COMX", BaudRate);
        }

        private void foreachDriver(Action<LedWallTeensyDriver, int> a)
        {
            for (int i = 0; i < _driverCount; i++)
            {
                if (_drivers[i] != null)
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        object[] args = o as object[];
                        a((LedWallTeensyDriver)args[1], (int)args[0]);
                    }, new object[] { i, _drivers[i] });
                    //a(_drivers[i], i);
            }
        }

        /// <summary>
        /// Assign specific colors to every single pixel on the wall
        /// </summary>
        /// <param name="wall">2-D array of colors</param>
        public override void SetWall(Color[,] wall)
        {
            if(wall == null) throw new ArgumentNullException();

            if (wall.GetLength(0) != Height) throw new ArgumentException();
            if (wall.GetLength(1) != Width) throw new ArgumentException();

            WallStatus.Instance.FullGrid = wall;

            foreachDriver((d, idx) =>
            {
                Color[,] section = new Color[LedWallTeensyDriver.StripsPerTeensy, Width];
                Array.Copy(wall, idx * _ledsPerSection, section, 0, _ledsPerSection);
                d.Send(new LedMessageSetWall(section));
            });
        }

        /// <summary>
        /// Sets the entire wall to one color
        /// </summary>
        /// <param name="c">Color</param>
        public override void SetWall(Color c)
        {
            LedMessage m = new LedMessageSolidColor(c);
            foreachDriver((d, idx) => d.Send(m));
        }
    }
}
