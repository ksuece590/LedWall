using LedWallProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private const int BytesPerPixel = 4;

        /// <summary>
        /// Collection of colors to be used to display the BodyIndexFrame data.
        /// </summary>
        private static readonly uint[] BodyColor =
        {
            0x0000FF00,
            0x00FF0000,
            0xFFFF4000,
            0x40FFFF00,
            0xFF40FF00,
            0xFF808000,
        };

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for body index frames
        /// </summary>
        private BodyIndexFrameReader bodyIndexFrameReader = null;

        /// <summary>
        /// Description of the data contained in the body index frame
        /// </summary>
        private FrameDescription bodyIndexFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap bodyIndexBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private uint[] bodyIndexPixels = null;

        int width;
        int height;
        BackgroundWorker bw;
        RenderTargetBitmap rtb;

        public const int LedsPerStrip = 170;

        public const int StripCount = 112;

        private LedWallDriver _ledWall;

        private bool formClosing;

        public MainWindow()
        {
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the depth frames
            this.bodyIndexFrameReader = this.kinectSensor.BodyIndexFrameSource.OpenReader();

            // wire handler for frame arrival
            this.bodyIndexFrameReader.FrameArrived += this.Reader_FrameArrived;

            this.bodyIndexFrameDescription = this.kinectSensor.BodyIndexFrameSource.FrameDescription;

            // allocate space to put the pixels being converted
            this.bodyIndexPixels = new uint[this.bodyIndexFrameDescription.Width * this.bodyIndexFrameDescription.Height];

            // create the bitmap to display
            this.bodyIndexBitmap = new WriteableBitmap(this.bodyIndexFrameDescription.Width, this.bodyIndexFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            InitializeComponent();
            IsPlaying(false);

            // Initialize hardware driver
            _ledWall = new LedWallHardwareDriver(LedsPerStrip, StripCount);

            bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();

            formClosing = false;
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.bodyIndexBitmap;
            }
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (formClosing)
            {
                Application.Current.Shutdown();
            }
        }

        private void IsPlaying(bool flag)
        {
            btnStop.IsEnabled = flag;
            btnMoveBack.IsEnabled = flag;
            btnMoveForward.IsEnabled = flag;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            viewport.Source = null;
            IsPlaying(false);
        }

        private void btnMoveBack_Click(object sender, RoutedEventArgs e)
        {
            viewport.Position -= TimeSpan.FromSeconds(10);
        }

        private void btnMoveForward_Click(object sender, RoutedEventArgs e)
        {
            viewport.Position += TimeSpan.FromSeconds(10);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box 
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Videos"; // Default file name 
            dialog.DefaultExt = ".mp4"; // Default file extension 
            dialog.Filter = "MP4 file (.mp4)|*.mp4"; // Filter files by extension  

            // Show open file dialog box 
            Nullable<bool> result = dialog.ShowDialog();

            // Process open file dialog box results  
            if (result == true)
            {
                // Open document  
                viewport.Source = new Uri(dialog.FileName);
                IsPlaying(true);
            }
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker me = sender as BackgroundWorker;

            int[] a = new int[LedsPerStrip * StripCount];
            TransformedBitmap tbmp = null;
            System.Drawing.Color[,] grid = new System.Drawing.Color[StripCount, LedsPerStrip];

            while (!me.CancellationPending)
            {
                if(width == 0)
                {
                    viewport_border.Dispatcher.Invoke(() =>
                    {
                        PresentationSource source = PresentationSource.FromVisual(this);
                        width = (int)(viewport.ActualWidth * source.CompositionTarget.TransformToDevice.M11);
                        height = (int)(viewport.ActualHeight * source.CompositionTarget.TransformToDevice.M22);
                        if(width != 0)
                        {
                            rtb = new RenderTargetBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32);
                        }
                    });
                    Thread.Sleep(33);
                    continue;
                }

                viewport_border.Dispatcher.Invoke(() =>
                {
                    rtb.Render(viewport_border);
                    double scalex = LedsPerStrip * 1.0 / width;
                    double scaley = StripCount * 1.0 / height;
                    tbmp = new TransformedBitmap(rtb, new ScaleTransform(scalex, scaley));
                    tbmp.CopyPixels(a, 4 * LedsPerStrip, 0);
                });

                for(int i = 0; i < StripCount; i++)
                {
                    for(int j = 0; j < LedsPerStrip; j++)
                    {
                        grid[i, j] = System.Drawing.Color.FromArgb(a[i * LedsPerStrip + j]);
                    }
                }

                _ledWall.SetWall(grid);

                Thread.Sleep(33);
            }
        }

        /// <summary>
        /// Handles the body index frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            bool bodyIndexFrameProcessed = false;

            using (BodyIndexFrame bodyIndexFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyIndexFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer bodyIndexBuffer = bodyIndexFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.bodyIndexFrameDescription.Width * this.bodyIndexFrameDescription.Height) == bodyIndexBuffer.Size) &&
                            (this.bodyIndexFrameDescription.Width == this.bodyIndexBitmap.PixelWidth) && (this.bodyIndexFrameDescription.Height == this.bodyIndexBitmap.PixelHeight))
                        {
                            this.ProcessBodyIndexFrameData(bodyIndexBuffer.UnderlyingBuffer, bodyIndexBuffer.Size);
                            bodyIndexFrameProcessed = true;
                        }
                    }
                }
            }

            if (bodyIndexFrameProcessed)
            {
                this.RenderBodyIndexPixels();
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the BodyIndexFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the bodyIndexFrameData pointer.
        /// </summary>
        /// <param name="bodyIndexFrameData">Pointer to the BodyIndexFrame image data</param>
        /// <param name="bodyIndexFrameDataSize">Size of the BodyIndexFrame image data</param>
        private unsafe void ProcessBodyIndexFrameData(IntPtr bodyIndexFrameData, uint bodyIndexFrameDataSize)
        {
            byte* frameData = (byte*)bodyIndexFrameData;

            // convert body index to a visual representation
            for (int i = 0; i < (int)bodyIndexFrameDataSize; ++i)
            {
                // the BodyColor array has been sized to match
                // BodyFrameSource.BodyCount
                if (frameData[i] < BodyColor.Length)
                {
                    // this pixel is part of a player,
                    // display the appropriate color
                    this.bodyIndexPixels[i] = BodyColor[frameData[i]];
                }
                else
                {
                    // this pixel is not part of a player
                    // display black
                    this.bodyIndexPixels[i] = 0x00000000;
                }
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderBodyIndexPixels()
        {
            this.bodyIndexBitmap.WritePixels(
                new Int32Rect(0, 0, this.bodyIndexBitmap.PixelWidth, this.bodyIndexBitmap.PixelHeight),
                this.bodyIndexPixels,
                this.bodyIndexBitmap.PixelWidth * (int)BytesPerPixel,
                0);
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bw.CancelAsync();
            formClosing = true;
            e.Cancel = true;
            this.Hide();

            if (this.bodyIndexFrameReader != null)
            {
                // remove the event handler
                this.bodyIndexFrameReader.FrameArrived -= this.Reader_FrameArrived;

                // BodyIndexFrameReder is IDisposable
                this.bodyIndexFrameReader.Dispose();
                this.bodyIndexFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
    }
}
