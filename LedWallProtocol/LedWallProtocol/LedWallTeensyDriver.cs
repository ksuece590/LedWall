using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace LedWallProtocol
{
    class LedWallTeensyDriver
    {
        public const int StripsPerTeensy = 8;

        SerialPort _serialPort;

        public LedWallTeensyDriver(string portName, int baud)
        {
            
            try
            {
                _serialPort = new SerialPort();
                _serialPort.PortName = portName;
                _serialPort.BaudRate = baud;
                _serialPort.WriteTimeout = SerialPort.InfiniteTimeout;
                _serialPort.Open();
            }
            catch
            {
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        ~LedWallTeensyDriver()
        {
            if(_serialPort != null)
                _serialPort.Close();
        }

        public void Send(LedMessage message)
        {
            if (_serialPort == null) return;

            byte[] buf = message.Serialize();
            _serialPort.Write(buf, 0, buf.Length);
        }
    }
}
