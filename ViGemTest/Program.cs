using System;

using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client;

namespace ViGemTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ViGEmClient();

            var x360 = new Xbox360Controller(client);
            var id = x360.ProductId;
            x360.FeedbackReceived +=
                (sender, eventArgs) => Console.WriteLine(
                    eventArgs.ToString() +
                    $"LM: {eventArgs.LargeMotor}, " +
                    $"SM: {eventArgs.SmallMotor}, " +
                    $"LED: {eventArgs.LedNumber}");
            x360.Connect();
            var id2 = x360.ProductId;

            Console.ReadLine();
        }
    }
}
