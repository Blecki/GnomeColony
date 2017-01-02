using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.RenderModule
{
    public class WorldSceneNodeProperties : Gem.PropertyBag
    {
        public BlockSet BlockSet { set { Upsert("block-set", value); } }
        public int HiliteTexture { set { Upsert("hilite-texture", value); } }
    }

    public class WorldSceneNode : Gem.Render.SceneNode
    {
        public static bool WireFrameMode = false;
        
        private CellGrid World;
        private Gem.Common.Grid3D<Gem.Geo.Mesh> ChunkMeshes;
        
        public int HiliteTexture;
        public float PickRadius = 100.0f;

        private bool MouseHover = false;
        private Gem.Geo.Mesh HiliteQuad;

        public Gem.Geo.Mesh PhantomPlacementMesh = null;

        public void SetPhantomPlacements(List<PhantomBlock> Phantoms)
        {
            PhantomPlacementMesh = Gem.Geo.Gen.Merge(
                Phantoms.Select(p =>
                {
                    if (p.Block.Type == BlockType.Decal)
                        return Generate.GenerateDecalPreviewGeometry(p.Block, Blocks, p.TargetCell,
                            p.FinalPosition.X, p.FinalPosition.Y, p.FinalPosition.Z);
                    else
                        return Gem.Geo.Gen.Merge(Generate.CreatePreviewBlockMesh(Blocks.Tiles,
                            p.FinalBlock,
                            (int)p.Orientation,
                            p.FinalPosition.X, p.FinalPosition.Y, p.FinalPosition.Z).ToArray());
                    
                }).ToArray());
        }
        
        public void ClearPhantomPlacementCell()
        {
            PhantomPlacementMesh = null;
        }

        public Coordinate HoverBlock { get; private set; }
        public Coordinate AdjacentHoverBlock { get; private set; }
        public Vector3 HoverNormal { get; private set; }

        private BlockSet Blocks;

        public Vector3 SunPosition = new Vector3(0, 100.0f, 1000.0f);

        public WorldSceneNode(CellGrid World, Gem.PropertyBag Properties)
        {
            this.World = World;
            this.Blocks = Properties.GetPropertyAsOrDefault<BlockSet>("block-set");
            this.HiliteTexture = Properties.GetPropertyAsOrDefault<int>("hilite-texture");
            this.Orientation = new Gem.Euler();
            ChunkMeshes = new Gem.Common.Grid3D<Gem.Geo.Mesh>(World.width / 16, World.height / 16, World.depth / 16, (x,y,z) => null);

            ChunkMeshes.forAll((m, x, y, z) => World.MarkDirtyBlock(new Coordinate(x * 16, y * 16, z * 16)));
        }

        public void UpdateGeometry()
        {
            foreach (var chunkCoordinate in World.DirtyChunks)
            {
                var chunkMesh = Generate.ChunkGeometry(World,
                    chunkCoordinate.X * 16, chunkCoordinate.Y * 16, chunkCoordinate.Z * 16, 16, 16, 16, Blocks);
                ChunkMeshes[chunkCoordinate.X, chunkCoordinate.Y, chunkCoordinate.Z] = chunkMesh;
                if (chunkMesh != null) chunkMesh.PrepareLineIndicies();
            }

            World.DirtyChunks.Clear();
        }

        public override void UpdateWorldTransform(Microsoft.Xna.Framework.Matrix M)
        {
            WorldTransform = M * Orientation.Transform;
        }

        public override void PreDraw(float ElapsedSeconds, Gem.Render.RenderContext Context)
        {
            UpdateGeometry();
        }

        public override void SetHover()
        {
            MouseHover = true;
        }

        public override void Draw(Gem.Render.RenderContext Context)
        {
            Context.Color = Vector3.One;
            Context.Texture = Blocks.Tiles.Texture;
            Context.NormalMap = Context.NeutralNormals;
            Context.World = WorldTransform;
            Context.LightingEnabled = true;

            Context.SetLight(0, SunPosition, float.PositiveInfinity, new Vector3(1.0f, 1.0f, 1.0f));
            Context.ActiveLightCount = 1;

            Context.ApplyChanges();

            if (WireFrameMode)
                ChunkMeshes.forAll((m, x, y, z) => Context.DrawLines(m));
            else
                ChunkMeshes.forAll((m, x, y, z) => Context.Draw(m));

            if (MouseHover)
            {
                if (PhantomPlacementMesh != null)
                {
                    Context.Alpha = 0.75f;
                    Context.ApplyChanges();
                    Context.Draw(PhantomPlacementMesh);
                    PhantomPlacementMesh = null;
                }

                Context.LightingEnabled = false;
                Context.UVTransform = Blocks.Tiles.TileMatrix(HiliteTexture);
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
