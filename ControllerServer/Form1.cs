using System;
using System.IO;
using System.Windows.Forms;

namespace ControllerServer
{
    public partial class Form1 : Form
    {
        private  static SystemDetails systemDetails;
        public static SystemDetails SystemDetails
        {
            get
            {
                return systemDetails;
            }

            set
            {
                if (value != systemDetails)
                {
                    systemDetails = value;
                }
            }
        }

        public Form1()
        {
            systemDetails = new SystemDetails();
            InitializeComponent();
            systemDetails.InitializeDriveDetails();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Connections connections = new Connections();
            connections.StartConnection();
            label1.Text = connections.IP.ToString();
        }

    }
}
