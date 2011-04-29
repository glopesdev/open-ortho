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
using System.IO;

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
        bool nonPowerOfTwo;
        bool frameBufferObjects;
        int backgroundWidth;
        int backgroundHeight;
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
            var renderWidth = backgroundWidth * scale;
            return new RectangleF(-renderWidth / 2f, -glControl.Height / 2f, renderWidth, glControl.Height);
        }

        void CaptureScreen()
        {
            using (var graphics = glControl.CreateGraphics())
            {
                if (frameBufferObjects && nonPowerOfTwo)
                {
                    using (var renderTarget = new RenderTarget2D(backgroundWidth, backgroundHeight))
                    {
                        var pixelsPerMeter = spriteBatch.PixelsPerMeter;
                        spriteBatch.SetDimensions(backgroundWidth, backgroundHeight);
                        spriteBatch.PixelsPerMeter = project.PixelsPerMillimeter;

                        renderTarget.Begin();
                        RenderModel(new RectangleF(-backgroundWidth / 2, -backgroundHeight / 2, backgroundWidth, backgroundHeight), 6, 2);
                        renderTarget.End();

                        screenCapture = renderTarget.Texture.ToBitmap();
                        screenCapture.SetResolution(project.PixelsPerMillimeter * MillimetersPerInch, project.PixelsPerMillimeter * MillimetersPerInch);

                        spriteBatch.PixelsPerMeter = pixelsPerMeter;
                        spriteBatch.SetDimensions(glControl.Width, glControl.Height);
                    }
                }
                else
                {
                    var renderRectangle = GetRenderRectangle();
                    var renderLocation = new Point(glControl.Location.X + (int)(renderRectangle.X + glControl.Width / 2f), glControl.Location.Y);
                    var renderSize = new Size((int)renderRectangle.Width, (int)renderRectangle.Height);
                    screenCapture = new Bitmap(renderSize.Width, renderSize.Height, graphics);
                    screenCapture.SetResolution(spriteBatch.PixelsPerMeter * MillimetersPerInch, spriteBatch.PixelsPerMeter * MillimetersPerInch);

                    var captureGraphics = System.Drawing.Graphics.FromImage(screenCapture);
                    captureGraphics.CopyFromScreen(glControl.PointToScreen(renderLocation), Point.Empty, renderSize);
                }

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

        void LoadProject(string projectDir)
        {
            if (background != null) background.Dispose();

            var radiographPath = Path.IsPathRooted(project.Radiograph) ? project.Radiograph : Path.Combine(projectDir, project.Radiograph);
            using (var bitmap = new Bitmap(radiographPath))
            {
                var query = new int[1];
                GL.GetInteger(GetPName.MaxTextureSize, query);
                var maxSize = query[0];

                backgroundWidth = bitmap.Width;
                backgroundHeight = bitmap.Height;
                if (!nonPowerOfTwo || backgroundWidth > maxSize || backgroundHeight > maxSize)
                {
                    var width = Math.Min(maxSize, Utilities.NearestPowerOfTwo(backgroundWidth));
                    var height = Math.Min(maxSize, Utilities.NearestPowerOfTwo(backgroundHeight));
                    using (var potsBitmap = new Bitmap(bitmap, width, height))
                    {
                        background = Texture2D.FromBitmap(potsBitmap);
                    }
                }
                else background = Texture2D.FromBitmap(bitmap);
            }

            analysisPropertyGrid.SelectedObject = project;
            analysisPropertyGrid.Enabled = true;
            ResetProjectStatus();
        }

        void ClearSetScale()
        {
            scaleRefs.Clear();
            setScale = false;
            setScaleButton.Enabled = scaleNumericUpDown.Enabled = false;
        }

        void UpdateScale()
        {
            if (background != null)
            {
                scale = (float)glControl.Height / (float)backgroundHeight;
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
                var descriptionSuffix = !string.IsNullOrEmpty(nextPlacement.Description) ? string.Format(" ({0})", nextPlacement.Description) : string.Empty;
                placementToolStripStatusLabel.Text = "Place point: " + nextPlacement.Name + descriptionSuffix;
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
            RenderModel(GetRenderRectangle(), 3, 1);
        }

        void RenderModel(RectangleF renderRectangle, float markerSize, float textSize)
        {
            GL.PointSize(markerSize);
            GL.LineWidth(markerSize);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            spriteBatch.Begin(camera.GetViewMatrix());

            if (project != null)
            {
                var points = project.Analysis.Points;
                spriteBatch.Draw(background, renderRectangle);

                if (setScale)
                {
                    var drawMode = scaleRefs.Count == scaleRefs.Capacity ? BeginMode.LineStrip : BeginMode.Points;
                    spriteBatch.DrawVertices(scaleRefs, drawMode, Color4.Turquoise);
                }
                else if (project.Analysis != null)
                {
                    var missingPoints = points.FirstOrDefault(p => !p.Placed) != null;
                    if (!missingPoints)
                    {
                        var options = DrawingOptions.None;
                        if (namesToolStripMenuItem.Checked) options |= DrawingOptions.Names;
                        if (mainLinesToolStripMenuItem.Checked) options |= DrawingOptions.MainLines;
                        if (auxiliaryLinesToolStripMenuItem.Checked) options |= DrawingOptions.AuxiliaryLines;
                        if (distanceLinesToolStripMenuItem.Checked) options |= DrawingOptions.DistanceLines;

                        foreach (var measurement in project.Analysis.Measurements)
                        {
                            if (!measurement.Enabled) continue;
                            measurement.Draw(spriteBatch, project.Analysis.Points, project.Analysis.Measurements, options);
                        }
                    }

                    spriteBatch.DrawVertices(from point in points
                                             where point.Placed
                                             select point.Measurement, BeginMode.Points, Color4.Red);

                    var textScale = textSize * Vector2.One / camera.Zoom;
                    if (namesToolStripMenuItem.Checked)
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
                        if (closestPoint != null && closestPoint.Placed)
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
            glControl.VSync = true;
            printDocument.DefaultPageSettings.Landscape = true;
            scaleRefs = new List<Vector2>(2);
            arcPoints = new List<Vector2>(11);

            var extensions = GL.GetString(StringName.Extensions).Split(' ');
            nonPowerOfTwo = extensions.Contains("GL_ARB_texture_non_power_of_two");
            frameBufferObjects = extensions.Contains("GL_ARB_framebuffer_object");

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
                var projectDir = Path.GetDirectoryName(openImageDialog.FileName);
                saveProjectDialog.InitialDirectory = projectDir;
                project = new OrthoProject();
                project.Radiograph = openImageDialog.FileName;
                setScale = true;
                LoadProject(projectDir);
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
                    ClearSetScale();
                    LoadProject(Path.GetDirectoryName(openProjectDialog.FileName));
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

            saveProjectDialog.FileName = Path.ChangeExtension(Path.GetFileName(openImageDialog.FileName), ".ortho");
            if (saveProjectDialog.ShowDialog() == DialogResult.OK)
            {
                var projectUri = new Uri(saveProjectDialog.FileName);
                if (Path.IsPathRooted(project.Radiograph))
                {
                    var radiographUri = new Uri(project.Radiograph);
                    radiographUri = projectUri.MakeRelativeUri(radiographUri);
                    project.Radiograph = radiographUri.ToString();
                }
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

            ClearSetScale();
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

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
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
                if (!measurement.Enabled) continue;

                var readout = string.Format("{0}: {1:F1} ({2})", measurement.Name, measurement.Measure(analysis.Points, analysis.Measurements), measurement.Units);
                e.Graphics.DrawString(readout, Font, Brushes.Black, offset);
                offset.Y += e.Graphics.MeasureString(readout, Font).Height;
            }

            e.HasMorePages = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void enableAllMeasurementsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project == null) return;

            foreach (var measurement in project.Analysis.Measurements)
            {
                measurement.Enabled = true;
            }
        }

        private void disableAllMeasurementsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project == null) return;

            foreach (var measurement in project.Analysis.Measurements)
            {
                measurement.Enabled = false;
            }
        }
    }
}
