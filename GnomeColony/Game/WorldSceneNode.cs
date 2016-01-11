using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class WorldSceneNodeProperties : Gem.PropertyBag
    {
        public TileSheet TileSheet { set { Upsert("tile-sheet", value); } }
        public int HiliteTexture { set { Upsert("hilite-texture", value); } }
        public BlockTemplateSet BlockTemplates { set { Upsert("block-templates", value); } }
    }

    public class WorldSceneNode : Gem.Render.SceneNode
    {
        private CellGrid World;
        private Gem.Geo.Mesh ChunkMesh;
        
        public int HiliteTexture;
        public float PickRadius = 100.0f;

        private bool MouseHover = false;
        private Gem.Geo.Mesh HiliteQuad;

        public Coordinate HoverBlock { get; private set; }
        public Coordinate AdjacentHoverBlock { get; private set; }
        public Vector3 HoverNormal { get; private set; }

        private TileSheet TileSheet;
        private BlockTemplateSet BlockTemplates;

        public WorldSceneNode(CellGrid World, Gem.PropertyBag Properties)
        {
            this.World = World;
            this.TileSheet = Properties.GetPropertyAsOrDefault<TileSheet>("tile-sheet");
            this.HiliteTexture = Properties.GetPropertyAsOrDefault<int>("hilite-texture");
            this.BlockTemplates = Properties.GetPropertyAsOrDefault<BlockTemplateSet>("block-templates");
            this.Orientation = new Gem.Euler();
        }

        public void UpdateGeometry()
        {
            ChunkMesh = Generate.ChunkGeometry(World, TileSheet, BlockTemplates);
        }

        public override void UpdateWorldTransform(Microsoft.Xna.Framework.Matrix M)
        {
            WorldTransform = M * Orientation.Transform;
        }

        public override void PreDraw(float ElapsedSeconds, Gem.Render.RenderContext Context)
        {
        }

        public override void SetHover()
        {
            MouseHover = true;
        }

        public override void Draw(Gem.Render.RenderContext Context)
        {
            Context.Color = Vector3.One;
            Context.Texture = TileSheet.Texture;
            Context.NormalMap = Context.NeutralNormals;
            Context.World = WorldTransform;
            Context.LightingEnabled = true;
            Context.ApplyChanges();
            Context.Draw(ChunkMesh);

            if (MouseHover)
            {
                Context.LightingEnabled = false;
                Context.UVTransform = TileSheet.TileMatrix(HiliteTexture);
                Context.ApplyChanges();
                Context.Draw(HiliteQuad);
            }
        }

        public override void CalculateLocalMouse(Microsoft.Xna.Framework.Ray MouseRay, Action<Gem.Render.SceneNode, float> HoverCallback)
        {
            MouseHover = false;

            var localMouse = GetLocalMouseRay(MouseRay);

            GridTraversal.Raycast(localMouse.Position, localMouse.Direction, PickRadius, (x, y, z, normal) =>
            {
                if (!World.check(x, y, z)) return false;
                if (World.CellAt(x, y, z).Block == null) return false;
                if (World.CellAt(x, y, z).Block.Solid == false) return false;

                HiliteQuad = Gem.Geo.Gen.CreateQuad();

                // Align quad with correct face.
                if (normal.Z < 0) // normal points down. 
                    Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateFromAxisAngle(Vector3.UnitX, Gem.Math.Angle.PI));
                else if (normal.Z == 0)
                    Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateFromAxisAngle(
                        Vector3.Cross(-Vector3.UnitZ, normal), -Gem.Math.Angle.PI / 2));

                // Move quad to center of block.
                Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z + 0.5f));

                // Move quad out to correct surface.
                Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateTranslation(normal * 0.52f));

                var faceIntersection = HiliteQuad.RayIntersection(localMouse);
                //if (faceIntersection.Intersects == false) throw new InvalidProgramException();

                HoverCallback(this, faceIntersection.Distance);

                HoverBlock = new Coordinate(x, y, z);
                AdjacentHoverBlock = new Coordinate((int)(x + normal.X), (int)(y + normal.Y), (int)(z + normal.Z));
                HoverNormal = normal;

                return true;
            });
        }
    }
}
