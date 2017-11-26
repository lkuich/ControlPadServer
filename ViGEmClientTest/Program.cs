using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput.Native;

namespace ViGEmClientTest
{
    class Program
    {
        class Keyboard
        {
            public void PressKey(char ch, bool press)
            {
                byte vk = WindowsAPI.VkKeyScan(ch);
                ushort scanCode = (ushort)WindowsAPI.MapVirtualKey(vk, 0);

                if (press)
                    KeyDown(scanCode);
                else
                    KeyUp(scanCode);
            }

            private void KeyDown(ushort scanCode)
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
                inputs[0].ki.dwFlags = 0;
                inputs[0].ki.wScan = (ushort)(scanCode & 0xff);
                // inputs[0].ki.wVk = 0x57;
                // (ushort)WindowsAPI.MapVirtualKey(0x57, 2);
                uint intReturn = WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
                if (intReturn != 1)
                {
                    throw new Exception("Could not send key: " + scanCode);
                }
            }

            private void KeyUp(ushort scanCode)
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = WindowsAPI.INPUT_KEYBOARD;
                inputs[0].ki.wScan = scanCode;
                inputs[0].ki.dwFlags = WindowsAPI.KEYEVENTF_KEYUP;
                uint intReturn = WindowsAPI.SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
                if (intReturn != 1)
                {
                    throw new Exception("Could not send key: " + scanCode);
                }
            }
        }

        static void Main(string[] args)
        {
            var keyboard = new Keyboard();
            keyboard.PressKey('w', true);
            keyboard.PressKey('w', false);

            Console.ReadLine();
        }
    }
}
