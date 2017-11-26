using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Service;

namespace ControlHubServer
{
    class XboxButtonsImpl : XboxButtons.XboxButtonsBase
    {
        private Xbox360Controller X360Controller { get; set; }

        public XboxButtonsImpl(Xbox360Controller X360Controller)
        {
            this.X360Controller = X360Controller;
        }

        public override async Task PressXboxButton(IAsyncStreamReader<XboxButton> buttonStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await buttonStream.MoveNext())
            {
                var button = buttonStream.Current;
                var report = new Xbox360Report();
                report.SetButtonState((Xbox360Buttons)button.Id, true);
                X360Controller.SendReport(report);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }

        public override async Task DepressXboxButton(IAsyncStreamReader<XboxButton> buttonStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await buttonStream.MoveNext())
            {
                var button = buttonStream.Current;
                var report = new Xbox360Report();
                report.SetButtonState((Xbox360Buttons)button.Id, false);
                X360Controller.SendReport(report);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }        
    }

    class XboxLeftThumbAxisImpl : XboxLeftThumbAxis.XboxLeftThumbAxisBase
    {
        private Xbox360Controller X360Controller { get; set; }
        public XboxLeftThumbAxisImpl(Xbox360Controller X360Controller)
        {
            this.X360Controller = X360Controller;
        }

        public override async Task XboxLeftThumbAxis(IAsyncStreamReader<XboxThumbAxis> axisStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await axisStream.MoveNext())
            {
                var axis = axisStream.Current;
                int factor = 100;

                var report = new Xbox360Report();
                report.SetAxis(Xbox360Axes.LeftThumbX, (short)((axis.X * factor) * -1));
                report.SetAxis(Xbox360Axes.LeftThumbY, (short)((axis.Y * factor)));
                X360Controller.SendReport(report);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }
    }

    class XboxRightThumbAxisImpl : XboxRightThumbAxis.XboxRightThumbAxisBase
    {
        private Xbox360Controller X360Controller { get; set; }
        public XboxRightThumbAxisImpl(Xbox360Controller X360Controller)
        {
            this.X360Controller = X360Controller;
        }

        public override async Task XboxRightThumbAxis(IAsyncStreamReader<XboxThumbAxis> axisStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await axisStream.MoveNext())
            {
                var axis = axisStream.Current;
                int factor = 100;

                var report = new Xbox360Report();
                report.SetAxis(Xbox360Axes.RightThumbX, (short)((axis.X * factor) * -1));
                report.SetAxis(Xbox360Axes.RightThumbY, (short)((axis.Y * factor)));
                X360Controller.SendReport(report);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }
    }
}
