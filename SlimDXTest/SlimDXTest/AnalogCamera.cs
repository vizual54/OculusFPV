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
    class AnalogCamera : ICamera, ISampleGrabberCB, IDisposable
    {
        public event FrameCompleteEventHandler FrameComplete;

        private object lockObject = new object();
        private IMediaControl mediaControl = null;
        private IMediaEvent mediaEvent = null;
        private IGraphBuilder pGraph = null;
        private int videoWidth = 0;
        private int videoHeight = 0;
        private Device d3dDevice;
        public Texture texture;
        private long elapsed = 0;
        private int countElapsed = 0;
        private float _fps = 0f;

        public AnalogCamera(DsDevice device, Device d3dDevice)
        {
            this.d3dDevice = d3dDevice;
            texture = new Texture(d3dDevice, 720, 576, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public float FPS
        {
            get { lock (lockObject) { return _fps; } }
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
                pmt.subType = MediaSubType.YUY2;
                pmt.formatType = FormatType.VideoInfo;
                pmt.fixedSizeSamples = true;
                pmt.formatSize = 88;
                pmt.sampleSize = 829440;
                pmt.temporalCompression = false;
                VideoInfoHeader format = new VideoInfoHeader();
                format.SrcRect = new DsRect();
                format.TargetRect = new DsRect();
                format.BitRate = 20736000;
                format.AvgTimePerFrame = 400000;
                format.BmiHeader = new BitmapInfoHeader();
                format.BmiHeader.Size = 40;
                format.BmiHeader.Width = 720;
                format.BmiHeader.Height = 576;
                format.BmiHeader.Planes = 1;
                format.BmiHeader.BitCount = 24;
                format.BmiHeader.Compression = 844715353;
                format.BmiHeader.ImageSize = 827440;
                pmt.formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(format));
                Marshal.StructureToPtr(format, pmt.formatPtr, false);
                hr = ((IAMStreamConfig)DsFindPin.ByCategory(camera, PinCategory.Capture, 0)).SetFormat(pmt);
                //hr = ((IAMStreamConfig)GetPin(pUSB20Camera, "Capture")).SetFormat(pmt);
                DsUtils.FreeAMMediaType(pmt);
                DsError.ThrowExceptionForHR(hr);

                IAMCrossbar crossBar = null;
                object dummy;
                hr = pBuilder.FindInterface(PinCategory.Capture, MediaType.Video, camera, typeof(IAMCrossbar).GUID, out dummy);
                if( hr >=0)
                {
                    crossBar = (IAMCrossbar)dummy;
                    int oPin, iPin;
                    int ovLink, ivLink;
                    ovLink = ivLink = 0;
                    crossBar.get_PinCounts(out oPin, out iPin);
                    int pIdxRel;
                    PhysicalConnectorType physicalConType;
                    for (int i = 0; i < iPin; i++)
                    {
                        crossBar.get_CrossbarPinInfo(true, i, out pIdxRel, out physicalConType);
                        if (physicalConType == PhysicalConnectorType.Video_Composite)
                            ivLink = i;
                    }
                    for (int i = 0; i < oPin; i++)
                    {
                        crossBar.get_CrossbarPinInfo(false, i, out pIdxRel, out physicalConType);
                        if (physicalConType == PhysicalConnectorType.Video_VideoDecoder)
                            ovLink = i;
                    }

                    try
                    {
                        crossBar.Route(ovLink, ivLink);
                    }
                    catch
                    {

                        throw new Exception("Failed to get IAMCrossbar");
                    }
                }

                //add AVI Decompressor
                IBaseFilter pAVIDecompressor = (IBaseFilter)new AVIDec();
                hr = pGraph.AddFilter(pAVIDecompressor, "AVI Decompressor");
                
                //add color space converter
                IBaseFilter pColorSpaceConverter = (IBaseFilter)new Colour();
                hr = pGraph.AddFilter(pColorSpaceConverter, "Color space converter");
                DsError.ThrowExceptionForHR(hr);

                // Connect camera and AVI Decomp
                hr = pGraph.ConnectDirect(DsFindPin.ByCategory(camera, PinCategory.Capture, 0), DsFindPin.ByName(pAVIDecompressor, "XForm In"), null);
                DsError.ThrowExceptionForHR(hr);

                // Connect AVI Decomp and color space converter
                hr = pGraph.ConnectDirect(DsFindPin.ByName(pAVIDecompressor, "XForm Out"), DsFindPin.ByName(pColorSpaceConverter, "Input"), null);
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
                pSampleGrabber_pmt.sampleSize = 1658880;
                pSampleGrabber_pmt.temporalCompression = false;
                VideoInfoHeader pSampleGrabber_format = new VideoInfoHeader();
                pSampleGrabber_format.SrcRect = new DsRect();
                pSampleGrabber_format.SrcRect.right = 720;
                pSampleGrabber_format.SrcRect.bottom = 576;
                pSampleGrabber_format.TargetRect = new DsRect();
                pSampleGrabber_format.TargetRect.right = 720;
                pSampleGrabber_format.TargetRect.bottom = 576;
                pSampleGrabber_format.BitRate = 331776000;
                pSampleGrabber_format.AvgTimePerFrame = 400000;
                pSampleGrabber_format.BmiHeader = new BitmapInfoHeader();
                pSampleGrabber_format.BmiHeader.Size = 40;
                pSampleGrabber_format.BmiHeader.Width = 720;
                pSampleGrabber_format.BmiHeader.Height = 576;
                pSampleGrabber_format.BmiHeader.Planes = 1;
                pSampleGrabber_format.BmiHeader.BitCount = 32;
                pSampleGrabber_format.BmiHeader.ImageSize = 1658880;

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
            if (hr != 0)
                throw new Exception();
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

                unsafe
                {
                    Surface privateSurface = texture.GetSurfaceLevel(0);
                    DataRectangle graphicsStream = privateSurface.LockRectangle(LockFlags.None);

                    int pos = 576 * graphicsStream.Pitch;
                    int stride = 720 * 4;
                    byte* data = (byte*)pBuffer.ToPointer();
                    for (int i = 0; i < 576; i++)
                    {
                        graphicsStream.Data.WriteRange((IntPtr)data, stride);
                        pos -= graphicsStream.Pitch;
                        graphicsStream.Data.Position = pos;
                        data += stride;
                    }

                    privateSurface.UnlockRectangle();


                }
                
                FrameComplete(ref texture, _fps);
                return 0;
            }
        }

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            throw new NotImplementedException();
        }
    }
}
