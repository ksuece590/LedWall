using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace LedWallProtocol
{
    class LedWallTeensyDriver
    {
        /// <summary>
        /// The number of strips controlled by one Teensy
        /// </summary>
        public const int StripsPerTeensy = 8;

        /// <summary>
        /// Serial communication's baud rate
        /// </summary>
        public int Baud { get; private set; }

        /// <summary>
        /// Serial port's name
        /// </summary>
        public string Port { get; private set; }

        /// <summary>
        /// The actual serial port communicating with the Teensy
        /// </summary>
        private SerialPort _serialPort;

        /// <summary>
        /// Construct new LedWallTeensyDriver
        /// </summary>
        /// <param name="portName">Serial port's name</param>
        /// <param name="baud">Serial communication's baud rate</param>
        public LedWallTeensyDriver(string portName, int baud)
        {
            // Save port info in case of close
            this.Baud = baud;
            this.Port = portName;

            // Attempt to open
            openPort(Port, Baud, out _serialPort);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~LedWallTeensyDriver()
        {
            if(_serialPort != null)
                _serialPort.Close();
        }

        /// <summary>
        /// Sends the given LedMessage to this Teensy
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="notify">A semaphore to release upon write completion</param>
        public void Send(LedMessage message, Semaphore notify = null)
        {
            // Only actually send data when serial port is available
            if ((this._serialPort != null) || openPort(Port, Baud, out _serialPort))
            {
                byte[] buf = message.Serialize();
                try
                {
                    _serialPort.Write(buf, 0, buf.Length);
                }
                catch
                {
                    _serialPort = null;
                }
            }

            // Release semaphore
            if (notify != null)
                notify.Release();
        }

        /// <summary>
        /// Opens a serial port with the given name at the given baud rate
        /// </summary>
        /// <param name="portName">Serial port's name</param>
        /// <param name="baud">Serial communication's baud rate</param>
        /// <param name="port">Output SerialPort</param>
        /// <returns>Success/Failure</returns>
        private bool openPort(string portName, int baud, out SerialPort port)
        {
            // Local variables
            SerialPort pt = null;
            bool success = false;

            // Init port
            pt = new SerialPort();
            pt.PortName = portName;
            pt.BaudRate = baud;
            pt.WriteTimeout = SerialPort.InfiniteTimeout;

            // Attempt to open
            try
            {
                pt.Open();
                success = true;
            }
            catch
            {
                pt.Dispose();
                pt = null;
            }

            // Assign output and return success/failure
            port = pt;
            return success;
        }
    }
}
