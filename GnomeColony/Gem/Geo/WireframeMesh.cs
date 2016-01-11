using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class WireframeMesh : Mesh, IMesh
    {
        void IMesh.Render(GraphicsDevice Device)
        {
            Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, verticies, 0, verticies.Length, indicies, 0, indicies.Length / 2);
        }
    }
}