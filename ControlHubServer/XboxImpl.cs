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
using System.Windows.Forms;

namespace ControlHub
{
    class XboxImpl : XboxButtons.XboxButtonsBase
    {
        private Xbox360Controller X360Controller { get; set; }
        private Xbox360Report Report { get; set; }

        public XboxImpl(Xbox360Controller X360Controller)
        {
            this.X360Controller = X360Controller;
            this.Report = new Xbox360Report();
        }

        private Google.Protobuf.ByteString TakeScreenshot()
        {
            using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                            Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                     Screen.PrimaryScreen.Bounds.Y,
                                     0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);

                    // Send to client
                    ImageConverter converter = new ImageConverter();
                    var image = (byte[])converter.ConvertTo(bmpScreenCapture, typeof(byte[]));
                    return Google.Protobuf.ByteString.CopyFrom(image);
                }
            }
        }

        public override async Task PressXboxButton(IAsyncStreamReader<XboxButton> buttonStream, IServerStreamWriter<ScreenshotData> responseStream, ServerCallContext context)
        {
            while (await buttonStream.MoveNext())
            {
                var button = buttonStream.Current;
                if (button.Id == 0x9001 && !X360Controller.IsConneced) // Init
                    X360Controller.Connect();

                Report.SetButtonState((Xbox360Buttons)button.Id, true);
                X360Controller.SendReport(Report);

                ScreenshotData response = new ScreenshotData { Index = 0, Content = TakeScreenshot() };
                await responseStream.WriteAsync(response);
            }
        }

        public override async Task DepressXboxButton(IAsyncStreamReader<XboxButton> buttonStream, IServerStreamWriter<ScreenshotData> responseStream, ServerCallContext context)
        {
            while (await buttonStream.MoveNext())
            {
                var button = buttonStream.Current;

                Report.SetButtonState((Xbox360Buttons)button.Id, false);
                X360Controller.SendReport(Report);

                ScreenshotData response = new ScreenshotData { Index = 0, Content = TakeScreenshot() };
                await responseStream.WriteAsync(response);
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

        public override async Task XboxLeftThumbAxis(IAsyncStreamReader<XboxThumbAxis> axisStream, IServerStreamWriter<ScreenshotData> responseStream, ServerCallContext context)
        {
            while (await axisStream.MoveNext())
            {

                var axis = axisStream.Current;
                SetThumbStickAxis(axis, Side.LEFT);

                ScreenshotData response = new ScreenshotData { Index = 0, Content = TakeScreenshot() };
                await responseStream.WriteAsync(response);
            }
        }

        public override async Task XboxRightThumbAxis(IAsyncStreamReader<XboxThumbAxis> axisStream, IServerStreamWriter<ScreenshotData> responseStream, ServerCallContext context)
        {
            while (await axisStream.MoveNext())
            {
                var axis = axisStream.Current;
                SetThumbStickAxis(axis, Side.RIGHT);

                ScreenshotData response = new ScreenshotData { Index = 0, Content = TakeScreenshot() };
                await responseStream.WriteAsync(response);
            }
        }

        public override async Task XboxLeftTrigger(IAsyncStreamReader<XboxTrigger> triggerStream, IServerStreamWriter<ScreenshotData> responseStream, ServerCallContext context)
        {
            while (await triggerStream.MoveNext())
            {
                var trigger = triggerStream.Current;

                Report.SetAxis(Xbox360Axes.LeftTrigger, (short)trigger.Pressure);
                X360Controller.SendReport(Report);

                ScreenshotData response = new ScreenshotData { Index = 0, Content = TakeScreenshot() };
                await responseStream.WriteAsync(response);
            }
        }

        public override async Task XboxRightTrigger(IAsyncStreamReader<XboxTrigger> triggerStream, IServerStreamWriter<ScreenshotData> responseStream, ServerCallContext context)
        {
            while (await triggerStream.MoveNext())
            {
                var trigger = triggerStream.Current;

                Report.SetAxis(Xbox360Axes.RightTrigger, (short)trigger.Pressure);
                X360Controller.SendReport(Report);

                ScreenshotData response = new ScreenshotData { Index = 0, Content = TakeScreenshot() };
                await responseStream.WriteAsync(response);
            }
        }
    }
}
