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

        private ControlHubServer.ControlHubServer Server { get; set; }
        private BroadcastServer BroadcastServer { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Server = new ControlHubServer.ControlHubServer();
            BroadcastServer = new BroadcastServer();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get list of networks
            comboNetworks.ItemsSource = Network.GetLocalAddresses();
            comboNetworks.SelectedIndex = 2;

            string selectedHost = comboNetworks.SelectedValue.ToString();
            new Thread(() =>
            {
                BroadcastServer.StartBroadcast(IPAddress.Parse(selectedHost));

                Server.Host = selectedHost; 
                Server.Start();
            }).Start();
            
            WindowRendered = true;
        }

        private void RestartServer(string ip)
        {
            if (Server.ServerStarted)
                Server.Stop();
            
            new Thread(() =>
            {
                while (true)
                {
                    BroadcastServer.StartBroadcast(IPAddress.Parse(ip));

                    Server.Host = ip;
                    Server.Start();

                    BroadcastServer = new BroadcastServer();
                }
            }).Start();
        }

        private void comboNetworks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!WindowRendered)
                return;
            var ip = comboNetworks.SelectedValue.ToString();
            RestartServer(ip);
        }

        private void radioXbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!WindowRendered)
                return;
            var ip = comboNetworks.SelectedValue.ToString();

            if (Server.ServerStarted)
            {
                Server.X360Controller.Connect();
                RestartServer(ip);
            }
        }

        private void radioMouseKeyboard_Checked(object sender, RoutedEventArgs e)
        {
            if (!WindowRendered)
                return;
            var ip = comboNetworks.SelectedValue.ToString();
            RestartServer(ip);
        }

        private void radioDirectInput_Checked(object sender, RoutedEventArgs e)
        {
            if (!WindowRendered)
                return;
            var ip = comboNetworks.SelectedValue.ToString();
            RestartServer(ip);
        }
    }
}
