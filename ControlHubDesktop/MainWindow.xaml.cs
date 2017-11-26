using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            Server = new ControlHubServer.ControlHubServer();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get list of networks
            comboNetworks.ItemsSource = Network.GetLocalAddresses();
            comboNetworks.SelectedIndex = 0;

            Server.Host = comboNetworks.SelectedValue.ToString();
            Server.Start(GetInputType());

            WindowRendered = true;
        }

        private ControlHubServer.InputType GetInputType()
        {
            if (radioXbox.IsChecked.Value)
                return ControlHubServer.InputType.XBOX;
            else if (radioMouseKeyboard.IsChecked.Value)
                return ControlHubServer.InputType.STANDARD;

            return ControlHubServer.InputType.DIRECTINPUT;
        }

        private void RestartServer(string ip)
        {
            Server.Stop();

            Server.Host = ip;
            Server.Start(GetInputType());
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

            Server.X360Controller.Connect();
            RestartServer(ip);
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
