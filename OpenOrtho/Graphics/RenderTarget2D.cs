using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace OpenOrtho.Graphics
{
    public class RenderTarget2D : GraphicsResource
    {
        int handle;
        Texture2D texture;

        public RenderTarget2D(int width, int height)
        {
            texture = new Texture2D(width, height);
            GL.TexImage2D(TextureTarget.Texture2D, 0, texture.Format, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }

        internal int Handle
        {
            get { return this.handle; }
        }

        public Texture2D Texture
        {
            get { return this.texture; }
        }

        public void Begin()
        {
            GL.GenFramebuffers(1, out handle);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, handle);
            GL.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, texture.Handle, 0);

            GL.PushAttrib(AttribMask.ViewportBit);
            GL.Viewport(0, 0, texture.Width, texture.Height);
        }

        public void End()
        {
            GL.PopAttrib();

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
        }

        protected override void Unload()
        {
            GL.DeleteFramebuffers(1, ref handle);
        }
    }
}
