using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControllerServer
{
    class Receiver
    {
        private static string _message;
        public static string Message
        {
            get
            {
                return _message;
            }
            private set
            {
                if (value != _message)
                {
                    _message = value;
                }
               
            }
        }

        private static bool _valueChanged;
        public static bool IsValueChanged
        {
            get
            {
                return _valueChanged;
            }
            set
            {
                if (value != _valueChanged)
                {
                    _valueChanged = value;               
                }
            }
        }

        private static Queue<String> requestQueue = new Queue<string>();

        public static void StartReceiver(Socket socket)
        {
            Thread setSignal = new Thread(new ThreadStart(setMessge));
            setSignal.Start();
            Console.WriteLine(setSignal.ManagedThreadId);

            IsValueChanged = false;
            while (true)
            {
                byte[] bytes = new byte[socket.Available];
                try
                {
                    socket.Receive(bytes);
                }
                catch(SocketException e) {
                    break;
                }

                string temp = System.Text.Encoding.UTF8.GetString(bytes);
                if (String.IsNullOrEmpty(temp))
                    continue;

                Console.WriteLine("Receiver : " + temp);
                string[] messages = temp.Split('#');

                foreach (string request in messages)
                {
                    if (!String.IsNullOrEmpty(request))
                    {
                        requestQueue.Enqueue(request);
                    }
                }
            }
        }

        private static void setMessge()
        {
            
            while (true)
            {
                if (!_valueChanged && requestQueue.Count > 0)
                {
                    Message = requestQueue.Dequeue();
                    IsValueChanged = true;
                   // Console.WriteLine("SetMessage : " + Message);
                }
            }
        }
    }

}
