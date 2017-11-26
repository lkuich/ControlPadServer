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
    class StandardInputImpl : StandardInput.StandardInputBase
    {
        private MouseSimulator MouseSim { get; set; }
        private KeyboardSimulator KeyboardSim { get; set; }
        private List<VirtualKeyCode> CurrentKeys { get; set; }

        public StandardInputImpl(InputType inputType)
        {
            var InputSim = new InputSimulator();
            MouseSim = new MouseSimulator(InputSim);
            KeyboardSim = new KeyboardSimulator(InputSim, useScanCodes: inputType == InputType.DIRECTINPUT);
            CurrentKeys = new List<VirtualKeyCode>();
        }

        public override async Task PressKey(IAsyncStreamReader<Key> keyStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            while (await keyStream.MoveNext())
            {
                var key = keyStream.Current;
                var keys = new VirtualKeyCode[] { (VirtualKeyCode)key.FirstId, key.SecondId == 0 ? (VirtualKeyCode)key.SecondId : 0 };
                foreach (var k in keys)
                {
                    if (k != 0) // Not a blank key
                    {
                        if (k == VirtualKeyCode.CANCEL)
                        {
                            foreach (var downKey in CurrentKeys)
                            {
                                KeyboardSim.KeyUp(downKey);
                            }
                            CurrentKeys.Clear();
                        }
                        else if (k == VirtualKeyCode.VK_W || k == VirtualKeyCode.VK_A || k == VirtualKeyCode.VK_S || k == VirtualKeyCode.VK_D) // Directional
                        {
                            if (CurrentKeys.Count > 1)
                            {
                                foreach (var downKey in CurrentKeys)
                                {
                                    KeyboardSim.KeyUp(downKey);
                                }
                                CurrentKeys.Clear();
                            }
                            CurrentKeys.Add(k);
                            KeyboardSim.KeyDown(k);
                        }
                        else // Treat as a button
                        {
                            // If HOLD enabled, just keydown, await key up
                            // IF HOLD disabled, just a keypress
                            if (Settings.BUTTON_TOGGLE)
                            {
                                KeyboardSim.KeyPress(k);
                            }
                            else // HOLD
                            {
                                CurrentKeys.Add(k);
                                KeyboardSim.KeyDown(k);
                            }
                        }
                    }
                }

                if (CurrentKeys.Count > 0)
                    Console.WriteLine(string.Join(",", CurrentKeys.ToArray()));

                Response reply = new Response { Received = true };
                await responseStream.WriteAsync(reply);
            }
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
