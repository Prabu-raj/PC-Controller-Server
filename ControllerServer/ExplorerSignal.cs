using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerServer
{
    class ExplorerSignal
    {
        public const String GET_FILES = "GET_FILES";
        public const String DOWNLOAD_FILE = "DOWNLOAD_FILE";
        public const String END_EXPLORER = "END";
        public const String OPEN_FILE = "OPEN_FILE";

        private String _action;

        public String Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (value != _action)
                {
                    _action = value;
                }
            }
        }

        private String _filePath;

        public String FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (value != _filePath)
                {
                    _filePath = value;
                }
            }
        }

        public ExplorerSignal(String action, String filePath)
        {
            _action = action;
            _filePath = filePath;
        }
    }
}
