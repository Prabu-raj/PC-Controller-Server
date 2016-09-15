
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ControllerServer
{

    public class SystemDetails
    {
        //This gives the Width and the height of the desktop or laptop screen.
        private static int _deskWidth;
        private static int _deskHeight;

        //This gives details about the disk.
        private String[] _driveLabel;
        private String[] _driveSizeFree;
        private String[] _driveTotalSize;
        private String[] _driveSizeUsed;
        

        //Environment Details
        private String _machineName;
        private String _osVersion;

        private String[] _driveName;
        public String[] DriveName
        {
            get
            {
                return _driveName;
            }
            set
            {
                if (value != _driveName)
                {
                    _driveName = value;
                }
            }
        }
        public static int DeskWidth
        {
            get
            {
                return _deskWidth;
            }
            private set
            {
                if (value != _deskWidth)
                {
                    _deskWidth = value;
                }
            }
        }

        public static int DeskHeight
        {
            get
            {
                return _deskHeight;
            }
            private set
            {
                if (value != _deskHeight)
                {
                    _deskHeight = value;
                }
            }
        }

        public String[] DriveLabel
        {
            get
            {
                return _driveLabel;
            }
            set
            {
                if (value != _driveLabel)
                {
                    _driveLabel = value;
                }
            }
        }

        public String[] DriveSizeUsed
        {
            get
            {
                return _driveSizeUsed;
            }
            set
            {
                if (value != _driveSizeUsed)
                {
                    _driveSizeUsed = value;
                }
            }
        }

        public String[] DriveSizeFree
        {
            get
            {
                return _driveSizeFree;
            }
            set
            {
                if (value != _driveSizeFree)
                {
                    _driveSizeFree = value;
                }
            }
        }

        public String[] DriveTotalSize
        {
            get
            {
                return _driveTotalSize;
            }
            set
            {
                if (value != _driveTotalSize)
                {
                    _driveTotalSize = value;
                }
            }
        }

        

        public void InitializeDriveDetails()
        {
            _deskWidth = SystemInformation.VirtualScreen.Width;
            _deskHeight = SystemInformation.VirtualScreen.Height;


            _osVersion = Environment.OSVersion.ToString();
            _machineName = Environment.MachineName.ToString();

            DriveInfo[] drives = DriveInfo.GetDrives();
            int size = drives.Length;

            _driveLabel = new String[size];
            _driveSizeUsed = new String[size];
            _driveSizeFree = new String[size];
            _driveTotalSize = new String[size];
            _driveName = new String[size];


            for (int i = 0; i < drives.Length; i++)
            {
                try
                {
                    _driveLabel[i] = drives[i].VolumeLabel + " (" + drives[i].Name.TrimEnd('\\') + ")";
                    _driveName[i] = drives[i].Name;
                    _driveSizeFree[i] = Math.Round((Decimal)(drives[i].TotalFreeSpace / (1024.0 * 1024 * 1024)), 2, MidpointRounding.AwayFromZero).ToString() + " GB Free";
                    _driveTotalSize[i] = Math.Round((Decimal)(drives[i].TotalSize / (1024.0 * 1024 * 1024)), 2, MidpointRounding.AwayFromZero).ToString();
                    _driveSizeUsed[i] = Math.Round(((drives[i].TotalSize - drives[i].TotalFreeSpace) / (1024.0 * 1024 * 1024)), 2, MidpointRounding.AwayFromZero).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine("SystemDetails : " + e.ToString());
                }

            }

        }

        public void sendSystemDetails(ref Socket socket)
        {
            socket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this)));
        }
    }
}
