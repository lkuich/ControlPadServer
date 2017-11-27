using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            comboNetworks.SelectedIndex = 0;

            string selectedHost = comboNetworks.SelectedValue.ToString();
            new Thread(() =>
            {
                BroadcastServer.StartBroadcast(IPAddress.Parse(selectedHost));

                Server.Host = selectedHost;
                Server.Start(GetInputType());
            });
            
            WindowRendered = true;
        }

        private ControlHubServer.InputType GetInputType()
        {
            var inputType = ControlHubServer.InputType.DIRECTINPUT;
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (radioXbox.IsChecked.Value)
                    inputType = ControlHubServer.InputType.XBOX;
                else if (radioMouseKeyboard.IsChecked.Value)
                    inputType = ControlHubServer.InputType.STANDARD;
            }));            

            return inputType;
        }

        private void RestartServer(string ip)
        {
            if (Server.ServerStarted)
                Server.Stop();
            
            new Thread(() =>
            {
                BroadcastServer.StopBroadcast();
                BroadcastServer.StartBroadcast(IPAddress.Parse(ip));

                Server.Host = ip;
                Server.Start(GetInputType());
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
