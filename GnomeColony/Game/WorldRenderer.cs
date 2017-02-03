using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    

    public class WorldRenderer
    {
        public class CompiledChunk
        {
            public Gem.Geo.Mesh Mesh;
            public List<OrientedBlock> Lights;
        }

        public bool WireFrameMode = false;
        public bool DebugShadersMode = false;
        public bool DebugCubeMapMode = false;
        
        private CellGrid World;
        private Gem.Common.Grid3D<CompiledChunk> Chunks;
        
        public int HiliteTexture;
        public float PickRadius = 100.0f;

        public bool MouseHover { get; private set; }
        private Gem.Geo.Mesh HiliteQuad;

        public Gem.Geo.Mesh PhantomPlacementMesh = null;
        public Vector3 PhantomColor = new Vector3(1, 1, 1);

        public GraphicsDevice Device;
        private Effect MainEffect;
        private Effect ClearGBufferEffect;
        private Effect RenderGBufferEffect;
        private Effect PointLightEffect;
        private Effect FinalCombineEffect;
        private Effect QuickDrawEffect;
        private Effect DrawLightShadowEffect;
        private Vector2 HalfPixel;
        private RenderTarget2D ColorRT;
        private RenderTarget2D NormalRT;
        private RenderTarget2D DepthRT;
        private RenderTarget2D LightRT;
        public RenderTargetCube ShadowRT;
        private Gem.Geo.Mesh Quad;
        private Gem.Geo.Mesh LightGeometry;
        private Gem.Geo.Mesh DebugCube;
        private Texture2D TileNormals;


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
        public Texture2D Black { get; private set; }
        public Texture2D NeutralNormals { get; private set; }

        public Vector3 SunPosition = new Vector3(0, 100.0f, 1000.0f);

        public WorldRenderer(
            GraphicsDevice Device, 
            Microsoft.Xna.Framework.Content.ContentManager Content,
            CellGrid World, BlockSet Blocks, int HiliteTexture)
        {
            this.Device = Device;
            this.World = World;
            this.Blocks = Blocks;
            this.HiliteTexture = HiliteTexture;
            
             MainEffect = Content.Load<Effect>("Content/draw");
             ClearGBufferEffect = Content.Load<Effect>("Content/clear_gbuffer");
             RenderGBufferEffect = Content.Load<Effect>("Content/draw_voxels_gbuffer");
             PointLightEffect = Content.Load<Effect>("Content/draw_point_light");
             FinalCombineEffect = Content.Load<Effect>("Content/combine_gbuffer");
             QuickDrawEffect = Content.Load<Effect>("Content/draw_quick");
             DrawLightShadowEffect = Content.Load<Effect>("Content/draw_voxels_shadow");

            Chunks = new Gem.Common.Grid3D<CompiledChunk>(World.width / 16, World.height / 16, World.depth / 16, (x,y,z) => new CompiledChunk());

            Chunks.forAll((m, x, y, z) => World.MarkDirtyBlock(new Coordinate(x * 16, y * 16, z * 16)));

            MouseHover = false;

            Black = new Texture2D(Device, 1, 1, false, SurfaceFormat.Color);
            Black.SetData(new Color[] { new Color(0, 0, 0, 255) });

            // Todo: Create actual normal map and spec map.
            NeutralNormals = new Texture2D(Device, 1, 1, false, SurfaceFormat.Color);
            NeutralNormals.SetData(new Color[] { new Color(128, 128, 255, 255) });

            HalfPixel = new Vector2()
            {
                X = 0.5f / (float)Device.Viewport.Width,
                Y = 0.5f / (float)Device.Viewport.Height
            };

            int backbufferWidth = Device.Viewport.Width;
            int backbufferHeight = Device.Viewport.Height;

            ColorRT = new RenderTarget2D(Device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            NormalRT = new RenderTarget2D(Device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            DepthRT = new RenderTarget2D(Device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            LightRT = new RenderTarget2D(Device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            ShadowRT = new RenderTargetCube(Device, 512, false, SurfaceFormat.Single, DepthFormat.Depth24);

            Quad = Gem.Geo.Gen.CreateSpriteQuad();

            var ico = Gem.Geo.Ico.Icosahedron.Generate();
            ico = ico.Subdivide();
            LightGeometry = ico.GenerateMesh(1.0f);

            DebugCube = Gem.Geo.Gen.CreateCube();
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

        private void SetGBuffer()
        {
            Device.SetRenderTargets(ColorRT, NormalRT, DepthRT);
        }

        private void ResolveGBuffer()
        {
            Device.SetRenderTarget(null);
        }

        private void ClearGBuffer()
        {
            Device.DepthStencilState = DepthStencilState.None;
            ClearGBufferEffect.Techniques[0].Passes[0].Apply();
            Quad.Render(Device);
        }

        public void Draw(Gem.Render.ICamera Camera)
        {
            UpdateGeometry();

            if (WireFrameMode)
            {
                Device.BlendState = BlendState.Opaque;
                Device.DepthStencilState = DepthStencilState.Default;
                Device.RasterizerState = RasterizerState.CullCounterClockwise;

                Device.Clear(Color.CornflowerBlue);

                QuickDrawEffect.Parameters["Texture"].SetValue(Black);
                QuickDrawEffect.Parameters["View"].SetValue(Camera.View);
                QuickDrawEffect.Parameters["Projection"].SetValue(Camera.Projection);
                QuickDrawEffect.Parameters["World"].SetValue(Matrix.Identity);
                QuickDrawEffect.CurrentTechnique = QuickDrawEffect.Techniques[0];
                QuickDrawEffect.CurrentTechnique.Passes[0].Apply();

                Chunks.forAll((m, x, y, z) =>
                {
                    if (m.Mesh != null)
                    {
                        if (m.Mesh.lineIndicies == null) m.Mesh.PrepareLineIndicies();

                        if (m.Mesh.lineIndicies != null && m.Mesh.verticies.Length > 0)
                            Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, m.Mesh.verticies, 0,
                                m.Mesh.verticies.Length,
                                m.Mesh.lineIndicies, 0, m.Mesh.lineIndicies.Length / 2);
                    }
                });

                return;
            }

            SetGBuffer();
            ClearGBuffer();

            Device.BlendState = BlendState.Opaque;
            Device.DepthStencilState = DepthStencilState.Default;
            Device.RasterizerState = RasterizerState.CullCounterClockwise;

            RenderGBufferEffect.Parameters["View"].SetValue(Camera.View);
            RenderGBufferEffect.Parameters["Projection"].SetValue(Camera.Projection);
            RenderGBufferEffect.Parameters["World"].SetValue(Matrix.Identity);
            RenderGBufferEffect.Parameters["specularIntensity"].SetValue(0.8f);
            RenderGBufferEffect.Parameters["specularPower"].SetValue(0.5f);
            RenderGBufferEffect.Parameters["SpecularMap"].SetValue(Black);

            // Todo: Draw proper normal map.
            RenderGBufferEffect.Parameters["NormalMap"].SetValue(Blocks.Tiles.Texture);
            RenderGBufferEffect.Parameters["Texture"].SetValue(Blocks.Tiles.Texture);
            RenderGBufferEffect.Parameters["DiffuseColor"].SetValue(Vector3.One);

            RenderGBufferEffect.Techniques[0].Passes[0].Apply();

            // Todo: Frustrum cull chunks.
            Chunks.forAll((m, x, y, z) =>
                {
                    if (m.Mesh != null)
                        m.Mesh.Render(Device);
                });

            if (MouseHover)
            {
                if (PhantomPlacementMesh != null)
                {
                    RenderGBufferEffect.Parameters["DiffuseColor"].SetValue(PhantomColor);

                    RenderGBufferEffect.Techniques[0].Passes[0].Apply();

                    PhantomPlacementMesh.Render(Device);
                    PhantomPlacementMesh = null;

                    RenderGBufferEffect.Parameters["DiffuseColor"].SetValue(Vector3.One);
                }

                if (HiliteQuad != null)
                {
                    Device.DepthStencilState = DepthStencilState.None;
                    RenderGBufferEffect.Techniques[0].Passes[0].Apply();
                    HiliteQuad.Render(Device);
                }
            }

            ResolveGBuffer();

            Device.SetRenderTarget(LightRT);
            Device.Clear(Color.Transparent);

            Chunks.forAll((c, x, y, z) =>
                {
                    foreach (var light in c.Lights)
                        DrawPointLight(Camera, light.Offset.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f),
                            Color.White, 8.0f, 10.0f);
                });

            Device.BlendState = BlendState.Opaque;
            Device.DepthStencilState = DepthStencilState.None;
            Device.RasterizerState = RasterizerState.CullCounterClockwise;

            Device.SetRenderTarget(null);

            //Device.Clear(Color.CornflowerBlue);

            if (DebugCubeMapMode)
            {
                Device.Clear(Color.CornflowerBlue);
                QuickDrawEffect.Parameters["View"].SetValue(Camera.View);
                QuickDrawEffect.Parameters["Projection"].SetValue(Camera.Projection);
                QuickDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(Camera.GetPosition() + Camera.GetEyeVector() * 4.0f));
                QuickDrawEffect.Parameters["Texture"].SetValue(ShadowRT);
                QuickDrawEffect.Techniques[1].Passes[0].Apply();
                DebugCube.Render(Device);
                return;
            }

            if (DebugShadersMode)
            {
                QuickDrawEffect.Parameters["View"].SetValue(Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.UnitY));
                QuickDrawEffect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(-1, 1, -1, 1, -32, 32));
                QuickDrawEffect.Parameters["World"].SetValue(Matrix.Identity);
                QuickDrawEffect.Parameters["Texture"].SetValue(NormalRT);

                QuickDrawEffect.Techniques[0].Passes[0].Apply();
                Quad.Render(Device);

                QuickDrawEffect.Parameters["Texture"].SetValue(DepthRT);
                QuickDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(-1, 0, 0));

                QuickDrawEffect.Techniques[0].Passes[0].Apply();
                Quad.Render(Device);

                QuickDrawEffect.Parameters["Texture"].SetValue(ColorRT);
                QuickDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(-1, -1, 0));

                QuickDrawEffect.Techniques[0].Passes[0].Apply();
                Quad.Render(Device);

                QuickDrawEffect.Parameters["Texture"].SetValue(LightRT);
                QuickDrawEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(0, -1, 0));

                QuickDrawEffect.Techniques[0].Passes[0].Apply();
                Quad.Render(Device);

                return;
            }

            FinalCombineEffect.Parameters["World"].SetValue(Matrix.CreateScale(2.0f) * Matrix.CreateTranslation(-1.0f, -1.0f, 0.0f));
            FinalCombineEffect.Parameters["colorMap"].SetValue(ColorRT);
            FinalCombineEffect.Parameters["lightMap"].SetValue(LightRT);
            FinalCombineEffect.Parameters["AmbientLight"].SetValue(new Vector3(0.2f, 0.2f, 0.2f));
         
            FinalCombineEffect.Techniques[0].Passes[0].Apply();
            Quad.Render(Device);
        }

        private void DrawOmniShadow(int Face, Matrix View, Matrix Projection, float LightRadius)
        {
            // Draw shadow depth
            Device.SetRenderTarget(ShadowRT, (CubeMapFace)Face);

            Device.DepthStencilState = DepthStencilState.None;
            DrawLightShadowEffect.Techniques[1].Passes[0].Apply();
            Quad.Render(Device);

            Device.BlendState = BlendState.Opaque;
            Device.DepthStencilState = DepthStencilState.Default;

            DrawLightShadowEffect.Parameters["View"].SetValue(View);
            DrawLightShadowEffect.Parameters["World"].SetValue(Matrix.Identity);
            DrawLightShadowEffect.Parameters["Projection"].SetValue(Projection);
            DrawLightShadowEffect.Parameters["Texture"].SetValue(Blocks.Tiles.Texture);
           
            DrawLightShadowEffect.Techniques[0].Passes[0].Apply();

            // Todo: Only draw chunks that are within depth range.
            Chunks.forAll((m, x, y, z) =>
            {
                if (m.Mesh != null)
                    m.Mesh.Render(Device);
            });

            Device.SetRenderTarget(null);
        }

        private void DrawPointLight(Gem.Render.ICamera Camera, Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {
            //var views = new Matrix[6];
            //views[0] = Matrix.CreateLookAt(lightPosition, lightPosition - Vector3.UnitX, -Vector3.UnitY);
            //views[1] = Matrix.CreateLookAt(lightPosition, lightPosition + Vector3.UnitX, -Vector3.UnitY);
            //views[2] = Matrix.CreateLookAt(lightPosition, lightPosition - Vector3.UnitY, Vector3.UnitZ);
            //views[3] = Matrix.CreateLookAt(lightPosition, lightPosition + Vector3.UnitY, -Vector3.UnitZ);
            //views[4] = Matrix.CreateLookAt(lightPosition, lightPosition - Vector3.UnitZ, -Vector3.UnitX);
            //views[5] = Matrix.CreateLookAt(lightPosition, lightPosition + Vector3.UnitZ, Vector3.UnitX);

            //var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 0.1f, lightRadius);

            //for (var i = 0; i < 6; ++i)
            //    DrawOmniShadow(i, views[i], projection, lightRadius);

            Device.SetRenderTarget(LightRT);
            Device.BlendState = BlendState.AlphaBlend;
            Device.DepthStencilState = DepthStencilState.None;
            
            PointLightEffect.Parameters["colorMap"].SetValue(ColorRT);
            PointLightEffect.Parameters["normalMap"].SetValue(NormalRT);
            PointLightEffect.Parameters["depthMap"].SetValue(DepthRT);
            //PointLightEffect.Parameters["shadowMap"].SetValue(ShadowRT);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);

            PointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            PointLightEffect.Parameters["View"].SetValue(Camera.View);
            PointLightEffect.Parameters["Projection"].SetValue(Camera.Projection);
            
            //PointLightEffect.Parameters["World"].SetValue(Matrix.CreateScale(2.0f) * Matrix.CreateTranslation(-1.0f, -1.0f, 0.0f));//Matrix.CreateTranslation(-0.5f, -0.5f, 0.0f) * Matrix.CreateScale(2.0f));
            //PointLightEffect.Parameters["View"].SetValue(Matrix.Identity);
            //PointLightEffect.Parameters["Projection"].SetValue(Matrix.Identity);
            ////light position
            PointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            //set the color, radius and Intensity
            PointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            PointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            PointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            //parameters for specular computations
            PointLightEffect.Parameters["cameraPosition"].SetValue(Camera.GetPosition());
            PointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.View *
                Camera.Projection));
            //size of a halfpixel, for texture coordinates alignment
            //PointLightEffect.Parameters["halfPixel"].SetValue(HalfPixel);
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(Camera.GetPosition(), lightPosition);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                Device.RasterizerState = RasterizerState.CullNone;
            else
                Device.RasterizerState = RasterizerState.CullClockwise;

            Device.DepthStencilState = DepthStencilState.None;

            PointLightEffect.Techniques[0].Passes[0].Apply();
            
            //Quad.Render(Device);
            LightGeometry.Render(Device);

            Device.RasterizerState = RasterizerState.CullCounterClockwise;
            Device.DepthStencilState = DepthStencilState.Default;
        }

        public void CalculateLocalMouse(Microsoft.Xna.Framework.Ray MouseRay, GuiTool.HiliteFace HiliteFaces)
        {
            MouseHover = false;

            GridTraversal.Raycast(MouseRay.Position, MouseRay.Direction, PickRadius, (x, y, z, normal) =>
            {
                if (!World.check(x, y, z)) return false;
                if (World.CellAt(x, y, z).Template == null) return false;

                // Ray intersect cell geometry.
                var cell = World.CellAt(x, y, z);
                Gem.Geo.Mesh.RayIntersectionResult closestIntersection = null;
                Gem.Geo.Mesh hitFace = null;

                foreach (var face in Generate.EnumerateFaceMeshes(cell))
                {
                    var rayResult = face.RayIntersection(MouseRay, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    if (rayResult.Intersects)
                    {
                        if (closestIntersection == null || (closestIntersection.Intersects && rayResult.Distance < closestIntersection.Distance))
                        {
                            hitFace = face;
                            closestIntersection = rayResult;
                            RealHoverNormal = -Gem.Geo.Gen.CalculateNormal(face, face.indicies[0], face.indicies[1], face.indicies[2]);
                            HitLocation = MouseRay.Position + (MouseRay.Direction * closestIntersection.Distance);
                        }
                    }
                }

                if (closestIntersection == null) return false;

                // Must copy... because enumerating the faces did not.
                HiliteQuad = Gem.Geo.Gen.Copy(hitFace);
                Gem.Geo.Gen.Transform(HiliteQuad, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z + 0.5f));
                var hiliteMatrix = Blocks.Tiles.TileMatrix(HiliteTexture);
                Gem.Geo.Gen.MorphEx(HiliteQuad, v =>
                    {
                        v.TextureCoordinate = Vector2.Transform(v.TextureCoordinate, hiliteMatrix);
                        return v;
                    });

                HoverBlock = new Coordinate(x, y, z);
                HoverNormal = Vector3.Normalize(new Vector3(
                    (float)Math.Round(RealHoverNormal.X),
                    (float)Math.Round(RealHoverNormal.Y),
                    (float)Math.Round(RealHoverNormal.Z)));
                AdjacentHoverBlock = new Coordinate((int)(x + HoverNormal.X), (int)(y + HoverNormal.Y), (int)(z + HoverNormal.Z));

                var hoverSide = GuiTool.HiliteFace.Sides;
                if (HoverNormal.Z > 0)
                    hoverSide = GuiTool.HiliteFace.Top;

                if ((HiliteFaces & hoverSide) == hoverSide)
                    MouseHover = true;

                return true;
            });
        }
    }
}
