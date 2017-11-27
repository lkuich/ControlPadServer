using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Grpc.Core;
using Service;

public static class IPAddressExtensions
{
    public static IPAddress GetSubnetMask(this IPAddress address)
    {
        foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
            {
                if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (address.Equals(unicastIPAddressInformation.Address))
                    {
                        return unicastIPAddressInformation.IPv4Mask;
                    }
                }
            }
        }
        throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", address));
    }

    // Infer subnet
    public static IPAddress GetBroadcastAddress(this IPAddress address)
    {
        return GetBroadcastAddress(address.GetSubnetMask());
    }

    public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAdressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAdressBytes.Length != subnetMaskBytes.Length)
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

        byte[] broadcastAddress = new byte[ipAdressBytes.Length];
        for (int i = 0; i < broadcastAddress.Length; i++)
        {
            broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
        }
        return new IPAddress(broadcastAddress);
    }

    public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAdressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAdressBytes.Length != subnetMaskBytes.Length)
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

        byte[] broadcastAddress = new byte[ipAdressBytes.Length];
        for (int i = 0; i < broadcastAddress.Length; i++)
        {
            broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
        }
        return new IPAddress(broadcastAddress);
    }

    public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
    {
        IPAddress network1 = address.GetNetworkAddress(subnetMask);
        IPAddress network2 = address2.GetNetworkAddress(subnetMask);

        return network1.Equals(network2);
    }
}

public class BroadcastServer
{
    private readonly int Port = 58384;
    private UdpClient UdpClient { get; set; }
    private bool ClientRecieved { get; set; }
    
    // Broadcasts IP
    public BroadcastServer()
    {
        UdpClient = new UdpClient(Port);
    }

    public void StartBroadcast(IPAddress localAddress)
    {
        // Start listening for responses on new thread
        Thread recieverThread = new Thread(() =>
        {
            Listen(localAddress);
        });
        recieverThread.Start();

        IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, Port);
        byte[] bytes = Encoding.ASCII.GetBytes(localAddress.ToString());
        while (!ClientRecieved)
        {
            UdpClient.Send(bytes, bytes.Length, ip);
            Thread.Sleep(2000);
        }
        UdpClient.Close();
    }

    private void Listen(IPAddress localAddress)
    {
        var endPoint = new IPEndPoint(localAddress, Port);
        UdpClient.BeginReceive((IAsyncResult ar) =>
        {
            byte[] bytes = UdpClient.EndReceive(ar, ref endPoint);
            string message = Encoding.ASCII.GetString(bytes);
            if (message != "recieved")
                Listen(localAddress);
            else
            {
                ClientRecieved = true;
            }
        }, new object());

    }
}