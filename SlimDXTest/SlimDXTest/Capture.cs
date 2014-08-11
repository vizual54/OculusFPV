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
        public ICamera m_leftCamera = null;
        public ICamera m_rightCamera = null;
        public ICameraStream streamCamera = null;

        public Capture(string cameraStream, bool steroStream, Device d3dDevice)
        {
            if (steroStream)
            {
                streamCamera = new StreamingCamera(cameraStream, d3dDevice, 2);
            }
            else
            {
                streamCamera = new StreamingCamera(cameraStream, d3dDevice, 1);
            }
        }

        public Capture(string leftCamera, string rightCamera, Device d3dDevice)
        {
            if (leftCamera.ToLower().StartsWith("udp://"))
            {
                
                if (rightCamera != "" && rightCamera.ToLower().StartsWith("udp://"))
                {
                    m_rightCamera = new StreamingCamera(rightCamera, d3dDevice, 1);
                }
            }
            else
            {
                DsDevice[] systemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

                foreach (var device in systemCamereas)
                {
                    if (device.DevicePath.Equals(leftCamera))
                    {
                        if (device.Name.ToLower().StartsWith("hauppauge"))
                        {
                            m_leftCamera = new AnalogCamera(device, d3dDevice);
                        }
                        else
                        {
                            m_leftCamera = new WebCamera(device, d3dDevice);
                        }
                    }
                    if (device.DevicePath.Equals(rightCamera))
                    {
                        m_rightCamera = new WebCamera(device, d3dDevice);
                    }
                }
            }   
        }

        public void startCameras()
        {
            try
            {
                if (streamCamera != null)
                    streamCamera.StartCapture();
                if (m_leftCamera != null)
                    m_leftCamera.StartCapture();
                if (m_rightCamera != null)
                    m_rightCamera.StartCapture();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
