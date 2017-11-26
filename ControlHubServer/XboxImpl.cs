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
using System.Drawing;

namespace ControlHubServer
{
    class XboxImpl : XboxButtons.XboxButtonsBase
    {
        private Xbox360Controller X360Controller { get; set; }
        private List<Xbox360Buttons> ButtonsDown { get; set; }
        private Point LeftAxis { get; set; }
        private Point RightAxis { get; set; }

        private Xbox360Report Report { get; set; }

        public XboxImpl(Xbox360Controller X360Controller)
        {
            this.X360Controller = X360Controller;
            this.ButtonsDown = new List<Xbox360Buttons>();
            this.LeftAxis = new Point();
            this.RightAxis = new Point();

            this.Report = new Xbox360Report();
        }

        public override async Task PressXboxButton(IAsyncStreamReader<XboxButton> buttonStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await buttonStream.MoveNext())
            {
                var button = buttonStream.Current;

                Report.SetButtonState((Xbox360Buttons)button.Id, true);
                X360Controller.SendReport(Report);

                // ButtonsDown.Add((Xbox360Buttons)button.Id);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }

        public override async Task DepressXboxButton(IAsyncStreamReader<XboxButton> buttonStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await buttonStream.MoveNext())
            {
                var button = buttonStream.Current;

                Report.SetButtonState((Xbox360Buttons)button.Id, false);
                X360Controller.SendReport(Report);
                
                // ButtonsDown.Remove((Xbox360Buttons)button.Id);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }

        private enum Side
        {
            LEFT, RIGHT
        }

        private void SetThumbStickAxis(XboxThumbAxis axis, Side side)
        {
            int factor = 200;
            var x = axis.X * factor;
            var y = axis.Y * factor;
            if (x > short.MaxValue)
                x = short.MaxValue - 1;
            if (x < short.MinValue)
                x = short.MinValue + 1;
            if (y > short.MaxValue)
                y = short.MaxValue - 1;
            if (y < short.MinValue)
                y = short.MinValue + 1;

            Report.SetAxis(side == Side.LEFT ? Xbox360Axes.LeftThumbX : Xbox360Axes.RightThumbX, (short)(x * -1));
            Report.SetAxis(side == Side.LEFT ? Xbox360Axes.LeftThumbY : Xbox360Axes.RightThumbY, (short)y);
            X360Controller.SendReport(Report);
        }

        public override async Task XboxLeftThumbAxis(IAsyncStreamReader<XboxThumbAxis> axisStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await axisStream.MoveNext())
            {

                var axis = axisStream.Current;
                SetThumbStickAxis(axis, Side.LEFT);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }

        public override async Task XboxRightThumbAxis(IAsyncStreamReader<XboxThumbAxis> axisStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await axisStream.MoveNext())
            {
                var axis = axisStream.Current;
                SetThumbStickAxis(axis, Side.RIGHT);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }

        public override async Task XboxLeftTrigger(IAsyncStreamReader<XboxTrigger> triggerStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await triggerStream.MoveNext())
            {
                var trigger = triggerStream.Current;

                Report.SetAxis(Xbox360Axes.LeftTrigger, (short)trigger.Pressure);
                X360Controller.SendReport(Report);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }

        public override async Task XboxRightTrigger(IAsyncStreamReader<XboxTrigger> triggerStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await triggerStream.MoveNext())
            {
                var trigger = triggerStream.Current;

                Report.SetAxis(Xbox360Axes.RightTrigger, (short)trigger.Pressure);
                X360Controller.SendReport(Report);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }
    }
}
