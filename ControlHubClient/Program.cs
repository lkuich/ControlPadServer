using System;
using Service;
using Grpc.Core;
using System.Threading.Tasks;

namespace ControlHub
{
    class ControlHubClient
    {
        public string Host { get; private set; }
        public int Port { get; private set; }

        private Robot.RobotClient RobotClient { get; set; }
        private Channel channel;

        public ControlHubClient(string host = "localhost", int port = 50051)
        {
            this.Host = host;
            this.Port = port;

            this.channel = new Channel(this.Host + ":" + this.Port, ChannelCredentials.Insecure);
            RobotClient = new Robot.RobotClient(channel);
        }

        public void PressKey(uint keyCode)
        {
            RobotClient.PressKey(new Key() { Id = keyCode });
        }

        public void MoveMouse(int x, int y)
        {
            RobotClient.MoveMouse(new MouseCoords() { X = x, Y = y });
        }

        public void Close()
        {
            channel.ShutdownAsync().Wait();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var client = new ControlHubClient();
            Console.WriteLine("Enter a key to send");

            while (true)
            {
                var keyToSend = Console.ReadLine();
                if (keyToSend == "exit")
                    return;
                if (keyToSend.Contains(","))
                {
                    var s = keyToSend.Split(',');
                    int x = int.Parse(s[0]);
                    int y = int.Parse(s[1]);
                    client.MoveMouse(x, y);
                } else
                {
                    client.PressKey(Convert.ToUInt32(keyToSend, 16));
                }
            }
        }
    }
}
