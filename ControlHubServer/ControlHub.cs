using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Grpc.Core;
using System.Drawing;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Service;
using WindowsInput;

namespace ControlHub
{
    public enum InputType
    {
        STANDARD,
        DIRECTINPUT,
        XBOX
    }

    class Settings // Global app settings
    {
        public readonly static bool BUTTON_TOGGLE = false;
        public readonly static InputType INPUT_TYPE = InputType.XBOX;
        public readonly static string DEFAULT_HOST = "localhost";
        public readonly static int DEFAULT_PORT = 50051;
    }
    
    public class ControlHubServer
    {
        public bool ServerStarted { get; set; }
        public string Host { get; set; }
        public int Port { get; private set; }

        private Server server;
        public Xbox360Controller X360Controller { get; set; }
        
        public ControlHubServer(string host = "localhost", int port = 50051)
        {
            this.Host = host;
            this.Port = port;
        }

        public void Start()
        {
            var client = new ViGEmClient();
            X360Controller = new Xbox360Controller(client);
            
            server = new Server
            {
                Services = {
                    XboxButtons.BindService(new XboxImpl(X360Controller)),
                    StandardInput.BindService(new StandardInputImpl()) // this))
                },
                Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
            };

            server.Start();
            ServerStarted = true;
        }

        public async void Stop()
        {
            if (ServerStarted)
            {
                try {
                    X360Controller.Disconnect();
                } catch (Exception e) { }
                await server.ShutdownAsync();
                ServerStarted = false;
            }
        }
    }

    public class ControlHubClient
    {
        public string Host { get; private set; }
        public int Port { get; private set; }

        private StandardInput.StandardInputClient StandardInputClient { get; set; }
        private Channel channel;

        public ControlHubClient(string host = "localhost", int port = 50051)
        {
            this.Host = host;
            this.Port = port;

            this.channel = new Channel(this.Host, this.Port, ChannelCredentials.Insecure);
            StandardInputClient = new StandardInput.StandardInputClient(channel);

            var InputSim = new InputSimulator();
            var MouseSim = new MouseSimulator(InputSim);
            var KeyboardSim = new KeyboardSimulator(InputSim, useScanCodes: true);

            Console.WriteLine("0 for left, 1 for right");
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                    return;
                else if (input == "0")
                {
                    // MouseSim.LeftButtonClick();
                    MouseSim.LeftButtonDown();
                    MouseSim.LeftButtonUp();
                }
                else if (input == "1")
                {
                    MouseSim.RightButtonDown();
                    MouseSim.RightButtonUp();
                }
            }
        }

        public void PressKey(uint keyCode)
        {
            var k = new Key() { FirstId = keyCode };

            // StandardInputClient.PressKey()
        }

        public void MoveMouse(int x, int y)
        {
            // StandardInputClient.MoveMouse(new MouseCoords() { X = x, Y = y });
        }

        public void Close()
        {
            channel.ShutdownAsync().Wait();
        }
    }
}