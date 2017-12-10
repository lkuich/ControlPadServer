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

            var networkConnectionNames = NetworkInterface.GetAllNetworkInterfaces(); //.Select(ni => ni.Name);
            foreach (var net in networkConnectionNames)
            {
                bool status = net.OperationalStatus == OperationalStatus.Up &&
                    (net.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    net.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                    !net.Name.ToUpper().Contains("VM");

                if (status)
                {
                    var ips = net.GetIPProperties().UnicastAddresses;
                    foreach (var ip in ips)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            networks.Add(ip.Address);
                }
            }

            if (networks.Count <= 0)
                throw new Exception("No network adapters with an IPv4 address in the system!");
            
            return networks.ToArray();
        }
    }
}