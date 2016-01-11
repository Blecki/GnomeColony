using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gem.Geo;
using Gem;

namespace Gem.Render
{
    public class MeshNode : SceneNode
    {
        public Mesh Mesh;
        public Vector3 Color = Vector3.One;
        public Texture2D Texture = null;
        public Texture2D NormalMap = null;
        public Texture2D HoverOverlay = null;
        public Matrix UVTransform = Matrix.Identity;
        public bool AlphaMouse = false;
        public bool HiliteOnHover = false;

        internal bool MouseHover = false;
        internal Vector2 LocalMouse = Vector2.Zero;

        public MeshNode(Mesh Mesh, Texture2D Texture, Texture2D NormalMap, Euler Orientation = null) 
        { 
            this.Mesh = Mesh;
            this.Texture = Texture;
            this.NormalMap = NormalMap;
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        public override void Draw(Gem.Render.RenderContext context)
        {
            context.Color = Color;
            if (Texture != null) context.Texture = Texture;
            context.NormalMap = NormalMap == null ? context.NeutralNormals : NormalMap;
            context.World = WorldTransform;
            context.UVTransform = UVTransform;
            context.LightingEnabled = true;
            context.ApplyChanges();
            context.Draw(Mesh);
            context.NormalMap = context.NeutralNormals;

            if (HiliteOnHover && MouseHover)
            {
                context.Texture = HoverOverlay;
                context.LightingEnabled = false;
                context.ApplyChanges();
                context.Draw(Mesh);
            }

            context.UVTransform = Matrix.Identity;
        }

        public override void CalculateLocalMouse(Ray MouseRay, Action<Gem.Render.SceneNode, float> HoverCallback)
        {
            MouseHover = false;

            if (InteractWithMouse == false) return;

            var localMouse = GetLocalMouseRay(MouseRay);

            var intersection = Mesh.RayIntersection(localMouse);
            
            if (intersection.Intersects)
            {
                LocalMouse = Vector2.Transform(intersection.UV, UVTransform);
                if (AlphaMouse)
                {
                    var pixel = new Color[] { new Color(1.0f, 1.0f, 1.0f, 1.0f) };
                    var pX = (int)System.Math.Round(LocalMouse.X * Texture.Width);
                    var pY = (int)System.Math.Round(LocalMouse.Y * Texture.Height);
                    Texture.GetData<Color>(0, new Rectangle(pX, pY, 1, 1), pixel, 0, 1);
                    if (pixel[0].A > 0.01f)
                        HoverCallback(this, intersection.Distance);
                }
                else
                    HoverCallback(this, intersection.Distance);
            }
        }

        public override Action GetHoverAction()
        {
            MouseHover = true;
            return base.GetHoverAction();
        }

    }
}
