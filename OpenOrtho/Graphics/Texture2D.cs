using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using System.IO;

namespace OpenOrtho.Graphics
{
    public class Texture2D : GraphicsResource
    {
        int handle;

        public Texture2D(int width, int height)
            : this(width, height, PixelInternalFormat.Rgba)
        {
        }

        public Texture2D(int width, int height, PixelInternalFormat format)
        {
            Width = width;
            Height = height;
            Format = format;

            GL.GenTextures(1, out handle);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
        }

        internal int Handle
        {
            get { return handle; }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public PixelInternalFormat Format { get; private set; }

        public void SetData(Color4[] data)
        {
            SetData(data, PixelFormat.Rgba, PixelType.Float);
        }

        public void SetData<T>(T[] data, PixelType type) where T : struct
        {
            SetData(data, (PixelFormat)Format, type);
        }

        public void SetData<T>(T[] data, PixelFormat format, PixelType type) where T : struct
        {
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                GL.BindTexture(TextureTarget.Texture2D, handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, Format, Width, Height, 0,
                              format, type, dataHandle.AddrOfPinnedObject());
            }
            finally { dataHandle.Free(); }
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            return ToBitmap(System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public System.Drawing.Bitmap ToBitmap(System.Drawing.Imaging.PixelFormat format)
        {
            var bitmap = new System.Drawing.Bitmap(Width, Height, format);

            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, handle);
                GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            }
            finally { bitmap.UnlockBits(bitmapData); }

            bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

        public static Texture2D FromBitmap(System.Drawing.Bitmap bitmap)
        {
            return FromBitmap(bitmap, PixelInternalFormat.Rgba);
        }

        public static Texture2D FromBitmap(System.Drawing.Bitmap bitmap, PixelInternalFormat format)
        {
            bitmap.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
            var texture = new Texture2D(bitmap.Width, bitmap.Height, format);

            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, texture.Format, texture.Width, texture.Height, 0,
                              PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            }
            finally { bitmap.UnlockBits(bitmapData); }

            return texture;
        }

        public static Texture2D FromStream(Stream stream)
        {
            return FromStream(stream, PixelInternalFormat.Rgba);
        }

        public static Texture2D FromStream(Stream stream, PixelInternalFormat format)
        {
            using (var bitmap = new System.Drawing.Bitmap(stream))
            {
                return FromBitmap(bitmap);
            }
        }

        public static Texture2D FromFile(string path)
        {
            return FromFile(path, PixelInternalFormat.Rgba);
        }

        public static Texture2D FromFile(string path, PixelInternalFormat format)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return FromStream(stream);
            }
        }

        public static Texture2D CreateRectangle(int width, int height, Color4 color)
        {
            var texture = new Texture2D(width, height);
            var colors = new Color4[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            texture.SetData(colors);
            return texture;
        }

        public static Texture2D CreateCircle(int radius, Color4 color)
        {
            return CreateEllipse(radius, radius, color);
        }

        public static Texture2D CreateEllipse(int xradius, int yradius, Color4 color)
        {
            var width = xradius * 2;
            var height = yradius * 2;
            var texture = new Texture2D(width, height);
            var colors = new Color4[width * height];
            var normalizingWidth = 2f / width;
            var normalizingHeight = 2f / height;
            for (int i = 0; i < colors.Length; i++)
            {
                var x = (float)(i % width) * normalizingWidth - 1;
                var y = (float)(i / width) * normalizingHeight - 1;
                var d = x * x + y * y;
                colors[i] = d < 1 ? color : Color4.Transparent;
            }
            texture.SetData(colors);
            return texture;
        }

        protected override void Unload()
        {
            GL.DeleteTextures(1, ref handle);
        }
    }
}

