using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace ControlHubDesktop
{
    public class Network
    {
        public static IPAddress[] GetLocalAddresses()
        {
            var networks = new List<IPAddress>();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    networks.Add(ip);

            if (networks.Count <= 0)
                throw new Exception("No network adapters with an IPv4 address in the system!");
            
            return networks.ToArray();
        }
    }
}