using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Management.Automation;
using System.Collections.ObjectModel;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp;
using FireSharp.Response;
using System.Reflection;

namespace ControlHubDesktop
{
    public partial class MainWindow : Window
    {
        private bool WindowRendered { get; set; }

        private ControlHub.ControlHubServer Server { get; set; }
        private BroadcastServer BroadcastServer { get; set; }

        private string Website { get; set; } = "www.lkuich.com";
        private string HelpSite { get; set; } = "www.lkuich.com/controlpad";

        public MainWindow()
        {
            InitializeComponent();

            Server = new ControlHub.ControlHubServer();
            BroadcastServer = new BroadcastServer();
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
                    Server.Host = selectedHost;
                    Server.Start();
                    
                    BroadcastServer.StartBroadcast(IPAddress.Parse(selectedHost));
                }).Start();

                CheckForUpdates();

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

        public async void CheckForUpdates()
        {
            try
            {
                IFirebaseConfig config = new FirebaseConfig
                {
                    AuthSecret = "D9ztDYuvGqUjZ3njymXV15MhGKGW2eAo70PNACTW",
                    BasePath = "https://controlhub-b4c62.firebaseio.com"
                };

                IFirebaseClient client = new FirebaseClient(config);
                FirebaseResponse versionResponse = await client.GetAsync("config/version");
                FirebaseResponse downloadResponse = await client.GetAsync("config/download");
                FirebaseResponse help = await client.GetAsync("config/help");

                if (help != null)
                    HelpSite = help.Body.ToString();

                var remoteVer = versionResponse.Body.ToString();
                var localVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var remoteDownload = downloadResponse.Body.ToString();

                if (localVer != remoteVer)
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show(
                        "A new version of ControlHub Server is available (" + remoteVer + "), download now?",
                        "Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );
                    if (result == System.Windows.Forms.DialogResult.Yes)
                        LaunchSite(remoteDownload);
                }
            } catch (Exception e)
            {
                System.Windows.MessageBox.Show("Could not reach the update server, ensure you have internet connectivity",
                    "Network error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LaunchSite(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        public void InstallVigem()
        {
            // Run powershell scripts to install ViGEm
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                PowerShellInstance.AddCommand("Get-Host");
                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // check the other output streams (for example, the error stream)
                if (PowerShellInstance.Streams.Error.Count > 0)
                {
                    var i = 0;
                    // error records were written to the error stream.
                    // do something with the items found.
                }

                // loop through each output object item
                foreach (PSObject outputItem in PSOutput)
                {
                    // if null object was dumped to the pipeline during the script then a null
                    // object may be present here. check for null to prevent potential NRE.
                    if (outputItem != null)
                    {
                        //TODO: do something with the output item 
                        var i = outputItem.BaseObject; //.Version.Major;
                        var x = 0;
                    }
                }
            }
        }

        private void btnHelp_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LaunchSite(HelpSite);
        }

        private void btnSite_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LaunchSite(Website);
        }
    }
}
