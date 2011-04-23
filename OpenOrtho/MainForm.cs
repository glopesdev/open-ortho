using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenOrtho.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using OpenTK.Graphics;

namespace OpenOrtho
{
    public partial class MainForm : Form
    {
        OrthoProject project;

        bool loaded;
        float scale;
        Camera2D camera;
        Texture2D background;
        SpriteBatch spriteBatch;

        bool keyUp;
        bool keyDown;
        bool keyLeft;
        bool keyRight;
        bool keyReset;
        bool keyZoomIn;
        bool keyZoomOut;

        Stopwatch clock;
        AboutBox about;

        public MainForm()
        {
            InitializeComponent();
            about = new AboutBox();
        }

        void LoadProject()
        {
            if (background != null) background.Dispose();

            background = Texture2D.FromFile(project.Radiograph);
            UpdateScale();
        }

        void UpdateScale()
        {
            if (background != null)
            {
                scale = (float)glControl.Height / (float)background.Height;
                spriteBatch.PixelsPerMeter = project.PixelsPerMeter * scale;
            }
        }

        void UpdateModel(float time)
        {
            camera.Zoom += time * camera.Zoom * (keyZoomIn ? 1f : keyZoomOut ? -1f : 0);
            camera.Position += time / scale * new Vector2(
                keyLeft ? -2f : keyRight ? 2f : 0,
                keyDown ? -2f : keyUp ? 2f : 0);

            if (keyReset)
            {
                camera.Zoom = 1;
                camera.Position = Vector2.Zero;
            }
        }

        void RenderModel()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            spriteBatch.Begin(camera.GetViewMatrix());

            if (background != null)
            {
                var renderWidth = background.Width * scale;
                spriteBatch.Draw(background, new RectangleF(-renderWidth / 2f, -glControl.Height / 2f, renderWidth, glControl.Height));
            }

            if (project != null && project.Analysis != null)
            {
                spriteBatch.DrawPoints(project.Analysis.Points, Color4.Red, 3);
            }

            spriteBatch.End();
            glControl.SwapBuffers();
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            project = new OrthoProject();
            project.Analysis = new CephalometricAnalysis();

            clock = new Stopwatch();
            clock.Start();

            GL.ClearColor(Color.Black);

            scale = 1;
            camera = new Camera2D();
            spriteBatch = new SpriteBatch(glControl.Width, glControl.Height);

            Application.Idle += new EventHandler(Application_Idle);
            loaded = true;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl.IsIdle)
            {
                clock.Stop();
                var elapsedTime = clock.Elapsed.TotalMilliseconds;
                clock.Reset();
                clock.Start();

                UpdateModel((float)(elapsedTime / 1000.0));
                RenderModel();
            }
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            if (!loaded) return;

            UpdateScale();
            spriteBatch.SetDimensions(glControl.Width, glControl.Height);
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;
            RenderModel();
        }

        private void openRadiograph_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog(this) == DialogResult.OK)
            {
                project.Radiograph = openImageDialog.FileName;
                LoadProject();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: keyUp = true; break;
                case Keys.S: keyDown = true; break;
                case Keys.A: keyLeft = true; break;
                case Keys.D: keyRight = true; break;
                case Keys.R: keyReset = true; break;
                case Keys.I: keyZoomIn = true; break;
                case Keys.O: keyZoomOut = true; break;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: keyUp = false; break;
                case Keys.S: keyDown = false; break;
                case Keys.A: keyLeft = false; break;
                case Keys.D: keyRight = false; break;
                case Keys.R: keyReset = false; break;
                case Keys.I: keyZoomIn = false; break;
                case Keys.O: keyZoomOut = false; break;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(saveProjectDialog.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else
            {
                using (var writer = XmlWriter.Create(saveProjectDialog.FileName, new XmlWriterSettings { Indent = true }))
                {
                    var serializer = new XmlSerializer(typeof(OrthoProject));
                    serializer.Serialize(writer, project);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveProjectDialog.ShowDialog(this) == DialogResult.OK)
            {
                saveToolStripMenuItem_Click(this, e);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openProjectDialog.ShowDialog(this) == DialogResult.OK)
            {
                using (var reader = XmlReader.Create(openProjectDialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof(OrthoProject));
                    project = (OrthoProject)serializer.Deserialize(reader);
                    LoadProject();
                }
            }
        }

        private void glControl_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
