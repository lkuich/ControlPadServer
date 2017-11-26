using System;
using Service;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Grpc.Core;
using System.Collections.Generic;
using System.Drawing;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;

namespace ControlHubServer
{
    enum InputType
    {
        STANDARD,
        DIRECTINPUT,
        XBOX
    }

    class Settings // Global app settings
    {
        public readonly static bool BUTTON_TOGGLE = false;
        public readonly static InputType INPUT_TYPE = InputType.XBOX;
    }


    public class ControlHubServer
    {
        public string Host { get; private set; }
        public int Port { get; private set; }

        private Server server;
        
        public ControlHubServer(string host = "192.168.1.118", int port = 50051)
        {
            this.Host = host;
            this.Port = port;
        }

        public void Start()
        {
            if (Settings.INPUT_TYPE == InputType.XBOX)
            {
                // TODO: Only connect when prompted
                var client = new ViGEmClient();
                var X360Controller = new Xbox360Controller(client);
                X360Controller.FeedbackReceived +=
                    (sender, eventArgs) => Console.WriteLine(
                        eventArgs.ToString() +
                        $"LM: {eventArgs.LargeMotor}, " +
                        $"SM: {eventArgs.SmallMotor}, " +
                        $"LED: {eventArgs.LedNumber}");
                X360Controller.Connect();

                server = new Server
                {
                    Services = {
                        XboxButtons.BindService(new XboxImpl(X360Controller))
                    },
                    Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
                };
            } else if (Settings.INPUT_TYPE == InputType.STANDARD || Settings.INPUT_TYPE == InputType.DIRECTINPUT)
            {
                server = new Server
                {
                    Services = {
                        StandardInput.BindService(new StandardInputImpl())
                    },
                    Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
                };
            }
            
            server.Start();

            Console.WriteLine("Server listening on port " + Port);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var s = new ControlHubServer();
            s.Start();
            Console.ReadLine();
        }
    }
}