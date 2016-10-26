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

namespace LedWallInteract
{
    public partial class WallSimulatorForm : Form
    {
        public WallSimulatorForm()
        {
            InitializeComponent();

            //listViewPixels.Items.Add(new List<Color>(170) x);
            //ListViewItem item = new ListViewItem()
            //listViewPixels.Items.Add()

            getColorGrid();

            
            listViewPixels.Items[0].SubItems[0].BackColor = Color.Aqua;
            listViewPixels.Items[0].SubItems[1].BackColor = Color.OrangeRed;
        }

        public void getColorGrid()
        {
            //Color[,] grid = WallStatus.Instance.FullGrid;
            List<string> colorHold = new List<string>();
            //List<List<string>> gridHolder = new List<List<string>>(LedWallInteract.StripCount);

            /*for(int i = 0; i < LedWallInteract.StripCount; i++)
            {
                colorHold = new List<string>(LedWallInteract.LedsPerStrip);

                for (int j = 0; j < LedWallInteract.LedsPerStrip; j++)
                    colorHold[j] = "blk";

                listViewPixels.Items.Add(colorHold);
            }*/



            for (int i = 0; i < LedWallInteract.LedsPerStrip; i++)
            {
                colorHold.Add("blk");
                listViewPixels.Columns.Add("led" + i);
            }

            //ListViewItem item = new ListViewItem(colorHold.ToArray());
            for (int i = 0; i < LedWallInteract.StripCount; i++)
                listViewPixels.Items.Add(new ListViewItem(colorHold.ToArray()));
        }
    }
}
