using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ControllerServer
{
    class FolderOrFileDetails
    {
        private String[] _folderOrFilename;
        public String[] FolderOrFilename
        {
            get
            {
                return _folderOrFilename;
            }

            set
            {
                if (value != _folderOrFilename)
                {
                    _folderOrFilename = value;
                }
            }
        }

        private String[] _fileExtension;
        public String[] FileExtension
        {
            get
            {
                return _fileExtension;
            }

            set
            {
                if (value != _fileExtension)
                {
                    _fileExtension = value;
                }
            }
        }

        private bool[] _isFolder;
        public bool[] IsFolder
        {
            get
            {
                return _isFolder;
            }

            set
            {
                if (value != _isFolder)
                {
                    _isFolder = value;
                }
            }
        }

        public void Start()
        {
            string message;

            while (true)
            {
                //Console.WriteLine(Receiver.Message + " File Or Folder Details");
                if (!Receiver.IsValueChanged)
                    continue;

                message = Receiver.Message;
                Console.WriteLine(message + " File Or Folder Details");
                if (String.IsNullOrEmpty(message) || !Receiver.IsValueChanged || String.IsNullOrWhiteSpace(message))
                    continue;
                ExplorerSignal explorerSignal = null;

                try
                {
                    explorerSignal = JsonConvert.DeserializeObject<ExplorerSignal>(message);
                }
                catch (Exception)
                {
                    continue;
                }

                if (String.IsNullOrEmpty(explorerSignal.Action))
                {
                    Receiver.IsValueChanged = false;
                    continue;
                }
                else if (explorerSignal.Action.Equals(ExplorerSignal.GET_FILES))
                {
                    Receiver.IsValueChanged = false;
                    int itemsPerPacket = 20;

                    explorerSignal.FilePath.Replace('\\', '/');
                    String[] allfolders = System.IO.Directory.GetDirectories(explorerSignal.FilePath, "*", System.IO.SearchOption.TopDirectoryOnly);
                    String[] allfiles = System.IO.Directory.GetFiles(explorerSignal.FilePath, "*", System.IO.SearchOption.TopDirectoryOnly);

                    int noOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(allfolders.Length + allfiles.Length) / Convert.ToDouble(itemsPerPacket)));
                    try
                    {
                        Connections.MySocket.Send(System.Text.Encoding.ASCII.GetBytes(noOfPackets.ToString().ToCharArray()));
                    }
                    catch
                    {
                        return;
                    }

                    int TotalLength = allfolders.Length + allfiles.Length, CurrentPacketLength;


                    for (int j = 0, i = 0; j < noOfPackets; j++)
                    {
                        String ack = Receiver.Message;
                        if (ack.Equals("SendAnother") && Receiver.IsValueChanged)
                        {
                            Receiver.IsValueChanged = false;
                            FolderOrFileDetails details = new FolderOrFileDetails();
                            if (TotalLength > itemsPerPacket)
                            {
                                CurrentPacketLength = itemsPerPacket;
                                TotalLength = TotalLength - CurrentPacketLength;
                            }
                            else
                                CurrentPacketLength = TotalLength;

                            details.FolderOrFilename = new String[CurrentPacketLength];
                            //_folderOrFilePath = new String[allfiles.Length + allfolders.Length];
                            details.FileExtension = new String[CurrentPacketLength];
                            details.IsFolder = new bool[CurrentPacketLength];
                            int k = 0;
                            for (; k < CurrentPacketLength && i < allfolders.Length; k++, i++)
                            {
                                details.FolderOrFilename[k] = Path.GetFileName(allfolders[i]);
                                details.FileExtension[k] = "";
                                details.IsFolder[k] = true;
                                //_folderOrFilePath[i] = folder;
                            }

                            if (k < CurrentPacketLength)
                            {
                                int temp = i;
                                for (int l = i - (j * itemsPerPacket); l < CurrentPacketLength + temp - (j * itemsPerPacket) - k && i < allfolders.Length + allfiles.Length; l++, i++)
                                {
                                    
                                    details.FolderOrFilename[l] = Path.GetFileNameWithoutExtension(allfiles[i - allfolders.Length]);
                                    details.FileExtension[l] = Path.GetExtension(allfiles[i - allfolders.Length]);
                                    details.IsFolder[l] = false;
                                    //_folderOrFilePath[i] = file;
                                }
                            }
                            try
                            {

                                Connections.MySocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(details)));
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("FileExplorer : JsonConvert Exception");
                            }
                        }
                        else
                        {
                            j--;
                        }
                    }

                }
                else if (explorerSignal.Action.Equals(ExplorerSignal.DOWNLOAD_FILE))
                {
                    Receiver.IsValueChanged = false;

                    explorerSignal.FilePath.Replace('\\', '/');
                    FileStream fileStream = new FileStream(explorerSignal.FilePath, FileMode.Open, FileAccess.Read);
                    new Thread(() => sendFile(fileStream)).Start();
                }
                else if (explorerSignal.Action.Equals(ExplorerSignal.OPEN_FILE))
                {
                    Receiver.IsValueChanged = false;
                    explorerSignal.FilePath.Replace('\\', '/');
                    System.Diagnostics.Process.Start(explorerSignal.FilePath);
                }
                else if (explorerSignal.Action.Equals(ExplorerSignal.END_EXPLORER))
                {
                    Receiver.IsValueChanged = false;
                    return;
                }
            } // end of while(true)
        }// end of Start()

        private void sendFile(FileStream fileStream)
        {
            int BufferSize = 4096;
            byte[] buffer = null; ;

            int NoOfPackets = Convert.ToInt32
                (Math.Ceiling(Convert.ToDouble(fileStream.Length) / Convert.ToDouble(BufferSize)));
            Console.WriteLine(NoOfPackets);
            try
            {
                Connections.MySocket.Send(System.Text.Encoding.ASCII.GetBytes(NoOfPackets.ToString().ToCharArray()));
            }
            catch
            {
                return;
            }


            int TotalLength = (int)fileStream.Length, CurrentPacketLength;

            for (int i = 0; i < NoOfPackets; i++)
            {
                if (TotalLength > BufferSize)
                {
                    CurrentPacketLength = BufferSize;
                    TotalLength = TotalLength - CurrentPacketLength;
                }
                else
                    CurrentPacketLength = TotalLength;
                buffer = new byte[CurrentPacketLength];
                fileStream.Read(buffer, 0, CurrentPacketLength);
                try
                {
                    Connections.MySocket.Send(buffer);
                }
                catch
                {
                    return;
                }
                //netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length); 
            }


            fileStream.Close();
        }

    }
}
