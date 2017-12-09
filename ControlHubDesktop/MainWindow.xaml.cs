using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get list of networks
            var networks = Network.GetLocalAddresses();
            comboNetworks.ItemsSource = networks;
            if (networks.Length > 0)
            {
                comboNetworks.SelectedIndex = 0;

                string selectedHost = comboNetworks.SelectedValue.ToString();
                new Thread(() =>
                {
                    BroadcastServer.StartBroadcast(IPAddress.Parse(selectedHost));

                    Server.Host = selectedHost;
                    Server.Start();
                }).Start();

                WindowRendered = true;
            } else
            {
                MessageBox.Show("We couldn't detect your primary network! Ensure you have a network connection and try again.");
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
            var ip = comboNetworks.SelectedValue.ToString();
            RestartServer(ip);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopServer();
        }
    }
}
