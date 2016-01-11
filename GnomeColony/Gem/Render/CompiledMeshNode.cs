using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gem.Geo;

namespace Gem.Render
{
    public class CompiledMeshNode : SceneNode
    {
        public CompiledModel Mesh;
        public Vector3 Color = Vector3.One;
        public Texture2D Texture = null;

        public CompiledMeshNode(CompiledModel Mesh, Texture2D Texture, Euler Orientation = null)
        {
            this.Mesh = Mesh;
            this.Texture = Texture;
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        public override void Draw(RenderContext context)
        {
            context.Color = Color;
            if (Texture != null) context.Texture = Texture;
            else context.Texture = context.White;
            context.NormalMap = context.NeutralNormals;
            context.World = WorldTransform;
            context.ApplyChanges();
            context.Draw(Mesh);
        }
    }
}
