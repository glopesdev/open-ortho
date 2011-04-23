using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;

namespace OpenOrtho.Graphics
{
    public class SpriteBatch
    {
        Matrix4 projection;
        Matrix4 view;
        Matrix4 translation;
        Matrix4 rotationZ;
        Matrix4 scale2D;
        Matrix4 modelView;

        public SpriteBatch(float width, float height)
        {
            SetDimensions(width, height);
            PixelsPerMeter = 100;
        }

        public void SetDimensions(float width, float height)
        {
            Matrix4.CreateOrthographic(width, height, 0, 1, out projection);
        }

        public float PixelsPerMeter { get; set; }

        public void Begin()
        {
            Begin(Matrix4.Identity);
        }

        public void Begin(Matrix4 transform)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            view = transform;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        void LoadModelView(ref Vector2 position, float rotation, ref Vector2 scale)
        {
            Matrix4.CreateTranslation(position.X, position.Y, 0, out translation);
            Matrix4.CreateRotationZ(rotation, out rotationZ);
            scale2D = Matrix4.Scale(scale.X, scale.Y, 1);

            Matrix4.Mult(ref rotationZ, ref scale2D, out modelView);
            Matrix4.Mult(ref modelView, ref translation, out modelView);

            Matrix4.Mult(ref modelView, ref view, out modelView);
            modelView.M41 *= PixelsPerMeter;
            modelView.M42 *= PixelsPerMeter;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
        }

        void LoadView()
        {
            Matrix4.Mult(ref Matrix4.Identity, ref view, out modelView);
            modelView.M41 *= PixelsPerMeter;
            modelView.M42 *= PixelsPerMeter;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
        }

        public void Draw(Texture2D texture, Vector2 position, float rotation, Vector2 scale)
        {
            LoadModelView(ref position, rotation, ref scale);

            var halfWidth = texture.Width / 2f;
            var halfHeight = texture.Height / 2f;
            Draw(texture, new RectangleF(-halfWidth, -halfHeight, texture.Width, texture.Height));
        }

        public void DrawPoints(IEnumerable<Vector2> points, Color4 color, float size)
        {
            LoadView();

            GL.Color4(color);
            GL.PointSize(size);
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(BeginMode.Points);

            foreach (var point in points)
            {
                GL.Vertex2(point * PixelsPerMeter);
            }

            GL.End();
        }

        public void Draw(Texture2D texture, RectangleF rectangle)
        {
            LoadView();
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

            GL.Color4(Color4.White);
            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0); GL.Vertex2(rectangle.X, rectangle.Y);
            GL.TexCoord2(1, 0); GL.Vertex2(rectangle.X + rectangle.Width, rectangle.Y);
            GL.TexCoord2(1, 1); GL.Vertex2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
            GL.TexCoord2(0, 1); GL.Vertex2(rectangle.X, rectangle.Y + rectangle.Height);

            GL.End();
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, float rotation, Vector2 scale, Color4 color)
        {
            LoadModelView(ref position, rotation, ref scale);

            var glyphs = font.Glyphs;
            var spacing = font.Spacing;
            var lineSpacing = font.LineSpacing;
            GL.BindTexture(TextureTarget.Texture2D, font.Handle);

            GL.Color4(color);
            GL.Begin(BeginMode.Quads);

            float x = 0;
            float y = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var character = text[i];
                if (character == '\n')
                {
                    x = 0;
                    y -= lineSpacing;
                }
                else
                {
                    var glyphIndex = character - font.RangeStart;
                    var gx = glyphs[glyphIndex].X;
                    var gy = glyphs[glyphIndex].Y;
                    var gwidth = glyphs[glyphIndex].Width;
                    var gheight = glyphs[glyphIndex].Height;

                    GL.TexCoord2(gx, gy - gheight); GL.Vertex2(x, y - lineSpacing);
                    GL.TexCoord2(gx + gwidth, gy - gheight); GL.Vertex2(x + spacing, y - lineSpacing);
                    GL.TexCoord2(gx + gwidth, gy); GL.Vertex2(x + spacing, y);
                    GL.TexCoord2(gx, gy); GL.Vertex2(x, y);

                    x += spacing;
                }
            }

            GL.End();
        }

        public void End()
        {
        }
    }
}

