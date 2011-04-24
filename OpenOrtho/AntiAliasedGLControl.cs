using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace OpenOrtho
{
    public partial class AntiAliasedGLControl : GLControl
    {
        public AntiAliasedGLControl()
            : base(new GraphicsMode(GraphicsMode.Default.ColorFormat, GraphicsMode.Default.Depth, GraphicsMode.Default.Stencil, 4))
        {
        }
    }
}
