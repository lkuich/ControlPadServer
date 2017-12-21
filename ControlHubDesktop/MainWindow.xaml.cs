using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ControlHubDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool WindowRendered { get; set; }

        private ControlHub.ControlHubServer Server { get; set; }
        private BroadcastServer BroadcastServer { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Server = new ControlHub.ControlHubServer();

            BroadcastServer = new BroadcastServer();

            using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                            Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                     Screen.PrimaryScreen.Bounds.Y,
                                     0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);

                    var bytes = ImageToByte(bmpScreenCapture);
                    // Send to client
                    
                }
            }
        }

        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get list of networks
            try
            {
                var networks = Network.GetLocalAddresses();
                comboNetworks.ItemsSource = networks;
            
                comboNetworks.SelectedIndex = 0;

                string selectedHost = comboNetworks.SelectedValue.ToString();
                new Thread(() =>
                {
                    BroadcastServer.StartBroadcast(IPAddress.Parse(selectedHost));

                    Server.Host = selectedHost;
                    Server.Start();
                }).Start();

                WindowRendered = true;
            } catch (Exception ex)
            {
                System.Windows.MessageBox.Show("We couldn't detect your primary network! Ensure you have a network connection and try again. (" + ex.Message + ")",
                    "Network error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close(); // Close the window
            }
        }

        private void StopServer()
        {
            // Stop will await shutdown, running threads should stop when all stops are called
            BroadcastServer.StopBroadcast();
            Server.Stop();
        }

        private void RestartServer(string ip)
        {
            StopServer();
            
            new Thread(() =>
            {
                BroadcastServer.StartBroadcast(IPAddress.Parse(ip));

                Server.Host = ip;
                Server.Start();
            }).Start();
        }

        private void comboNetworks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!WindowRendered)
                return;
            
            // TODO: Send disconnect


            var ip = comboNetworks.SelectedValue.ToString();
            RestartServer(ip);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopServer();
        }
    }
}
