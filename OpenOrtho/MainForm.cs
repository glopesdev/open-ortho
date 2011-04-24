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
using System.Drawing.Printing;

namespace OpenOrtho
{
    public partial class MainForm : Form
    {
        const float MillimetersPerInch = 1.0f / 0.03937f;
        const float MovementSpeed = 200.0f;
        const float MinZoom = 1f;
        const float MaxZoom = 100f;

        OrthoProject project;
        int version;
        int saveVersion;

        bool loaded;
        float scale;
        SpriteFont font;
        Camera2D camera;
        Texture2D background;
        SpriteBatch spriteBatch;

        bool setScale;
        List<Vector2> scaleRefs;
        List<Vector2> arcPoints;

        bool fixPoint;
        Vector2 originalMeasurement;
        CephalometricPoint selectedPoint;

        bool keyUp;
        bool keyDown;
        bool keyLeft;
        bool keyRight;
        bool keyReset;
        bool keyZoomIn;
        bool keyZoomOut;
        Point prevMouse;

        Stopwatch clock;
        AboutBox aboutBox;
        AnalysisEditorForm analysisEditor;

        Bitmap screenCapture;
        float deviceDpiX;

        public MainForm()
        {
            InitializeComponent();
            aboutBox = new AboutBox();
            analysisEditor = new AnalysisEditorForm();
        }

        bool CheckUnsavedChanges()
        {
            if (project != null && saveVersion != version)
            {
                var result = MessageBox.Show("Project has unsaved changes. Save project file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.OK)
                {
                    saveToolStripMenuItem_Click(this, EventArgs.Empty);
                }
                else return result == DialogResult.No;
            }

            return true;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            camera.Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, camera.Zoom + 0.001f * camera.Zoom * e.Delta));
            base.OnMouseWheel(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!CheckUnsavedChanges()) e.Cancel = true;
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (background != null) background.Dispose();
            base.OnFormClosed(e);
        }

        RectangleF GetRenderRectangle()
        {
            var renderWidth = background.Width * scale;
            return new RectangleF(-renderWidth / 2f, -glControl.Height / 2f, renderWidth, glControl.Height);
        }

        void CaptureScreen()
        {
            using (var graphics = glControl.CreateGraphics())
            {
                var renderRectangle = GetRenderRectangle();
                var renderLocation = new Point(glControl.Location.X + (int)(renderRectangle.X + glControl.Width / 2f), glControl.Location.Y);
                var renderSize = new Size((int)renderRectangle.Width, (int)renderRectangle.Height);
                screenCapture = new Bitmap(renderSize.Width, renderSize.Height, graphics);
                screenCapture.SetResolution(spriteBatch.PixelsPerMeter * MillimetersPerInch, spriteBatch.PixelsPerMeter * MillimetersPerInch);

                var captureGraphics = System.Drawing.Graphics.FromImage(screenCapture);
                captureGraphics.CopyFromScreen(glControl.PointToScreen(renderLocation), Point.Empty, renderSize);
                deviceDpiX = graphics.DpiX;
            }
        }

        void ResetProjectStatus()
        {
            UpdateScale();
            ResetCamera();
            UpdateStatus();
            commandExecutor.Clear();
            version = 0;
            saveVersion = version;
        }

        void LoadProject()
        {
            if (background != null) background.Dispose();

            background = Texture2D.FromFile(project.Radiograph);
            analysisPropertyGrid.SelectedObject = project;
            analysisPropertyGrid.Enabled = true;
            ResetProjectStatus();
        }

        void UpdateScale()
        {
            if (background != null)
            {
                scale = (float)glControl.Height / (float)background.Height;
                spriteBatch.PixelsPerMeter = project.PixelsPerMillimeter * scale;
            }
        }

        void UpdateStatus()
        {
            var nextPlacement = project.Analysis.Points.FirstOrDefault(p => !p.Placed);

            if (setScale)
            {
                if (scaleRefs.Count < 2)
                {
                    placementToolStripStatusLabel.Text = "Place scale reference " + (scaleRefs.Count + 1);
                }
                else placementToolStripStatusLabel.Text = "Set scale length";
            }
            else if (nextPlacement != null)
            {
                placementToolStripStatusLabel.Text = "Place point: " + nextPlacement.Name;
            }
            else placementToolStripStatusLabel.Text = "Ready";
        }

        void ResetCamera()
        {
            camera.Zoom = 1;
            camera.Position = Vector2.Zero;
        }

        void UpdateModel(float time)
        {
            camera.Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, camera.Zoom + time * camera.Zoom * (keyZoomIn ? 1f : keyZoomOut ? -1f : 0)));
            camera.Position += time / spriteBatch.PixelsPerMeter * new Vector2(
                keyLeft ? -MovementSpeed : keyRight ? MovementSpeed : 0,
                keyDown ? -MovementSpeed : keyUp ? MovementSpeed : 0);

            if (Form.MouseButtons == MouseButtons.Right)
            {
                camera.Position += MovementSpeed * time / spriteBatch.PixelsPerMeter * new Vector2(
                    prevMouse.X - Form.MousePosition.X,
                    Form.MousePosition.Y - prevMouse.Y);
            }

            if (keyReset)
            {
                ResetCamera();
            }

            prevMouse = Form.MousePosition;
        }

        void Swap(ref Vector2 p0, ref Vector2 p1)
        {
            var tmp = p0;
            p0 = p1;
            p1 = tmp;
        }

        void RenderModel()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            spriteBatch.Begin(camera.GetViewMatrix());

            if (project != null)
            {
                spriteBatch.Draw(background, GetRenderRectangle());

                if (setScale)
                {
                    var drawMode = scaleRefs.Count == scaleRefs.Capacity ? BeginMode.LineStrip : BeginMode.Points;
                    spriteBatch.DrawVertices(scaleRefs, drawMode, Color4.Turquoise, 3);
                }
                else if (project.Analysis != null)
                {
                    var points = project.Analysis.Points;
                    var missingPoints = points.FirstOrDefault(p => !p.Placed) != null;
                    if (linesToolStripMenuItem.Checked && !missingPoints)
                    {
                        spriteBatch.DrawVertices(from measurement in project.Analysis.Measurements
                                                 let distanceMeasurement = measurement as CephalometricDistanceMeasurement
                                                 where distanceMeasurement != null &&
                                                       !string.IsNullOrEmpty(distanceMeasurement.Point0) &&
                                                       !string.IsNullOrEmpty(distanceMeasurement.Point1)
                                                 let point0 = points[distanceMeasurement.Point0]
                                                 let point1 = points[distanceMeasurement.Point1]
                                                 where point0.Placed && point1.Placed
                                                 from point in new[] { point0.Measurement, point1.Measurement }
                                                 select point, BeginMode.Lines, Color4.Violet, 3);
                    }

                    if (pointLineDistancesToolStripMenuItem.Checked && !missingPoints)
                    {
                        var measurements = from measurement in project.Analysis.Measurements
                                           let lineDistanceMeasurement = measurement as CephalometricLineDistanceMeasurement
                                           where lineDistanceMeasurement != null &&
                                                 !string.IsNullOrEmpty(lineDistanceMeasurement.Point) &&
                                                 !string.IsNullOrEmpty(lineDistanceMeasurement.Line0) &&
                                                 !string.IsNullOrEmpty(lineDistanceMeasurement.Line1)
                                           let point = points[lineDistanceMeasurement.Point]
                                           let line0 = points[lineDistanceMeasurement.Line0]
                                           let line1 = points[lineDistanceMeasurement.Line1]
                                           where point.Placed && line0.Placed && line1.Placed
                                           select new
                                           {
                                               p = point.Measurement,
                                               l0 = line0.Measurement,
                                               l1 = line1.Measurement,
                                               lp = Utilities.PointOnLine(point.Measurement, line0.Measurement, line1.Measurement)
                                           };

                        spriteBatch.DrawVertices(from m in measurements
                                                 from point in new[] { m.lp, Utilities.ClosestOnLine(m.lp, m.l0, m.l1) }
                                                 select point, BeginMode.Lines, Color4.Orange, 3);

                        spriteBatch.DrawVertices(from m in measurements
                                                 from point in new[] { m.p, m.lp }
                                                 select point, BeginMode.Lines, Color4.Blue, 3);
                    }

                    if (anglesToolStripMenuItem.Checked && !missingPoints)
                    {
                        var measurements = from measurement in project.Analysis.Measurements
                                           let angleMeasurement = measurement as CephalometricAngleMeasurement
                                           where angleMeasurement != null &&
                                                 !string.IsNullOrEmpty(angleMeasurement.PointA0) &&
                                                 !string.IsNullOrEmpty(angleMeasurement.PointA1) &&
                                                 !string.IsNullOrEmpty(angleMeasurement.PointB0) &&
                                                 !string.IsNullOrEmpty(angleMeasurement.PointB1)
                                           let pointA0 = points[angleMeasurement.PointA0]
                                           let pointA1 = points[angleMeasurement.PointA1]
                                           let pointB0 = points[angleMeasurement.PointB0]
                                           let pointB1 = points[angleMeasurement.PointB1]
                                           where pointA0.Placed && pointA1.Placed && pointB0.Placed && pointB1.Placed
                                           let intersection = Utilities.LineIntersection(pointA0.Measurement, pointA1.Measurement, pointB0.Measurement, pointB1.Measurement)
                                           where intersection.HasValue
                                           select new
                                           {
                                               pA0 = pointA0.Measurement,
                                               pA1 = pointA1.Measurement,
                                               pB0 = pointB0.Measurement,
                                               pB1 = pointB1.Measurement,
                                               intersection = intersection.Value,
                                               angle = angleMeasurement.Measure(points)
                                           };

                        spriteBatch.DrawVertices(from m in measurements
                                                 from point in new[] { m.intersection, Utilities.ClosestOnLine(m.intersection, m.pA0, m.pA1), m.intersection, Utilities.ClosestOnLine(m.intersection, m.pB0, m.pB1) }
                                                 select point, BeginMode.Lines, Color4.Orange, 3);

                        foreach (var m in measurements)
                        {
                            var angleIncrement = MathHelper.DegreesToRadians(m.angle) / (arcPoints.Capacity - 1);
                            bool swapA = false;
                            bool swapB = false;

                            var a0 = m.pA0;
                            var a1 = m.pA1;
                            if (Utilities.ClosestOnLineExclusive(m.intersection, a0, a1) != a0) Swap(ref a0, ref a1);

                            var b0 = m.pB0;
                            var b1 = m.pB1;
                            if (Utilities.ClosestOnLineExclusive(m.intersection, b0, b1) != b0) Swap(ref b0, ref b1);

                            var dot = Vector2.Dot(a1 - a0, b1 - b0);

                            var dotTest =
                                swapA && swapB ? dot > 0 : dot < 0;

                            var direction = dotTest ? (a1 - a0) : (b1 - b0);
                            direction.Normalize();

                            for (int i = 0; i < arcPoints.Capacity; i++)
                            {
                                arcPoints.Add(m.intersection + direction * 4);
                                direction = Utilities.Rotate(direction, angleIncrement);
                            }

                            spriteBatch.DrawVertices(arcPoints, BeginMode.LineStrip, Color4.Orange, 3);
                            spriteBatch.DrawString(font, string.Format("{0:F1} {1} {2}", dot, swapA, swapB), m.intersection, 0, Vector2.One, Color4.Red);
                            arcPoints.Clear();
                        }
                    }

                    spriteBatch.DrawVertices(from point in points
                                             where point.Placed
                                             select point.Measurement, BeginMode.Points, Color4.Red, 3);

                    var textScale = Vector2.One / camera.Zoom;
                    if (pointNamesToolStripMenuItem.Checked)
                    {
                        foreach (var point in points)
                        {
                            if (!point.Placed) continue;
                            spriteBatch.DrawString(font, point.Name, point.Measurement, 0, textScale, Color4.Red);
                        }
                    }
                    else
                    {
                        var closestPoint = ClosestPoint(PickModelPoint(), 5);
                        if (closestPoint != null)
                        {
                            spriteBatch.DrawString(font, closestPoint.Name, closestPoint.Measurement, 0, textScale, Color4.Red);
                        }
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

        CephalometricPoint ClosestPoint(Vector2 measurement, float threshold)
        {
            return (from p in project.Analysis.Points
                    let distance = (measurement - p.Measurement).Length
                    where distance < threshold * spriteBatch.PixelsPerMeter
                    orderby distance
                    select p).FirstOrDefault();
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            printDocument.DefaultPageSettings.Landscape = true;
            scaleRefs = new List<Vector2>(2);
            arcPoints = new List<Vector2>(11);

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
            if (!CheckUnsavedChanges()) return;

            if (openImageDialog.ShowDialog() == DialogResult.OK)
            {
                project = new OrthoProject();
                project.Radiograph = openImageDialog.FileName;
                setScale = true;
                LoadProject();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            if (openProjectDialog.ShowDialog() == DialogResult.OK)
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
                    saveVersion = version;
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project == null) return;

            if (saveProjectDialog.ShowDialog() == DialogResult.OK)
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
                    () => { scaleRefs.Add(point); setScaleButton.Enabled = scaleNumericUpDown.Enabled = scaleRefs.Count == scaleRefs.Capacity; UpdateStatus(); },
                    () => { scaleRefs.RemoveAt(scaleRefs.Count - 1); setScaleButton.Enabled = scaleNumericUpDown.Enabled = false; UpdateStatus(); });
            }
            else if (project.Analysis != null)
            {
                var measurement = PickModelPoint();
                if (fixPoint)
                {
                    var point = selectedPoint;
                    var prevMeasurement = originalMeasurement;

                    commandExecutor.Execute(
                        () => point.Measurement = measurement,
                        () => point.Measurement = prevMeasurement);

                    originalMeasurement = Vector2.Zero;
                    selectedPoint = null;
                    fixPoint = false;
                }
                else
                {
                    var point = project.Analysis.Points.FirstOrDefault(p => !p.Placed);
                    if (point != null)
                    {
                        var prevMeasurement = point.Measurement;

                        commandExecutor.Execute(
                            () => { point.Measurement = measurement; point.Placed = true; UpdateStatus(); },
                            () => { point.Measurement = prevMeasurement; point.Placed = false; UpdateStatus(); });
                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutBox.ShowDialog();
        }

        private void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            undoToolStripButton.Enabled = commandExecutor.CanUndo;
            undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
            redoToolStripButton.Enabled = commandExecutor.CanRedo;
            redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
            version++;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            version -= 2;
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
            analysisPropertyGrid.Refresh();

            scaleRefs.Clear();
            setScale = false;
            setScaleButton.Enabled = scaleNumericUpDown.Enabled = false;

            ResetProjectStatus();
        }

        private void analysisEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analysisEditor.ShowDialog();
        }

        private void selectAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project == null) return;

            if (openAnalysisDialog.ShowDialog() == DialogResult.OK)
            {
                using (var reader = XmlReader.Create(openAnalysisDialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof(CephalometricAnalysis));
                    project.Analysis = (CephalometricAnalysis)serializer.Deserialize(reader);
                    analysisPropertyGrid.Refresh();
                    UpdateStatus();
                }
            }
        }

        private void glControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (project == null || project.Analysis == null) return;

            if (e.Button == MouseButtons.Left)
            {
                if (project.Analysis.Points.FirstOrDefault(p => !p.Placed) != null) return;

                var measurement = PickModelPoint();
                if (!fixPoint)
                {
                    var point = ClosestPoint(measurement, 1);
                    if (point != null)
                    {
                        originalMeasurement = point.Measurement;
                        selectedPoint = point;
                        fixPoint = true;
                    }
                }
                else selectedPoint.Measurement = measurement;
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project != null)
            {
                CaptureScreen();
                printDialog.Document = printDocument;
            }
            printDialog.ShowDialog();
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project != null)
            {
                CaptureScreen();
                printPreviewDialog.Document = printDocument;
            }
            printPreviewDialog.ShowDialog();
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var offset = new PointF(e.PageSettings.PrintableArea.X, e.PageSettings.PrintableArea.Y);
            e.Graphics.DrawImage(screenCapture, offset);

            var analysis = project.Analysis;
            offset.X += screenCapture.Width / screenCapture.HorizontalResolution * deviceDpiX + 50;

            var reportString = "Measurements:";
            e.Graphics.DrawString(reportString, Font, Brushes.Black, offset);
            offset.Y += e.Graphics.MeasureString(reportString, Font).Height * 2;

            foreach (var measurement in analysis.Measurements)
            {
                var readout = string.Format("{0}: {1:F1} ({2})", measurement.Name, measurement.Measure(analysis.Points), measurement.Units);
                e.Graphics.DrawString(readout, Font, Brushes.Black, offset);
                offset.Y += e.Graphics.MeasureString(readout, Font).Height;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
