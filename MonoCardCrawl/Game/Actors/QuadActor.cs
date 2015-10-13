using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game.Actors
{
    public class QuadActorPropertyBag : Gem.PropertyBag
    {
        public Texture2D Sprite
        {
            get { return GetPropertyAs<Texture2D>("sprite"); }
            set { Upsert("sprite", value); }
        }

        public Texture2D NormalMap
        {
            get { return GetPropertyAs<Texture2D>("normal-map"); }
            set { Upsert("normal-map", value); }
        }

        public Texture2D DropShadow
        {
            get { return GetPropertyAs<Texture2D>("drop-shadow"); }
            set { Upsert("drop-shadow", value); }
        }

        public float Width
        {
            get { return GetPropertyAs<float>("width", () => 1); }
            set { Upsert("width", value); }
        }

        public float Height
        {
            get { return GetPropertyAs<float>("height", () => 1); }
            set { Upsert("height", value); }
        }
    }

    public class QuadActor : Actor
    {
        protected Gem.Euler ShadowOrientation = new Gem.Euler();
        protected Gem.Render.MeshNode ShadowNode;
        protected Gem.Render.MeshNode MeshNode;

        public override void Create(Gem.PropertyBag Properties)
        {
            base.Create(Properties);

            var branch = new Gem.Render.BranchNode();

            var Mesh = Gem.Geo.Gen.CreateQuad();
            //Gem.Geo.Gen.Transform(Mesh, Matrix.CreateRotationZ((float)Math.PI));
            Gem.Geo.Gen.Transform(Mesh, Matrix.CreateTranslation(0, 0.5f, 0));
            Gem.Geo.Gen.Transform(Mesh, Matrix.CreateScale(Properties.GetPropertyAs<float>("width"), Properties.GetPropertyAs<float>("height"), 1));
            Gem.Geo.Gen.Transform(Mesh, Matrix.CreateRotationX((float)(Math.PI / 8) * 3));
            Mesh = Gem.Geo.Gen.FacetCopy(Mesh);
            Gem.Geo.Gen.CalculateTangentsAndBiNormals(Mesh);
            
            var shadowMesh = Gem.Geo.Gen.CreateQuad();
            shadowMesh = Gem.Geo.Gen.FacetCopy(shadowMesh);
            Gem.Geo.Gen.CalculateTangentsAndBiNormals(shadowMesh);
            
            ShadowNode = new Gem.Render.MeshNode(shadowMesh, Properties.GetPropertyAs<Texture2D>("drop-shadow"), null, ShadowOrientation);
            ShadowNode.InteractWithMouse = false;
            MeshNode = new Gem.Render.MeshNode(Mesh, Properties.GetPropertyAs<Texture2D>("sprite"), Properties.GetPropertyAs<Texture2D>("normal-map"), Orientation);

            branch.Add(ShadowNode);
            branch.Add(MeshNode);

            Renderable = branch;

            MeshNode.AlphaMouse = true;
        }

        public override void Update(World World, float ElapsedSeconds)
        {
            base.Update(World, ElapsedSeconds);
            ShadowOrientation.Position = Orientation.Position;
        }
    }
}
