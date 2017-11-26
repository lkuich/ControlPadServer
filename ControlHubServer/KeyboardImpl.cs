using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Service;
using WindowsInput;
using WindowsInput.Native;

namespace ControlHubServer
{
    class KeyboardImpl: Keyboard.KeyboardBase
    {
        private KeyboardSimulator KeyboardSim { get; set; }
        private List<VirtualKeyCode> CurrentKeys { get; set; }

        public KeyboardImpl()
        {
            var InputSim = new InputSimulator();
            KeyboardSim = new KeyboardSimulator(InputSim, useScanCodes: Settings.INPUT_TYPE == InputType.DIRECTINPUT);

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
    }
}
