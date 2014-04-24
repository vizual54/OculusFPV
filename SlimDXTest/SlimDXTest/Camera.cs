using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DirectShowLib;
using SlimDX;
using SlimDX.Direct3D9;

namespace SlimDXTest
{
    public delegate void FrameCompleteEventHandler(ref Texture texture, float fps);

    class Camera : ISampleGrabberCB, IDisposable
    {
        public event FrameCompleteEventHandler FrameComplete;

        private object lockObject = new object();
        private IMediaControl mediaControl = null;
        private IMediaEvent mediaEvent = null;
        private IGraphBuilder pGraph = null;
        private int videoWidth = 0;
        private int videoHeight = 0;
        private byte[] frameData = new byte[1280 * 720 * 4];
        private Device d3dDevice;
        public Texture texture;
        public System.Drawing.Bitmap currentBitmap = null;
        private Test bitmapWindow;
        private bool writeOnce = true;
        private long elapsed = 0;
        private int countElapsed = 0;
        private float _fps = 0f;

        public Camera(DsDevice device, Device d3dDevice)
        {
            //bitmapWindow = new Test();
            //bitmapWindow.Show();
            this.d3dDevice = d3dDevice;

            texture = new Texture(d3dDevice, 1280, 720, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
            //texture = Texture.FromFile(d3dDevice, ".\\Images\\checkerboard.jpg", D3DX.DefaultNonPowerOf2, D3DX.DefaultNonPowerOf2, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, Filter.None, Filter.None, 0);
            var desc = texture.GetLevelDescription(0);
            DataRectangle dr = texture.LockRectangle(0, LockFlags.Discard);
            int rowPitch = dr.Pitch;
            texture.UnlockRectangle(0);
            

            try
            {
                BuildGraph(device);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public float FPS
        {
            get { lock (lockObject) { return _fps; } }
        }

        public byte[] FrameData
        {
            get { lock (lockObject) { return frameData; } }
            private set { lock (lockObject) { frameData = value; } }
        }

        private void BuildGraph(DirectShowLib.DsDevice dsDevice)
        {
            int hr = 0;
            pGraph = new FilterGraph() as IFilterGraph2;

            //graph builder
            ICaptureGraphBuilder2 pBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

            try
            {
                hr = pBuilder.SetFiltergraph(pGraph);
                DsError.ThrowExceptionForHR(hr);

                // Add camera
                IBaseFilter camera;
                //hr = pGraph.FindFilterByName(dsDevice.Name, out camera);
                hr = ((IFilterGraph2)pGraph).AddSourceFilterForMoniker(dsDevice.Mon, null, dsDevice.Name, out camera);
                DsError.ThrowExceptionForHR(hr);

                hr = pGraph.AddFilter(camera, "camera");
                DsError.ThrowExceptionForHR(hr);

                // Set format for camera
                AMMediaType pmt = new AMMediaType();
                pmt.majorType = MediaType.Video;
                pmt.subType = MediaSubType.MJPG;
                pmt.formatType = FormatType.VideoInfo;
                pmt.fixedSizeSamples = true;
                pmt.formatSize = 88;
                pmt.sampleSize = 2764800;
                pmt.temporalCompression = false;
                VideoInfoHeader format = new VideoInfoHeader();
                format.SrcRect = new DsRect();
                format.TargetRect = new DsRect();
                format.BitRate = 663552000;
                format.AvgTimePerFrame = 333333;
                format.BmiHeader = new BitmapInfoHeader();
                format.BmiHeader.Size = 40;
                format.BmiHeader.Width = 1280;
                format.BmiHeader.Height = 720;
                format.BmiHeader.Planes = 1;
                format.BmiHeader.BitCount = 24;
                format.BmiHeader.Compression = 1196444237;
                format.BmiHeader.ImageSize = 2764800;
                pmt.formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(format));
                Marshal.StructureToPtr(format, pmt.formatPtr, false);
                hr = ((IAMStreamConfig)DsFindPin.ByCategory(camera, PinCategory.Capture, 0)).SetFormat(pmt);
                //hr = ((IAMStreamConfig)GetPin(pUSB20Camera, "Capture")).SetFormat(pmt);
                DsUtils.FreeAMMediaType(pmt);
                DsError.ThrowExceptionForHR(hr);

                //add MJPG Decompressor
                IBaseFilter pMJPGDecompressor = (IBaseFilter)new MjpegDec();
                hr = pGraph.AddFilter(pMJPGDecompressor, "MJPG Decompressor");
                DsError.ThrowExceptionForHR(hr);

                //add color space converter
                IBaseFilter pColorSpaceConverter = (IBaseFilter)new Colour();
                hr = pGraph.AddFilter(pColorSpaceConverter, "Color space converter");
                DsError.ThrowExceptionForHR(hr);

                // Connect camera and MJPEG Decomp
                //hr = pGraph.ConnectDirect(GetPin(pUSB20Camera, "Capture"), GetPin(pMJPGDecompressor, "XForm In"), null);
                hr = pGraph.ConnectDirect(DsFindPin.ByCategory(camera, PinCategory.Capture, 0), DsFindPin.ByName(pMJPGDecompressor, "XForm In"), null);
                DsError.ThrowExceptionForHR(hr);

                // Connect MJPG Decomp and color space converter
                hr = pGraph.ConnectDirect(DsFindPin.ByName(pMJPGDecompressor, "XForm Out"), DsFindPin.ByName(pColorSpaceConverter, "Input"), null);
                DsError.ThrowExceptionForHR(hr);

                //add SampleGrabber
                //IBaseFilter pSampleGrabber = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_SampleGrabber));
                //hr = pGraph.AddFilter(pSampleGrabber, "SampleGrabber");
                IBaseFilter sampleGrabber = new SampleGrabber() as IBaseFilter;
                hr = pGraph.AddFilter(sampleGrabber, "Sample grabber");
                DsError.ThrowExceptionForHR(hr);

                // Configure the samplegrabber
                AMMediaType pSampleGrabber_pmt = new AMMediaType();
                pSampleGrabber_pmt.majorType = MediaType.Video;
                pSampleGrabber_pmt.subType = MediaSubType.ARGB32;
                pSampleGrabber_pmt.formatType = FormatType.VideoInfo;
                pSampleGrabber_pmt.fixedSizeSamples = true;
                pSampleGrabber_pmt.formatSize = 88;
                pSampleGrabber_pmt.sampleSize = 3686400;
                pSampleGrabber_pmt.temporalCompression = false;
                VideoInfoHeader pSampleGrabber_format = new VideoInfoHeader();
                pSampleGrabber_format.SrcRect = new DsRect();
                pSampleGrabber_format.TargetRect = new DsRect();
                pSampleGrabber_format.BitRate = 884736885;
                pSampleGrabber_format.AvgTimePerFrame = 333333;
                pSampleGrabber_format.BmiHeader = new BitmapInfoHeader();
                pSampleGrabber_format.BmiHeader.Size = 40;
                pSampleGrabber_format.BmiHeader.Width = 1280;
                pSampleGrabber_format.BmiHeader.Height = 720;
                pSampleGrabber_format.BmiHeader.Planes = 1;
                pSampleGrabber_format.BmiHeader.BitCount = 32;

                //pSampleGrabber_format.BmiHeader.Compression = 1196444237;
                pSampleGrabber_format.BmiHeader.ImageSize = 3686400;
                pSampleGrabber_pmt.formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(pSampleGrabber_format));
                Marshal.StructureToPtr(pSampleGrabber_format, pSampleGrabber_pmt.formatPtr, false);
                hr = ((ISampleGrabber)sampleGrabber).SetMediaType(pSampleGrabber_pmt);
                DsError.ThrowExceptionForHR(hr);



                //connect MJPG dec and SampleGrabber
                //hr = pGraph.ConnectDirect(GetPin(pMJPGDecompressor, "XForm Out"), GetPin(pSampleGrabber, "Input"), null);
                hr = pGraph.ConnectDirect(DsFindPin.ByName(pColorSpaceConverter, "XForm Out"), DsFindPin.ByName(sampleGrabber, "Input"), null);
                DsError.ThrowExceptionForHR(hr);

                //set callback
                hr = ((ISampleGrabber)sampleGrabber).SetCallback(this, 1);
                DsError.ThrowExceptionForHR(hr);
            }
            finally
            {
                // Clean this mess up!
            }
        }

        public void StartCapture()
        {
            IMediaControl mediaControl = (IMediaControl)pGraph;
            IMediaEvent mediaEvent = (IMediaEvent)pGraph;
            int hr = mediaControl.Run();
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
 
            lock (lockObject)
            {
                countElapsed++;
                if (countElapsed == 10)
                {

                    elapsed = (DateTime.Now.Ticks - elapsed);
                    TimeSpan elapsedSpan = new TimeSpan(elapsed);
                    _fps = (float)10f / ((float)elapsedSpan.TotalSeconds);
                    _fps = (int)_fps;
                    elapsed = DateTime.Now.Ticks;
                    countElapsed = 0;
                }

                //byte[] pixelData = new byte[BufferLen];
                //byte[] readBackData = new byte[BufferLen];
                //Marshal.Copy(pBuffer, pixelData, 0, BufferLen);
                /*
                if (writeOnce)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\Pixeldata.txt", true))
                    {
                        for (int i = 0; i < BufferLen; i++)
                            file.WriteLine("pixeldata[" + i.ToString() + "] = " + pixelData[i]);
                    }
                    writeOnce = false;
                }
                */
                //currentBitmap = new System.Drawing.Bitmap(1280, 720, 1280 * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, pBuffer);
                //(bitmapWindow.getPixBox()).Image = currentBitmap;

                /*
                System.IO.MemoryStream mstream = new System.IO.MemoryStream();
                currentBitmap.Save(mstream, System.Drawing.Imaging.ImageFormat.Bmp);
                mstream.Position = 0;
                texture = Texture.FromStream(d3dDevice, mstream);
                */
                
                /*
                var data = texture.LockRectangle(0, LockFlags.None);
                data.Data.Write(pixelData, 0, BufferLen);
                texture.UnlockRectangle(0);
                */

                unsafe
                {
                    Surface privateSurface = texture.GetSurfaceLevel(0);
                    DataRectangle graphicsStream = privateSurface.LockRectangle(LockFlags.None);

                    int pos = 720 * graphicsStream.Pitch;
                    int stride = 1280 * 4;
                    byte* data = (byte*)pBuffer.ToPointer();
                    for (int i = 0; i < 720; i++)
                    {
                        graphicsStream.Data.WriteRange((IntPtr)data, stride);
                        pos -= graphicsStream.Pitch;
                        graphicsStream.Data.Position = pos;
                        data += stride;
                    }

                    //graphicsStream.Pitch = 1280 * 4;
                    //graphicsStream.Data.WriteRange(pBuffer, 1280 * 720 * 4);
                    privateSurface.UnlockRectangle();

                    
                }
                //DataRectangle graphicsStream2 = privateSurface.LockRectangle(LockFlags.None);
                //graphicsStream2.Data.Read(readBackData, 0, 1280 * 720 * 4);
                //privateSurface.UnlockRectangle();
                //d3dDevice.SetTexture(0, texture);

                //FrameCompleteEventArgs arg = new FrameCompleteEventArgs();
                //arg.buffer = pBuffer;
                //arg.length = BufferLen;
                //FrameComplete(ref currentBitmap);
                FrameComplete(ref texture, _fps);
                return 0;
            }
        }

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            return 0;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class FrameCompleteEventArgs : EventArgs
    {
        public IntPtr buffer;
        public int length;
    }
}
