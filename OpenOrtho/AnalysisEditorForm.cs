using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenOrtho.Analysis;
using System.Xml;
using System.Xml.Serialization;

namespace OpenOrtho
{
    public partial class AnalysisEditorForm : Form
    {
        public AnalysisEditorForm()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = new CephalometricAnalysis();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = new CephalometricAnalysis();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                saveFileDialog.FileName = openFileDialog.FileName;
                using (var reader = XmlReader.Create(openFileDialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof(CephalometricAnalysis));
                    propertyGrid.SelectedObject = serializer.Deserialize(reader);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(saveFileDialog.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else
            {
                using (var writer = XmlWriter.Create(saveFileDialog.FileName, new XmlWriterSettings { Indent = true }))
                {
                    var serializer = new XmlSerializer(typeof(CephalometricAnalysis));
                    serializer.Serialize(writer, propertyGrid.SelectedObject);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                saveToolStripMenuItem_Click(this, e);
            }
        }
    }
}
