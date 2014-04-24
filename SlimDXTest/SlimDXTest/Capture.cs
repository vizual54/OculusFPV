using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;
using SlimDX.Direct3D9;

namespace SlimDXTest
{

    class Capture
    {
        public Camera m_leftCamera = null;
        public Camera m_rightCamera = null;

        public Capture(string leftCamera, string rightCamera, Device d3dDevice)
        {
            DsDevice[] systemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            
            foreach(var device in systemCamereas)
            {
                if (device.DevicePath.Equals(leftCamera))
                {
                    m_leftCamera = new Camera(device, d3dDevice);
                }
                if (device.DevicePath.Equals(rightCamera))
                {
                    m_rightCamera = new Camera(device, d3dDevice);
                }
            }
        }

        public void startCameras()
        {
            try
            {
                m_leftCamera.StartCapture();
                m_rightCamera.StartCapture();
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
