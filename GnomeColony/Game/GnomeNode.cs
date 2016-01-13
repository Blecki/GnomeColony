using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class GnomeNode : Gem.Render.BranchNode
    {
        private Gem.Render.SceneNode CarriedResourceVisual = null;
        private Gem.Render.MeshNode MainNode;
        private Gem.Render.MeshNode TaskIcon;

        private TileSheet Sheet;

        private Gnome ParentGnome;
        private Simulation Sim;

        public GnomeNode(Gnome ParentGnome, Simulation Sim, TileSheet Sheet) : base(ParentGnome.Orientation)
        {
            this.ParentGnome = ParentGnome;
            this.Sim = Sim;
            this.Sheet = Sheet;

            var gnomeMesh = Gem.Geo.Gen.CreateTexturedFacetedCube();
            Gem.Geo.Gen.Transform(gnomeMesh, Matrix.CreateTranslation(0.0f, 0.0f, 0.5f));
            Gem.Geo.Gen.Transform(gnomeMesh, Matrix.CreateScale(0.75f, 0.75f, 1.5f));

            Gem.Geo.Gen.MorphEx(gnomeMesh, (inV) =>
            {
                var r = inV;

                if (r.Normal.Z > 0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeBottom, 1, 1));
                else if (r.Normal.Z < -0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeTop, 1, 1));
                else if (r.Normal.Y > 0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeFront, 1, 2));
                else if (r.Normal.Y < -0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeBack, 1, 2));
                else
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeSide, 1, 2));
                return r;
            });

            MainNode = new Gem.Render.MeshNode(gnomeMesh, Sheet.Texture, null);
            base.Add(MainNode);

            var iconMesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.CreateQuad());
            Gem.Geo.Gen.Transform(iconMesh, Matrix.CreateFromYawPitchRoll(0.0f, Gem.Math.Angle.PI / 2.0f, 0.0f));
            Gem.Geo.Gen.Transform(iconMesh, Matrix.CreateTranslation(0, 0, 2));
            Gem.Geo.Gen.CalculateTangentsAndBiNormals(iconMesh);
            TaskIcon = new Gem.Render.MeshNode(iconMesh, Sheet.Texture, null);
            TaskIcon.InteractWithMouse = false;
            base.Add(TaskIcon);
        }

        public override void PreDraw(float ElapsedSeconds, Gem.Render.RenderContext Context)
        {
            base.PreDraw(ElapsedSeconds, Context);

            if (ParentGnome.CarriedResource == 0 && CarriedResourceVisual != null)
            {
                base.Remove(CarriedResourceVisual);
                CarriedResourceVisual = null;
            }
            else if (ParentGnome.CarriedResource != 0 && CarriedResourceVisual == null)
            {
                CarriedResourceVisual = new Gem.Render.MeshNode(
                    Generate.CreateResourceBlockMesh(Sim.Blocks.BlockTiles, Sim.Blocks.BlockTemplates[ParentGnome.CarriedResource]),
                    Sim.Blocks.BlockTiles.Texture, null);
                base.Add(CarriedResourceVisual);
            }

            // Rotate the carried resource to face the right way.
            if (CarriedResourceVisual != null)
            {
                CarriedResourceVisual.Orientation.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                    (Gem.Math.Angle.PI / 2) * (int)ParentGnome.FacingDirection);
                CarriedResourceVisual.Orientation.Position = Vector3.Transform(
                    new Vector3(0.0f, -0.5f, 0.5f), CarriedResourceVisual.Orientation.Orientation);
            }

            TaskIcon.UVTransform = Sheet.TileMatrix(ParentGnome.Mind.GetStateIcon());
            
            // Orient task icon toward camera
            var cameraPos = Context.Camera.GetPosition();
            var cameraDelta = this.Orientation.Position - cameraPos;
            var billboardAngle = Gem.Math.Vector.AngleBetweenVectors(new Vector2(0, 1), new Vector2(cameraDelta.X, cameraDelta.Y));
            TaskIcon.Orientation.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, billboardAngle);

            // Face gnome correct direction
            MainNode.Orientation.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                (Gem.Math.Angle.PI / 2) * (int)ParentGnome.FacingDirection);
        }
    }
}
