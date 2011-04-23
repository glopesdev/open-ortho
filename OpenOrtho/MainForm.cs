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
using System.Reflection;
using OpenOrtho.Analysis;

namespace OpenOrtho
{
    public partial class MainForm : Form
    {
        OrthoProject project;

        bool loaded;
        float scale;
        SpriteFont font;
        Camera2D camera;
        Texture2D background;
        SpriteBatch spriteBatch;

        bool setScale;
        List<Vector2> scaleRefs;

        bool keyUp;
        bool keyDown;
        bool keyLeft;
        bool keyRight;
        bool keyReset;
        bool keyZoomIn;
        bool keyZoomOut;

        Stopwatch clock;
        AboutBox aboutBox;

        public MainForm()
        {
            InitializeComponent();
            aboutBox = new AboutBox();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (background != null) background.Dispose();
            base.OnFormClosed(e);
        }

        void LoadProject()
        {
            if (background != null) background.Dispose();

            background = Texture2D.FromFile(project.Radiograph);
            UpdateScale();
            analysisPropertyGrid.SelectedObject = project;
            analysisPropertyGrid.Enabled = true;
        }

        void UpdateScale()
        {
            if (background != null)
            {
                scale = (float)glControl.Height / (float)background.Height;
                spriteBatch.PixelsPerMeter = project.PixelsPerMillimeter * scale;
            }
        }

        void ResetCamera()
        {
            camera.Zoom = 1;
            camera.Position = Vector2.Zero;
        }

        void UpdateModel(float time)
        {
            camera.Zoom += time * camera.Zoom * (keyZoomIn ? 1f : keyZoomOut ? -1f : 0);
            camera.Position += time / spriteBatch.PixelsPerMeter * new Vector2(
                keyLeft ? -200f : keyRight ? 200f : 0,
                keyDown ? -200f : keyUp ? 200f : 0);

            if (keyReset)
            {
                ResetCamera();
            }
        }

        void RenderModel()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            spriteBatch.Begin(camera.GetViewMatrix());

            if (project != null)
            {
                var renderWidth = background.Width * scale;
                spriteBatch.Draw(background, new RectangleF(-renderWidth / 2f, -glControl.Height / 2f, renderWidth, glControl.Height));

                if (setScale)
                {
                    var drawMode = scaleRefs.Count == scaleRefs.Capacity ? BeginMode.LineStrip : BeginMode.Points;
                    spriteBatch.DrawVertices(scaleRefs, drawMode, Color4.Turquoise, 3);
                }

                if (project.Analysis != null)
                {
                    var points = project.Analysis.Points;
                    spriteBatch.DrawVertices(points.Select(point => point.Measurement), BeginMode.Points, Color4.Red, 3);

                    spriteBatch.DrawVertices(from measurement in project.Analysis.Measurements
                                             let distanceMeasurement = measurement as CephalometricDistanceMeasurement
                                             where distanceMeasurement != null &&
                                                   !string.IsNullOrEmpty(distanceMeasurement.Point0) &&
                                                   !string.IsNullOrEmpty(distanceMeasurement.Point1)
                                             from pointName in new[] { distanceMeasurement.Point0, distanceMeasurement.Point1 }
                                             select points[pointName].Measurement, BeginMode.Lines, Color4.Violet, 3);

                    foreach (var point in points)
                    {
                        spriteBatch.DrawString(font, point.Name, point.Measurement, 0, Vector2.One, Color4.Red);
                    }
                }
            }

            spriteBatch.End();
            glControl.SwapBuffers();
        }

        Vector2 PickModelPoint()
        {
            var viewportPosition = glControl.PointToClient(Form.MousePosition);
            var position = new Vector3(
                viewportPosition.X - glControl.Width * 0.5f,
                glControl.Height * 0.5f - viewportPosition.Y, 0);
            position /= spriteBatch.PixelsPerMeter;

            var view = camera.GetViewMatrix();
            view.Invert();
            Vector3.Transform(ref position, ref view, out position);

            return new Vector2(position.X, position.Y);
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            scaleRefs = new List<Vector2>(2);

            clock = new Stopwatch();
            clock.Start();

            GL.ClearColor(Color.Black);

            scale = 1;
            camera = new Camera2D();
            spriteBatch = new SpriteBatch(glControl.Width, glControl.Height);

            var fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenOrtho.Resources.LiberationMono-Regular.spf");
            font = SpriteFont.FromStream(fontStream);

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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog(this) == DialogResult.OK)
            {
                project = new OrthoProject();
                project.Radiograph = openImageDialog.FileName;
                LoadProject();

                setScale = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openProjectDialog.ShowDialog(this) == DialogResult.OK)
            {
                saveProjectDialog.FileName = openProjectDialog.FileName;
                using (var reader = XmlReader.Create(openProjectDialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof(OrthoProject));
                    project = (OrthoProject)serializer.Deserialize(reader);
                    LoadProject();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project == null) return;

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
            if (project == null) return;

            if (saveProjectDialog.ShowDialog(this) == DialogResult.OK)
            {
                saveToolStripMenuItem_Click(this, e);
            }
        }

        private void glControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (project == null) return;

            if (setScale && scaleRefs.Count < scaleRefs.Capacity)
            {
                var point = PickModelPoint();
                commandExecutor.Execute(
                    () => { scaleRefs.Add(point); setScaleButton.Enabled = scaleNumericUpDown.Enabled = scaleRefs.Count == scaleRefs.Capacity; },
                    () => { scaleRefs.RemoveAt(scaleRefs.Count - 1); setScaleButton.Enabled = scaleNumericUpDown.Enabled = false; });
            }
            else if (project.Analysis != null)
            {
                var point = new CephalometricPoint
                {
                    Name = "Point" + project.Analysis.Points.Count,
                    Measurement = PickModelPoint()
                };

                commandExecutor.Execute(
                    () => project.Analysis.Points.Add(point),
                    () => project.Analysis.Points.RemoveAt(project.Analysis.Points.Count - 1));
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutBox.ShowDialog(this);
        }

        private void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            undoToolStripButton.Enabled = commandExecutor.CanUndo;
            undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
            redoToolStripButton.Enabled = commandExecutor.CanRedo;
            redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            commandExecutor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            commandExecutor.Redo();
        }

        private void setScaleButton_Click(object sender, EventArgs e)
        {
            var scaleLength = (scaleRefs[0] - scaleRefs[1]).Length;
            project.PixelsPerMillimeter = scaleLength / (float)scaleNumericUpDown.Value;
            UpdateScale();
            ResetCamera();
            analysisPropertyGrid.Refresh();

            commandExecutor.Execute(() =>
            {
                scaleRefs.Clear();
                setScale = false;
                setScaleButton.Enabled = scaleNumericUpDown.Enabled = false;
            }, null);
        }
    }
}
