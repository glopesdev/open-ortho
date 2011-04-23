using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Text;
using OpenTK;

namespace OpenOrtho.Graphics
{
    public class SpriteFont : GraphicsResource
    {
        Texture2D texture;
        FontGlyph[] glyphs;
        int rangeStart;

        private SpriteFont(Texture2D fontTexture, FontGlyph[] fontGlyphs, int fontRangeStart)
        {
            texture = fontTexture;
            glyphs = fontGlyphs;
            rangeStart = fontRangeStart;
        }

        internal int Handle
        {
            get { return texture.Handle; }
        }

        internal FontGlyph[] Glyphs
        {
            get { return glyphs; }
        }

        internal int RangeStart
        {
            get { return rangeStart; }
        }

        public int Spacing { get; internal set; }

        public int LineSpacing { get; internal set; }

        public Vector2 MeasureString(string text)
        {
            float sx = 0;
            float sy = 0;
            float sline = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var character = text[i];
                if (character == '\n')
                {
                    sx = Math.Max(sx, sline);
                    sline = 0;
                    sy += LineSpacing;
                }
                else
                {
                    sline += Spacing;
                }
            }

            sx = Math.Max(sx, sline);
            sy += sx > 0 ? LineSpacing : 0;
            return new Vector2(sx, sy);
        }

        public static SpriteFont FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var rangeStart = reader.ReadInt32();
                var glyphWidth = reader.ReadInt32();
                var glyphHeight = reader.ReadInt32();
                var glyphCount = reader.ReadInt32();
                var glyphs = new FontGlyph[glyphCount];
                for (int i = 0; i < glyphs.Length; i++)
                {
                    glyphs[i].X = reader.ReadSingle();
                    glyphs[i].Y = reader.ReadSingle();
                    glyphs[i].Width = reader.ReadSingle();
                    glyphs[i].Height = reader.ReadSingle();
                }

                var textureBytes = reader.ReadInt32();
                var textureData = reader.ReadBytes(textureBytes);
                using (var textureStream = new MemoryStream(textureData))
                {
                    var texture = Texture2D.FromStream(textureStream);
                    var font = new SpriteFont(texture, glyphs, rangeStart);
                    font.Spacing = glyphWidth;
                    font.LineSpacing = glyphHeight;
                    return font;
                }
            }
        }

        public static SpriteFont FromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return FromStream(stream);
            }
        }

        protected override void Unload()
        {
            texture.Dispose();
        }

        internal struct FontGlyph
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;
        }
    }
}

