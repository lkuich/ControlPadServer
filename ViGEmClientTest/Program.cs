﻿using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput.Native;

namespace ViGEmClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var broadcastServer = new BroadcastServer.BroadcastServer(IPAddress.Parse("192.168.1.118"));
            broadcastServer.StartBroadcast();
            
            Console.ReadLine();
        }
    }
}
