using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;
using FFMpegWrapper;
using System.Runtime.InteropServices;

namespace SlimDXTest
{
    class StreamingCamera : ICameraStream
    {
        public event FrameCompleteEventHandler FrameComplete;
        public event FrameCompleteEventHandler FrameComplete2;
        private Device d3dDevice;
        public Texture texture;
        public Texture texture2;
        private long elapsed = 0;
        private int countElapsed = 0;
        private float _fps = 0f;
        private long elapsed2 = 0;
        private int countElapsed2 = 0;
        private float _fps2 = 0f;
        private object lockObject = new object();
        private object lockObject2 = new object();
        private VideoStreamDecoder ffWrapper;
        private int width;
        private int height;

        public StreamingCamera(String address, Device d3dDevice, int streams)
        {
            this.d3dDevice = d3dDevice;
            texture = new Texture(d3dDevice, 1280, 720, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
            texture2 = new Texture(d3dDevice, 1280, 720, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
            ffWrapper = new VideoStreamDecoder();
            while(true)
            {
                if(ffWrapper.Initialize(address) > 0)
                {
                    break;
                }
                
            }
            
            //if (result < 0)
            //    throw new Exception("Could not initialize decoder.");
            ffWrapper.frameDone += ffWrapper_frameDone;
            if (streams == 2)
                ffWrapper.frame2Done += ffWrapper_frame2Done;
        }

        public void StartCapture()
        {
            ffWrapper.startDecode();
        }

        public void ffWrapper_frameDone(IntPtr data, int frameWidth, int frameHeight, int frameStride)
        {
            lock (lockObject)
            {
                width = frameWidth;
                height = frameHeight;

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
                    //byte* ptr = (byte*)data.ToPointer();
                    //graphicsStream.Data.WriteRange((IntPtr)ptr, 1280 * 720 * 4 * sizeof(byte));
                    //graphicsStream.Data.DataPointer
                    
                    int pos = 0; // 720 * graphicsStream.Pitch;
                    int stride = frameStride;
                    byte* ptr = (byte*)data.ToPointer();
                    //NativeMethods.CopyMemory(graphicsStream.Data.DataPointer, (IntPtr)ptr, 720 * frameStride * sizeof(byte));
                    
                    for (int i = 0; i < 720; i++)
                    {
                        graphicsStream.Data.WriteRange((IntPtr)ptr, stride);
                        pos += graphicsStream.Pitch;
                        graphicsStream.Data.Position = pos;
                        ptr += stride * sizeof(byte);
                    }
                    
                    privateSurface.UnlockRectangle();
                }

                FrameComplete(ref texture, _fps);
            }
        }

        void ffWrapper_frame2Done(IntPtr data, int frameWidth, int frameHeight, int frameStride)
        {
            lock (lockObject2)
            {
                countElapsed2++;
                if (countElapsed2 == 10)
                {

                    elapsed2 = (DateTime.Now.Ticks - elapsed2);
                    TimeSpan elapsedSpan = new TimeSpan(elapsed2);
                    _fps2 = (float)10f / ((float)elapsedSpan.TotalSeconds);
                    _fps2 = (int)_fps2;
                    elapsed2 = DateTime.Now.Ticks;
                    countElapsed2= 0;
                }

                unsafe
                {
                    Surface privateSurface = texture2.GetSurfaceLevel(0);
                    DataRectangle graphicsStream = privateSurface.LockRectangle(LockFlags.None);
                    int pos = 0; // 720 * graphicsStream.Pitch;
                    int stride = frameStride;
                    byte* ptr = (byte*)data.ToPointer();
                    
                    for (int i = 0; i < 720; i++)
                    {
                        graphicsStream.Data.WriteRange((IntPtr)ptr, stride);
                        pos += graphicsStream.Pitch;
                        graphicsStream.Data.Position = pos;
                        ptr += stride * sizeof(byte);
                    }

                    privateSurface.UnlockRectangle();
                }

                FrameComplete2(ref texture2, _fps2);
            }
        }

        private static class NativeMethods
        {
            [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
            internal static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);
        }

        public float FPS
        {
            get { lock (lockObject) { return _fps; } }
        }

        public float FPS2
        {
            get { lock (lockObject2) { return _fps2; } }
        }

    }
}
