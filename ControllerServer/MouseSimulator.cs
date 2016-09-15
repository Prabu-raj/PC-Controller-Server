using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControllerServer
{
    public struct INPUT
    {
        public int type;
        public MOUSEINPUT mi;
    }

    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public int dwExtraInfo;
    }

    class MouseSimulator
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);
        
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private const string END_SIMULATION = "END_SIMULATION";

        public void StartSimulation()
        {
            string message;
            while (true)
            {
                message = Receiver.Message;
               // Console.WriteLine("MouseSimulator : " + message);

                if (String.IsNullOrEmpty(message) || !Receiver.IsValueChanged || String.IsNullOrWhiteSpace(message))
                    continue;
                MouseSignal mouseSignal = null;

                try
                {
                 //   Console.WriteLine("MouseSimulator : " + message);
                    //MessageBox.Show("MouseSimulator : " + message);
                    mouseSignal = JsonConvert.DeserializeObject<MouseSignal>(message);
                }
                catch (Exception e) {
                    Console.WriteLine("JSONException : " + e);
                    Console.WriteLine("JSONException : " + message);
                   // MessageBox.Show(message);
                    continue;
                }


                int X = Cursor.Position.X;
                int Y = Cursor.Position.Y;

                if (String.IsNullOrEmpty(mouseSignal.Action))
                {
                    Receiver.IsValueChanged = false;
                    continue;
                }
                else if (mouseSignal.Action.Equals(MouseSignal.TAP))
                {
                    Console.WriteLine(message);
                    Receiver.IsValueChanged = false;
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                }

                else if (mouseSignal.Action.Equals(MouseSignal.HOLD))
                {
                    Console.WriteLine(message);
                    Receiver.IsValueChanged = false;
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
                }

                else if (mouseSignal.Action.Equals(MouseSignal.DOUBLE_TAP))
                {
                    Console.WriteLine(message);
                    Receiver.IsValueChanged = false;

                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                    //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
                }

                else if (mouseSignal.Action.Equals(MouseSignal.DRAG))
                {
                    Console.WriteLine(message);
                    Receiver.IsValueChanged = false;

                    double x = mouseSignal.XCoordinate;
                    double y = mouseSignal.YCoordinate;
                    Console.WriteLine(mouseSignal.XCoordinate + " " + " " + mouseSignal.YCoordinate + " " + mouseSignal.Action);

                    if (SystemDetails.DeskWidth > Connections.MobWidth)
                    {
                        x *= (SystemDetails.DeskWidth / Connections.MobWidth);
                    }
                    else
                    {
                        x *= (Connections.MobWidth / SystemDetails.DeskWidth);
                    }

                    if (SystemDetails.DeskHeight > Connections.MobHeight)
                    {
                        y *= (SystemDetails.DeskHeight / Connections.MobHeight);
                    }
                    else
                    {
                        y *= (Connections.MobHeight / SystemDetails.DeskHeight);
                    }
                    for (int i = 1; i < 3; ++i)
                    {
                        Cursor.Position = new Point(Cursor.Position.X + (int)Math.Round(x / 1.0005), Cursor.Position.Y + (int)Math.Round(y / 1.0005));
                    }

                    //lineDDA((int)Cursor.Position.X, (int)Cursor.Position.Y, (int)x, (int)y);
                } // end of drag.
                else if(mouseSignal.Action.Equals(MouseSignal.END_SIMULATION)) {
                    Receiver.IsValueChanged = false;
                    
                    return;
                }
            }// end of while(true).
        }// end of start simulation.
    }
}
