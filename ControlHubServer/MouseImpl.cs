using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Service;
using WindowsInput;
using WindowsInput.Native;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ControlHubServer
{
    class MouseImpl: Mouse.MouseBase
    {
        private MouseSimulator MouseSim { get; set; }

        public MouseImpl()
        {
            var InputSim = new InputSimulator();
            MouseSim = new MouseSimulator(InputSim);
        }

        public class PlatformControls
        {
            #region WIN32
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;

                public static implicit operator Point(POINT point)
                {
                    return new Point(point.X, point.Y);
                }
            }

            /// <summary>
            /// Retrieves the cursor's position, in screen coordinates.
            /// </summary>
            /// <see>See MSDN documentation for further information.</see>
            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out POINT lpPoint);

            public static Point GetCursorPosition()
            {
                POINT lpPoint;
                GetCursorPos(out lpPoint);
                //bool success = User32.GetCursorPos(out lpPoint);
                // if (!success)

                return lpPoint;
            }
            #endregion
        }

        public Point InitialMouseCoords { get; set; }
        public override async Task MoveMouse(IAsyncStreamReader<MouseCoords> coordsStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await coordsStream.MoveNext())
            {
                var coords = coordsStream.Current;

                if (coords.Y == 0 && coords.X == 0)
                    InitialMouseCoords = PlatformControls.GetCursorPosition();

                Console.WriteLine("Moving mouse to: " + coords.X + "," + coords.Y);

                var x = InitialMouseCoords.X + coords.X;
                var y = InitialMouseCoords.Y + coords.Y;

                // MouseSim.MoveMouseTo(x, y);
                int sensitivity = 3;
                MouseSim.MoveMouseBy((coords.X * -1) / sensitivity, (coords.Y * -1) / sensitivity);
                // MouseSim.MoveMouseToPositionOnVirtualDesktop(coords.X, coords.Y);

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
        }
    }
}
