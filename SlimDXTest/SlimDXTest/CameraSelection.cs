using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectShowLib;

namespace SlimDXTest
{
    public partial class CameraSelection : Form
    {
        private DsDevice[] systemCamereas;
        private string leftDevice = "";
        private string rightDevice = "";
        private string streamAddress = "";
        private bool stereoStream = false;
        private string shaderMethod = "";
        private List<String> shaderMethods;
        public bool Fullscreen { get; private set; }

        public string StreamAddress
        {
            get { return streamAddress; }
            private set { streamAddress = value; }
        }

        public bool StereoStream
        {
            get { return stereoStream; }
            private set { stereoStream = value; }
        }

        public String LeftDevice
        {
            get { return leftDevice; }
            private set { leftDevice = value; }
        }

        public String RightDevice
        {
            get { return rightDevice; }
            private set { rightDevice = value; }
        }

        public String ShaderMethod
        {
            get { return shaderMethod;  }
            private set { shaderMethod = value; }
        }

        public CameraSelection()
        {
            InitializeComponent();
            
            systemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            for (int i = 0; i < systemCamereas.Length; i++)
            {
                leftCameraSelector.Items.Add(systemCamereas[i].Name);
                rightCameraSelector.Items.Add(systemCamereas[i].Name);
            }

            shaderMethods = new List<String>();
            shaderCombox.Items.Add("Single");
            shaderMethods.Add("None");
            shaderCombox.Items.Add("Oculus Rift");
            shaderMethods.Add("Oculus");
            shaderCombox.Items.Add("3D TV");
            shaderMethods.Add("3DTV");
            shaderCombox.SelectedIndex = 0;

            streamAddressCB.Items.Add("udp://10.0.0.109:5004");
            streamAddressCB.Items.Add("udp://10.0.0.113:5004");
            streamAddressCB.Items.Add("udp://127.0.0.1:5004");

            Fullscreen = false;
        }

        

        private void okButton_Click(object sender, EventArgs e)
        {
            if (streamAddressCB.SelectedIndex >= 0 || streamAddressCB.Text != "")
            {
                streamAddress = streamAddressCB.Text;
                stereoStream = StereoStreamCB.Checked;
            }
           
            if (leftCameraSelector.SelectedIndex >= 0)
            {
                leftDevice = systemCamereas[leftCameraSelector.SelectedIndex].DevicePath;
            }

            if (rightCameraSelector.SelectedIndex >= 0)
            {
                rightDevice = systemCamereas[rightCameraSelector.SelectedIndex].DevicePath;
            }

            if (shaderCombox.SelectedIndex >= 0)
            {
                ShaderMethod = shaderMethods.ElementAt(shaderCombox.SelectedIndex);
            }

            this.Hide();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {

        }

        private void fullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(fullScreenCheckBox.Checked)
            {
                Fullscreen = true;
            }
            else
            {
                Fullscreen = false;
            }
        }
    }

    struct Video_Device
    {
        public string Device_Name;
        public int Device_ID;
        public Guid Identifier;

        public Video_Device(int ID, string Name, Guid Identity = new Guid())
        {
            Device_ID = ID;
            Device_Name = Name;
            Identifier = Identity;
        }

        public override string ToString()
        {
            return String.Format("[{0} {1}:{2}]", Device_ID, Device_Name, Identifier);
        }
    }
}
