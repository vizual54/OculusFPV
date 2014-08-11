using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D9;
using SlimDX;
using System.Windows.Forms;
using System.Drawing;
using DirectShowLib;

namespace SlimDXTest
{
    class Renderer : IDisposable
    {
        private object m_csLock = new object();
        private Device m_Device = null;
        private Control m_Control = null;
        private Surface m_RenderTarget = null;
        private Texture leftTex = null;
        private Texture rightTex = null;
        private Texture tex = null;
        private Effect m_distortionEffect = null;
        private bool m_swap = false;
        private float m_pupilOffset = 0.0f;
        private float m_lensCenterOffset = 0.0f;
        private float m_scale = 1.0f;
        private Surface leftImage;
        private Surface rightImage;
        private bool fullscreen = false;
        private String renderMethod = "";
        private SlimDX.Direct3D9.Font m_font = null;
        private long elapsed = 0;
        private int countElapsed = 0;
        private float fps = 0f;
        private float c1fps = 0f;
        private float c2fps = 0f;

        public Device D3DDevice
        {
            get {return m_Device; }
        }

        public Renderer(Control _control, bool _fullscreen, String _renderMethod)
        {
            fullscreen = _fullscreen;
            m_Control = _control;
            renderMethod = _renderMethod;
            if (m_Device == null)
            {
                Direct3DEx _d3d = new Direct3DEx();
                PresentParameters _parameters = new PresentParameters();
                if (fullscreen)
                {
                    _parameters.BackBufferWidth = 1920;// m_Control.Width;
                    _parameters.BackBufferHeight = 1080;// m_Control.Height;
                }
                else
                {
                    _parameters.BackBufferWidth = 1280;// m_Control.Width;
                    _parameters.BackBufferHeight = 800;// m_Control.Height;
                }

                if (fullscreen)
                {
                    _parameters.BackBufferFormat = Format.X8R8G8B8;
                    _parameters.BackBufferCount = 1;
                    _parameters.Multisample = MultisampleType.None;
                    _parameters.SwapEffect = SwapEffect.Flip;
                    //_parameters.PresentationInterval = PresentInterval.Default;
                    _parameters.PresentationInterval = PresentInterval.Immediate;
                    _parameters.Windowed = false;
                    _parameters.DeviceWindowHandle = m_Control.Parent.Handle;
                    _parameters.SwapEffect = SwapEffect.Flip;
                    _parameters.FullScreenRefreshRateInHertz = 0;
                    _parameters.PresentFlags = PresentFlags.None | PresentFlags.DiscardDepthStencil;
                    _parameters.AutoDepthStencilFormat = Format.D24X8;
                    _parameters.EnableAutoDepthStencil = true;
                    m_Device = new Device(_d3d, 0, DeviceType.Hardware, m_Control.Parent.Handle, CreateFlags.Multithreaded | CreateFlags.HardwareVertexProcessing, _parameters);
                }
                else
                {
                    
                    DisplayMode _mode = _d3d.GetAdapterDisplayMode(0);
                    _parameters.BackBufferFormat = _mode.Format;
                    _parameters.BackBufferCount = 1;
                    _parameters.Multisample = MultisampleType.None;
                    _parameters.SwapEffect = SwapEffect.Flip;
                    _parameters.PresentationInterval = PresentInterval.Default;
                    _parameters.Windowed = true;
                    _parameters.DeviceWindowHandle = m_Control.Handle;
                    _parameters.PresentationInterval = PresentInterval.Immediate;
                    _parameters.PresentFlags = PresentFlags.DeviceClip | PresentFlags.Video;
                    m_Device = new DeviceEx(_d3d, 0, DeviceType.Hardware, m_Control.Handle, CreateFlags.Multithreaded | CreateFlags.HardwareVertexProcessing, _parameters);
                }
            }

            m_RenderTarget = m_Device.GetRenderTarget(0);

            leftTex = new Texture(m_Device, 1280, 720, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            //leftTex = Texture.FromFile(m_Device, ".\\Images\\checkerboard.jpg", D3DX.DefaultNonPowerOf2, D3DX.DefaultNonPowerOf2, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default, Filter.None, Filter.None, 0);
            //leftTex = Texture.FromFile(m_Device, ".\\Images\\LeftEye.jpg", D3DX.DefaultNonPowerOf2, D3DX.DefaultNonPowerOf2, 1, Usage.RenderTarget, Format.Unknown, Pool.Default, Filter.None, Filter.None, 0);
            SurfaceDescription desc = leftTex.GetLevelDescription(0);
            //leftTex = Texture.FromFile(m_Device, ".\\Images\\CheckerboardLeft.jpg", Usage.RenderTarget, Pool.Default);
            
            rightTex = new Texture(m_Device, 1280, 720, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            //rightTex = Texture.FromFile(m_Device, ".\\Images\\RightEye.jpg", D3DX.DefaultNonPowerOf2, D3DX.DefaultNonPowerOf2, 1, Usage.RenderTarget, Format.Unknown, Pool.Default, Filter.None, Filter.None, 0);
            //rightTex = Texture.FromFile(m_Device, ".\\Images\\CheckerboardRight.jpg", Usage.RenderTarget, Pool.Default);

            m_font = new SlimDX.Direct3D9.Font(m_Device, new System.Drawing.Font(FontFamily.GenericSansSerif, 24.0f));

            switch (renderMethod)
            {  
                case "None" :
                    m_distortionEffect = Effect.FromFile(m_Device, "PlainPS.fx", ShaderFlags.None);
                    break;
                case "Oculus":
                    m_distortionEffect = Effect.FromFile(m_Device, "OculusRiftPS.fx", ShaderFlags.None);
                    break;
                case "3DTV":
                    m_distortionEffect = Effect.FromFile(m_Device, "PlainPS.fx", ShaderFlags.None);
                    break;
                default:
                    throw new Exception("No render method found.");
                    break;
            }
            

            //leftImage = Surface.CreateOffscreenPlain(m_Device, 640, 720, Format.A8R8G8B8, Pool.Default);
            //Surface.FromFile(leftImage, ".\\Images\\checkerboardRedGreen.jpg", Filter.None, 0);

            //rightImage = Surface.CreateOffscreenPlain(m_Device, 640, 720, Format.A8R8G8B8, Pool.Default);
            //Surface.FromFile(rightImage, ".\\Images\\checkerboardRedGreen.jpg", Filter.None, 0);
            //tex = new Texture(m_Device, 1280, 720, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
            
            //tex = new Texture(m_Device, 1280, 720, 1, Usage.RenderTarget, Format.R8G8B8, Pool.Default);
        }

        public void OnLeftFrameComplete(ref Texture texture, float cameraFps)
        {
            lock (m_csLock)
            {
                Surface surf = leftTex.GetSurfaceLevel(0);
                //m_Device.StretchRectangle(texture.GetSurfaceLevel(0), surf, TextureFilter.None);
                leftTex = texture;
                c1fps = cameraFps;
                //m_Device.UpdateSurface(texture.GetSurfaceLevel(0), surf);
                
                //((PictureBox)m_Control).Image = bitmap;
            }
        }

        public void OnRightFrameComplete(ref Texture texture, float cameraFps)
        {
            lock (m_csLock)
            {
                Surface surf = rightTex.GetSurfaceLevel(0);
                //m_Device.StretchRectangle(texture.GetSurfaceLevel(0), surf, TextureFilter.None);
                rightTex = texture;
                c2fps = cameraFps;
            }
        }

        public void Render()
        {
            lock (m_csLock)
            {
                countElapsed++;
                if (countElapsed == 50)
                {

                    elapsed = (DateTime.Now.Ticks - elapsed);
                    TimeSpan elapsedSpan = new TimeSpan(elapsed);
                    fps = (float)50f / ((float)elapsedSpan.TotalSeconds);
                    fps = (int)fps;
                    elapsed = DateTime.Now.Ticks;
                    countElapsed = 0;
                }

                m_Device.SetRenderTarget(0, m_RenderTarget);
                m_Device.Clear(ClearFlags.Target, Color.Blue, 1.0f, 0);
                Surface _backbuffer = m_Device.GetBackBuffer(0, 0);

                m_Device.BeginScene();

                float w = _backbuffer.Description.Width;
                float h = _backbuffer.Description.Height;

                //Surface tex = m_tex.GetSurfaceLevel(0);
                //m_Device.StretchRectangle(leftImage, tex, TextureFilter.Linear);
                //tex.Dispose();

                m_Device.SetRenderTarget(0, m_RenderTarget);

                float a = 2.0f * h / w;

                float aspectRatioValue = 640.0f / 800.0f;

                //Vertex[] lverts = { new Vertex(0, 0, 1, 0.5f, -1, -a), new Vertex(w / 2, 0, 1, 0.5f, 1, -a), new Vertex(0, h, 1, 0.5f, -1, a), new Vertex(w / 2, h, 1, 0.5f, 1, a) };
                //Vertex[] rverts = { new Vertex(w / 2, 0, 1, 0.5f, -1, -a), new Vertex(w, 0, 1, 0.5f, 1, -a), new Vertex(w / 2, h, 1, 0.5f, -1, a), new Vertex(w, h, 1, 0.5f, 1, a) };
                float z = 1.0f;

                Vertex[] lverts = null;
                Vertex[] rverts = null;
                //plain
                if (renderMethod == "None")
                {
                    lverts = new Vertex[] { new Vertex(0, 0, z, 0.5f, 0, 0), new Vertex(w, 0, z, 0.5f, 1, 0), new Vertex(0, h, z, 0.5f, 0, 1), new Vertex(w, h, z, 0.5f, 1, 1) };
                }
                else
                {
                    lverts = new Vertex[] { new Vertex(0, 0, z, 0.5f, 0, 0), new Vertex(w / 2, 0, z, 0.5f, 1, 0), new Vertex(0, h, z, 0.5f, 0, 1), new Vertex(w / 2, h, z, 0.5f, 1, 1) };
                    rverts = new Vertex[] { new Vertex(w / 2, 0, z, 0.5f, 0, 0), new Vertex(w, 0, z, 0.5f, 1, 0), new Vertex(w / 2, h, z, 0.5f, 0, 1), new Vertex(w, h, z, 0.5f, 1, 1) };
                }
                
                //Vertex[] vertsices = { new Vertex(0, 0, z, 0.5f, 0, 0), new Vertex(w, 0, z, 0.5f, 1, 0), new Vertex(0, h, z, 0.5f, 0, 1), new Vertex(w, h, z, 0.5f, 1, 1) };
                m_Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Texture1;
                m_Device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.SelectArg1);
                m_Device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
                m_Device.SetRenderState(RenderState.CullMode, Cull.None);
                m_Device.SetRenderState(RenderState.ZFunc, Compare.Always);

                //m_Device.SetTexture(0, leftTex);
                //m_Device.SetTexture(1, rightTex);
                m_Device.SetSamplerState(0, SamplerState.BorderColor, 0x00000000);
                m_Device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Border);
                m_Device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Border);
                /*
                EffectHandle hmdWarpParam = m_distortionEffect.GetParameter(null, "HmdWarpParam");
                EffectHandle texCoordOffset = m_distortionEffect.GetParameter(null, "TexCoordOffset");
                EffectHandle tcScaleMat = m_distortionEffect.GetParameter(null, "TCScaleMat");
                EffectHandle aspectRatio = m_distortionEffect.GetParameter(null, "AspectRatio");
                EffectHandle scale = m_distortionEffect.GetParameter(null, "Scale");
                EffectHandle pupilOffset = m_distortionEffect.GetParameter(null, "PupilOffset");
                */
                EffectHandle hmdWarpParam = null;
                EffectHandle lensCenterOffset = null;
                EffectHandle aspectRatio = null;
                EffectHandle scale = null;
                if (renderMethod == "Oculus")
                {
                    hmdWarpParam = m_distortionEffect.GetParameter(null, "HmdWarpParam");
                    lensCenterOffset = m_distortionEffect.GetParameter(null, "LensCenterOffset");
                    aspectRatio = m_distortionEffect.GetParameter(null, "AspectRatio");
                    scale = m_distortionEffect.GetParameter(null, "Scale");
                }
                
                //m_font.DrawString(null, "Hello", 200+640, 200, Color.White);
                for (int j = 0; j < 2; j++)
                {
                    Vertex[] verts;
                    
                    if (j == 0)
                    {
                        verts = lverts;
                        m_Device.SetTexture(0, leftTex);
                        if (renderMethod == "None")
                            j++;
                    }
                    else
                    {
                        verts = rverts;
                        m_Device.SetTexture(0, leftTex);
                        //m_Device.SetTexture(0, rightTex);
                    }
                    
                    //verts = vertsices;
                    //float texCoordOffsetValue = ((j == 0) ^ !!m_swap) ? 0.0f : 0.5f;
                    //Vector4 scaleMat = (j == 0) ? (new Vector4(0.0f, -1.0f, 1.0f, 0.0f)) : (new Vector4(0.0f, 1.0f, -1.0f, 0.0f));
                    Vector4 scaleMat = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    float pupilOffsetValue = (j == 0) ? m_pupilOffset : -m_pupilOffset;
                    float lensCenterOffsetValue = (j == 0) ? -m_lensCenterOffset : m_lensCenterOffset;
                    int numPasses = m_distortionEffect.Begin();
                    for (int i = 0; i < numPasses; i++)
                    {
                        m_distortionEffect.BeginPass(i);
                        /*
                        m_distortionEffect.SetValue(hmdWarpParam, new Vector4(1.0f, 0.22f, 0.24f, 0.0f));
                        m_distortionEffect.SetValue(tcScaleMat, scaleMat);
                        m_distortionEffect.SetValue(texCoordOffset, new Vector2(texCoordOffsetValue, 0.0f));
                        m_distortionEffect.SetValue(scale, new Vector2(0.25f / m_scale, 0.5f / m_scale));
                        m_distortionEffect.SetValue(aspectRatio, aspectRatioValue);
                        m_distortionEffect.SetValue(pupilOffset, pupilOffsetValue);
                        */
                        if (renderMethod == "Oculus")
                        {
                            m_distortionEffect.SetValue(hmdWarpParam, new Vector4(1.0f, 0.22f, 0.24f, 0.0f));
                            m_distortionEffect.SetValue(lensCenterOffset, lensCenterOffsetValue);
                            m_distortionEffect.SetValue(scale, new Vector2(m_scale, m_scale));
                            m_distortionEffect.SetValue(aspectRatio, aspectRatioValue);
                        }
                        m_Device.DrawUserPrimitives<Vertex>(PrimitiveType.TriangleStrip, 2, verts);
                        m_font.DrawString(null, "Scale: " + m_scale.ToString(), 0, 0, Color.Green);
                        m_font.DrawString(null, "Lens Offset: " + m_lensCenterOffset.ToString(), 0, 50, Color.Green);
                        m_font.DrawString(null, "FPS: " + fps.ToString(), 0, 100, Color.Green);
                        m_font.DrawString(null, "Left cam FPS: " + c1fps.ToString(), 0, 150, Color.Green);
                        m_font.DrawString(null, "Right cam FPS: " + c2fps.ToString(), 0, 200, Color.Green);
                        m_distortionEffect.EndPass();
                    }
                    m_distortionEffect.End();

                }

                m_Device.EndScene();
                m_Device.Present();
            }
        }

        public void IncreaseScale()
        {
            lock (m_csLock) { m_scale *= 1.05f; }
        }

        public void DecreaseScale()
        {
            lock (m_csLock) { m_scale /= 1.05f; }
        }

        public void MoveLensLeft()
        {
            lock (m_csLock) { m_lensCenterOffset += 0.025f; }
        }

        public void MoveLensRight()
        {
            lock (m_csLock) { m_lensCenterOffset -= 0.025f; }
        }

        public void Dispose()
        {
            lock (m_csLock)
            {
                if (leftTex != null)
                {
                    leftTex.Dispose();
                    leftTex = null;
                }
                    
                if (rightTex != null)
                {
                    rightTex.Dispose();
                    rightTex = null;
                }

                if (m_Device != null)
                {
                    m_Device.Dispose();
                    m_Device = null;
                }
                    


            }
            
        }

        
    }

    public struct Vertex
    {
        public Vertex(float _x, float _y, float _z, float _rhw, float _u, float _v) { x = _x; y = _y; z = _z; rhw = _rhw; u = _u; v = _v; }
        float x, y, z, rhw;
        float u, v;
    };
}
