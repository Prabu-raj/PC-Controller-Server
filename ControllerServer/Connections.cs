using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControllerServer
{
    public class Connections
    {
        private static Socket _mySocket;

        public static Socket MySocket
        {
            get
            {
                return _mySocket;
            }
            set
            {
                if (value != _mySocket)
                {
                    _mySocket = value;
                }
            }
        }

        private static int _mobWidth;
        public static int MobWidth
        {
            get
            {
                return _mobWidth;
            }
            set
            {
                if (value != _mobWidth)
                    _mobWidth = value;
            }
        }

        private static int _mobHeight;
        public static int MobHeight
        {
            get
            {
                return _mobHeight;
            }
            set
            {
                if (value != _mobHeight)
                    _mobHeight = value;
            }
        }

        private IPAddress _ip;
        public IPAddress IP
        {
            get
            {
                return _ip;
            }
            private set
            {
                if (value != _ip)
                {
                    _ip = value;
                }
            }
        }

        private TcpListener _listener;

        private string context = string.Empty;
        private const string FILE_BROWSER = "FILE_BROWSER";
        private const string MOUSE_CONTROL = "MOUSE_CONTROL";

        private Thread connectThread = null;
        private bool appPaused = false;
        private int port = 1212;

        private void SetMyIp()
        {

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            _ip = IPAddress.Parse(localIP);
        }

        public static bool IsConnected()
        {
            if (_mySocket == null)
                return false;

            bool part1 = _mySocket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (_mySocket.Available == 0);
            if (part1 & part2)
                return false;
            else
                return true;
        }

        public void StartConnection()
        {
            SetMyIp();
            _listener = new TcpListener(_ip, port);

            if (connectThread == null)
            {
                connectThread = new Thread(new ThreadStart(Connect));
                connectThread.Start();
                Console.WriteLine(connectThread.ManagedThreadId + " Thread Started(Connect)");
            }
            else
            {
                connectThread.Abort();
                connectThread = new Thread(new ThreadStart(Connect));
                connectThread.Start();
                Console.WriteLine(connectThread.ManagedThreadId + " Thread Started(Connect)");
            }
        }

        private void Connect()
        {

            try
            {
                _listener.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            while (true)
            {

                //if (!IsConnected())
                //{
                try
                {


                    _mySocket = _listener.AcceptSocket();


                    _mySocket.Send(Encoding.UTF8.GetBytes("connected"));

                    Thread startreceiver = new Thread(() => Receiver.StartReceiver(_mySocket));
                    startreceiver.Start();
                    Console.WriteLine(startreceiver.ManagedThreadId + " Thread Started(StartReceiver)");

                    if (!appPaused)
                    {
                        string resolution = String.Empty;

                        while (true)
                        {
                            if (!Receiver.IsValueChanged)
                                continue;
                            resolution = Receiver.Message;

                            if (resolution.Contains('$'))
                            {

                                Receiver.IsValueChanged = false;
                                string[] widthandheight = resolution.Split('$');

                                _mobWidth = Convert.ToInt16(widthandheight[0]);
                                _mobHeight = Convert.ToInt16(widthandheight[1]);
                                break;
                            }
                        }

                        Thread serverequest = new Thread(new ThreadStart(ServeRequest));
                        serverequest.Start();
                        Console.WriteLine(serverequest.ManagedThreadId + " Thread Started(ServeRequest)");
                        appPaused = false;
                    }
                }
                catch (Exception) { }
                //}

            }
        }

        private void ServeRequest()
        {
            string requestString;
            Thread fileExplorer = null;
            Thread mouse = null;

            while (true)
            {
                if (!Receiver.IsValueChanged)
                    continue;

                requestString = Receiver.Message;
                if (Receiver.Message.Equals(String.Empty))
                {
                    Receiver.IsValueChanged = false;
                    continue;
                }

                Console.WriteLine("Connections : " + requestString);
                if (requestString != null)
                {
                    if (requestString.Equals(FILE_BROWSER))
                    {
                        Receiver.IsValueChanged = false;
                        if (!appPaused)
                            Form1.SystemDetails.sendSystemDetails(ref _mySocket);
                        if (fileExplorer != null && fileExplorer.IsAlive)
                            fileExplorer.Abort();
                        if (mouse != null && mouse.IsAlive)
                            mouse.Abort();
                        fileExplorer = new Thread(() => new FolderOrFileDetails().Start());
                        fileExplorer.Start();
                    }
                    else if (requestString.Equals(MOUSE_CONTROL))
                    {
                        Receiver.IsValueChanged = false;
                        if (mouse != null && mouse.IsAlive)
                            mouse.Abort();
                        if (fileExplorer != null && fileExplorer.IsAlive)
                            fileExplorer.Abort();
                        mouse = new Thread(() => new MouseSimulator().StartSimulation());
                        mouse.Start();
                    }
                    else if (requestString.Equals("AppPaused"))
                    {
                        Receiver.IsValueChanged = false;
                        port++;
                        _listener.Stop();
                        _listener = new TcpListener(_ip, 1215);
                        _listener.Start();
                        appPaused = true;
                    }
                }
            }
        }
    }
}
