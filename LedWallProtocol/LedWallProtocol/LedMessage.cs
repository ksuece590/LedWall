using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LedWallProtocol
{

    public enum LedMessageId : byte
    {
        SetAllOff       = 0,
        SetSolidColor   = 1,
        SetRows         = 2,
        SetCols         = 3,
        SetPixel        = 4,
        SetWall         = 5,
        SetSquare       = 6,
    }

    public class LedMessage
    {
        public const int HeaderSize = 4;

        public const byte StartMessage = 0xFF;

        public LedMessageId Id { get; private set; }

        public short Length { get; private set; }

        public LedMessage(LedMessageId id, short length)
        {
            this.Id = id;
            this.Length = length;
        }

        public virtual byte[] Serialize() { return null; }

        protected byte[] getHeader()
        {
            byte[] header = new byte[4];
            header[0] = StartMessage;
            header[1] = (byte)Id;
            System.Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, header, 2, sizeof(short));
            return header;
        }

        protected void fillHeader(byte[] buf)
        {
            if (buf.Length < HeaderSize)
            {
                throw new ArgumentException();
            }
            buf[0] = StartMessage;
            buf[1] = (byte)Id;
            System.Buffer.BlockCopy(BitConverter.GetBytes(Length), 0, buf, 2, sizeof(short));
        }
    }

    public class LedMessageOff : LedMessage
    {
        public LedMessageOff() : base(LedMessageId.SetAllOff, 0) { }

        public override byte[] Serialize()
        {
            return getHeader();
        }
    }

    public class LedMessageSolidColor : LedMessage
    {
        public Color Color { get; set; }

        public LedMessageSolidColor() : this(Color.Purple)
        { }

        public LedMessageSolidColor(Color color) : base(LedMessageId.SetSolidColor, 3)
        {
            this.Color = color;
        }

        public override byte[] Serialize()
        {
            byte[] message = new byte[HeaderSize + base.Length];
            base.fillHeader(message);
            message[HeaderSize + 0] = Color.R;
            message[HeaderSize + 1] = Color.G;
            message[HeaderSize + 2] = Color.B;

            return message;
        }
    }

    public struct LedMessageRowType
    {
        public static int Length = 4;
        public byte Row { get; private set; }
        public Color Color { get; private set; }
        public LedMessageRowType(byte row, Color color)
        {
            this.Row = row;
            this.Color = color;
        }
    }

    public class LedMessageRows : LedMessage
    {
        public LedMessageRowType[] RowSettings { get; private set; }

        public LedMessageRows(LedMessageRowType[] rowSettings) : base(LedMessageId.SetRows, (short)(rowSettings.Length * LedMessageRowType.Length))
        {
            this.RowSettings = rowSettings;
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();

            byte[] message = new byte[HeaderSize + base.Length];
            base.fillHeader(message);

            for(int i = 0; i < RowSettings.Length; i++)
            {

            }

            return base.Serialize();
        }
    }

    public class LedMessageSetWall : LedMessage
    {
        public Color[,] Grid { get; private set; }

        public LedMessageSetWall(Color[,] grid)
            : base(LedMessageId.SetWall, (short)(grid.GetLength(0) * grid.GetLength(1) * 3))
        {
            this.Grid = grid;
        }

        private byte[] _serialized = null;
        public override byte[] Serialize()
        {
            if (_serialized != null) { return _serialized; }

            int height = Grid.GetLength(0);
            int width = Grid.GetLength(1);

            _serialized = new byte[HeaderSize + base.Length];
            base.fillHeader(_serialized);

            int idx = HeaderSize;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    _serialized[idx++] = Grid[i, j].R;
                    _serialized[idx++] = Grid[i, j].G;
                    _serialized[idx++] = Grid[i, j].B;
                }
            }
            
            return _serialized;
        }
    }
}
