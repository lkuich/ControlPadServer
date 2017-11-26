using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ControlHubDesktop
{
    public class Network
    {
        public static string[] GetLocalAddresses()
        {
            var networks = new List<string>();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    networks.Add(ip.ToString());

            if (networks.Count <= 0)
                throw new Exception("No network adapters with an IPv4 address in the system!");
            
            return networks.ToArray();
        }
    }
}