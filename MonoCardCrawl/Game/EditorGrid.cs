using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Game
{
    public class EditorGrid : Gem.Render.SceneNode
    {
        private Gem.Geo.Mesh QuadMesh = null;
        private Gem.Geo.WireframeMesh GridMesh = null;

        internal bool MouseHover = false;
        internal int LocalMouseX = 0;
        internal int LocalMouseY = 0;

        private int DivisionsX;
        private int DivisionsY;
        private float cellWidth;
        private float cellHeight;

        public EditorGrid(GraphicsDevice Device, int DivisionsX, int DivisionsY, Euler Euler = null)
        {
            this.Orientation = Euler;
            if (this.Orientation == null) this.Orientation = new Euler();

            this.DivisionsX = DivisionsX;
            this.DivisionsY = DivisionsY;

            cellWidth = 1.0f / DivisionsX;
            cellHeight = 1.0f / DivisionsY;

            QuadMesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.CreateQuad());
            Gem.Geo.Gen.Transform(QuadMesh, Matrix.CreateTranslation(0.5f, 0.5f, 0.0f));
            GridMesh = Gem.Geo.WireframeMesh.GenerateGrid(1.0f, 1.0f, DivisionsX, DivisionsY);
        }

        private static float ScalarProjection(Vector3 A, Vector3 B)
        {
            return Vector3.Dot(A, B) / B.Length();
        }

        public override void CalculateLocalMouse(Ray MouseRay, Action<Gem.Render.SceneNode, float> HoverCallback)
        {
            MouseHover = false;

            var verts = new Vector3[3];
            verts[0] = new Vector3(0.0f, 0.0f, 0);
            verts[1] = new Vector3(1.0f, 0.0f, 0);
            verts[2] = new Vector3(0.0f, 1.0f, 0);

            for (int i = 0; i < 3; ++i)
                verts[i] = Vector3.Transform(verts[i], WorldTransform);

            var distance = MouseRay.Intersects(new Plane(verts[0], verts[1], verts[2]));
            if (distance == null || !distance.HasValue) return;
            if (distance.Value < 0) return; //GUI plane is behind camera
            var interesectionPoint = MouseRay.Position + (MouseRay.Direction * distance.Value);

            var x = ScalarProjection(interesectionPoint - verts[0], verts[1] - verts[0]) / (verts[1] - verts[0]).Length();
            var y = ScalarProjection(interesectionPoint - verts[0], verts[2] - verts[0]) / (verts[2] - verts[0]).Length();

            LocalMouseX = (int)(x * DivisionsX);
            LocalMouseY = (int)(y * DivisionsY);

            MouseHover = true;
        }

        public override void Draw(Gem.Render.RenderContext Context)
        {
            Context.Color = new Vector3(1, 0, 0);
            Context.Texture = Context.White;
            Context.NormalMap = Context.NeutralNormals;
            Context.World = Orientation.Transform;
            Context.LightingEnabled = false;
            Context.ApplyChanges();
            Context.Draw(GridMesh);

            Context.World =
                Orientation.Transform
                * Matrix.CreateScale(cellWidth, cellHeight, 1.0f)
                * Matrix.CreateTranslation(LocalMouseX, LocalMouseY, 0.0f);
            Context.Draw(QuadMesh);
        }
    }
}
