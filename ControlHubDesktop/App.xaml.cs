using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.HockeyApp;

namespace ControlHubDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            Microsoft.HockeyApp.HockeyClient.Current.Configure("ac4fe8e9534d43e8996715473a6bd0d7");
        }
    }
}
