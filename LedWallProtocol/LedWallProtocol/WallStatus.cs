using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedWallProtocol
{
    public class WallStatus
    {
        private static WallStatus _instance = null;

        public static WallStatus Instance
        {
            get {
                if (_instance == null)
                    _instance = new WallStatus();
                return _instance;
            }
            set { _instance = value; }
        }

        private Color[,] _wallGrid = null;

        public Color[,] FullGrid {
            get { return _wallGrid; }
            set { _wallGrid = value;}
        }
    }
}
