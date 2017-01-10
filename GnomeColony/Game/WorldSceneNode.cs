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
        public BlockSet BlockSet { set { Upsert("block-set", value); } }
        public int HiliteTexture { set { Upsert("hilite-texture", value); } }
    }

    public class CompiledChunk
    {
        public Gem.Geo.Mesh Mesh;
        public List<OrientedBlock> Lights;
    }

    public class WorldSceneNode : Gem.Render.SceneNode
    {
        public static bool WireFrameMode = false;
        
        private CellGrid World;
        private Gem.Common.Grid3D<CompiledChunk> Chunks;
        
        public int HiliteTexture;
        public float PickRadius = 100.0f;

        private bool MouseHover = false;
        private Gem.Geo.Mesh HiliteQuad;

        public Gem.Geo.Mesh PhantomPlacementMesh = null;
        public Vector3 PhantomColor = new Vector3(1, 1, 1);

        public void SetPhantomPlacements(List<PhantomBlock> Phantoms)
        {
            if (Phantoms.Count(p => p.PlacementAllowed) == Phantoms.Count)
                PhantomColor = new Vector3(1, 1, 1);
            else
                PhantomColor = new Vector3(1, 0, 0);

            PhantomPlacementMesh = Gem.Geo.Gen.Merge(
                Phantoms.Select(p =>
                {
                    var meshList = new List<Gem.Geo.Mesh>();
                    Generate.GenerateCellGeometry(meshList, World, Blocks, p.Block);
                    return Gem.Geo.Gen.Merge(meshList.ToArray());
                }).ToArray());
        }
        
        public void ClearPhantomPlacementCell()
        {
            PhantomPlacementMesh = null;
        }

        public Coordinate HoverBlock { get; private set; }
        public Coordinate AdjacentHoverBlock { get; private set; }
        public Vector3 HoverNormal { get; private set; }
        public Vector3 RealHoverNormal { get; private set; }
        public Vector3 HitLocation { get; private set; }

        private BlockSet Blocks;

        public Vector3 SunPosition = new Vector3(0, 100.0f, 1000.0f);

        public WorldSceneNode(CellGrid World, Gem.PropertyBag Properties)
        {
            this.World = World;
            this.Blocks = Properties.GetPropertyAsOrDefault<BlockSet>("block-set");
            this.HiliteTexture = Properties.GetPropertyAsOrDefault<int>("hilite-texture");
            this.Orientation = new Gem.Euler();
            Chunks = new Gem.Common.Grid3D<CompiledChunk>(World.width / 16, World.height / 16, World.depth / 16, (x,y,z) => new CompiledChunk());

            Chunks.forAll((m, x, y, z) => World.MarkDirtyBlock(new Coordinate(x * 16, y * 16, z * 16)));
        }

        public void UpdateGeometry()
        {
            foreach (var chunkCoordinate in World.DirtyChunks)
            {
                var chunk = Chunks[chunkCoordinate.X, chunkCoordinate.Y, chunkCoordinate.Z];

                chunk.Mesh = Generate.ChunkGeometry(World,
                    chunkCoordinate.X * 16, chunkCoordinate.Y * 16, chunkCoordinate.Z * 16, 16, 16, 16, Blocks);
                if (chunk.Mesh != null) chunk.Mesh.PrepareLineIndicies();

                // Todo: Need to look in neighboring chunks for lights and possibly update those neighboring chunks as well.
                chunk.Lights = new List<OrientedBlock>();
                World.forRect(chunkCoordinate.X * 16, chunkCoordinate.Y * 16, chunkCoordinate.Z * 16, 16, 16, 16,
                    (cell, x, y, z) =>
                    {
                        if (cell.Template != null && cell.Template.EmitsLight)
                            chunk.Lights.Add(cell);
                    });                
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
            Context.Device.DepthStencilState = DepthStencilState.Default;
            Context.Color = Vector3.One;
            Context.NormalMap = Context.NeutralNormals;
            Context.World = WorldTransform;


            if (WireFrameMode)
            {
                Context.Texture = Context.Black;
                Context.LightingEnabled = false;
                Context.ApplyChanges();
                Chunks.forAll((m, x, y, z) => Context.DrawLines(m.Mesh));
            }
            else
            {
                Context.Texture = Blocks.Tiles.Texture;
                Context.LightingEnabled = true;
                //Context.SetLight(0, SunPosition, float.PositiveInfinity, new Vector3(1.0f, 1.0f, 1.0f));
                //Context.ActiveLightCount = 1;


                // Todo: Frustrum cull chunks.
                Chunks.forAll((m, x, y, z) =>
                    {
                        var lightCount = 0;
                        for (int i = 0; i < m.Lights.Count && i < Context.MaximumLights; ++i)
                        {
                            Context.SetLight(lightCount, m.Lights[i].Offset.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f), 8.0f, Vector3.One);
                            lightCount += 1;
                        }

                        Context.ActiveLightCount = lightCount;
                        Context.ApplyChanges();

                            Context.Draw(m.Mesh);
                    });
            }

            if (MouseHover)
            {
                Context.LightingEnabled = false;

                if (PhantomPlacementMesh != null)
                {
                    Context.Alpha = 0.75f;
                    Context.Color = PhantomColor;
                    Context.ApplyChanges();
                    Context.Draw(PhantomPlacementMesh);
                    PhantomPlacementMesh = null;
                }

                Context.Device.DepthStencilState = DepthStencilState.None;

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
                if (World.CellAt(x, y, z).Template == null) return false;

                // Ray intersect cell geometry.
                var cell = World.CellAt(x, y, z);
                Gem.Geo.Mesh.RayIntersectionResult closestIntersection = null;
                Gem.Geo.Mesh hitFace = null;

                foreach (var face in Generate.EnumerateFaceMeshes(cell))
                {
                    var rayResult = face.RayIntersection(localMouse, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    if (rayResult.Intersects)
                    {
                        if (closestIntersection == null || (closestIntersection.Intersects && rayResult.Distance < closestIntersection.Distance))
                        {
                            hitFace = face;
                            closestIntersection = rayResult;
                            RealHoverNormal = -Gem.Geo.Gen.CalculateNormal(face, face.indicies[0], face.indicies[1], face.indicies[2]);
                            HitLocation = localMouse.Position + (localMouse.Direction * closestIntersection.Distance);
                        }
                    }
                }
                 
                if (closestIntersection == null) return false;

                // Must copy... because enumerating the faces did not.
                HiliteQuad = Gem.Geo.Gen.Copy(hitFace);
                Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z + 0.5f));

                //// Align quad with correct face.
                //if (normal.Z < 0) // normal points down. 
                //    Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateFromAxisAngle(Vector3.UnitX, Gem.Math.Angle.PI));
                //else if (normal.Z == 0)
                //    Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateFromAxisAngle(
                //        Vector3.Cross(-Vector3.UnitZ, normal), -Gem.Math.Angle.PI / 2));

                //// Move quad to center of block.
                //Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z + 0.5f));

                //// Move quad out to correct surface.
                //Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateTranslation(normal * 0.52f));

                //var faceIntersection = HiliteQuad.RayIntersection(localMouse);
                ////if (faceIntersection.Intersects == false) throw new InvalidProgramException();

                //HoverCallback(this, faceIntersection.Distance);
                HoverCallback(this, closestIntersection.Distance);

                HoverBlock = new Coordinate(x, y, z);
                HoverNormal = Vector3.Normalize(new Vector3(
                    (float)Math.Round(RealHoverNormal.X),
                    (float)Math.Round(RealHoverNormal.Y),
                    (float)Math.Round(RealHoverNormal.Z)));
                AdjacentHoverBlock = new Coordinate((int)(x + HoverNormal.X), (int)(y + HoverNormal.Y), (int)(z + HoverNormal.Z));                

                return true;
            });
        }
    }
}
