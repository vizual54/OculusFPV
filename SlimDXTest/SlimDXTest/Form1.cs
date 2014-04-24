using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.Windows;

namespace SlimDXTest
{
    public partial class Form1 : Form
    {
        private Renderer renderer;
        private Capture capture;

        public Form1()
        {
            InitializeComponent();
            
            CameraSelection cameraSelection = new CameraSelection();
            cameraSelection.ShowDialog();

            renderer = new Renderer(this.pictureBox1, cameraSelection.Fullscreen);
            capture = new Capture(cameraSelection.LeftDevice, cameraSelection.RightDevice, renderer.D3DDevice);

            capture.m_leftCamera.FrameComplete += new FrameCompleteEventHandler(renderer.OnLeftFrameComplete);
            capture.m_rightCamera.FrameComplete += new FrameCompleteEventHandler(renderer.OnRightFrameComplete);
            
            MessagePump.Run(this, () =>
            {
                renderer.Render();
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            capture.startCameras();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (renderer != null)
            {
                renderer.Dispose();
                renderer = null;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.PageUp)
            {
                renderer.MoveLensLeft();
            }
            else if (e.KeyData == Keys.PageDown)
            {
                renderer.MoveLensRight();
            }
            else if (e.KeyData == Keys.Home)
            {
                renderer.IncreaseScale();
            }
            else if (e.KeyData == Keys.End)
            {
                renderer.DecreaseScale();
            }
            else if (e.KeyData == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
