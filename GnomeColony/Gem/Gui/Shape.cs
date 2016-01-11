using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Gem.Gui
{
    public abstract partial class Shape
    {
        public virtual bool PointInside(Vector2 Point) { return false; }
        public virtual void Render(Gem.Render.RenderContext Context) { }
        public virtual Shape Transform(Matrix M) { return this; }

        public static Shape CreateQuad(float X, float Y, float W, float H)
        {
            return new PolygonShape(
                new Vector2(X, Y),
                new Vector2(X + W, Y),
                new Vector2(X + W, Y + H),
                new Vector2(X, Y + H)
            );
        }

        public static Shape CreateSprite(float X, float Y, float W, float H)
        {
            return new SpriteShape(
                new Vector2(X, Y),
                new Vector2(X + W, Y),
                new Vector2(X + W, Y + H),
                new Vector2(X, Y + H)
            );
        }
    }

    public class CompositeShape : Shape
    {
        private Shape[] Children;

        public CompositeShape(params Shape[] Children)
        {
            this.Children = Children;
        }

        public CompositeShape(IEnumerable<Shape> Children)
        {
            this.Children = Children.ToArray();
        }

        public override bool PointInside(Vector2 Point)
        {
            foreach (var child in Children)
                if (child.PointInside(Point)) return true;
            return false;
        }

        public override void Render(Render.RenderContext Context)
        {
            foreach (var child in Children)
                child.Render(Context);
        }

        public override Shape Transform(Matrix M)
        {
            return new CompositeShape(Children.Select(c => c.Transform(M)));
        }
    }

    public class PolygonShape : Shape
    {
        private Vector2[] Points;

        public PolygonShape(params Vector2[] Points)
        {
            this.Points = Points;
        }

        public PolygonShape(IEnumerable<Vector2> Points)
        {
            this.Points = Points.ToArray();
        }

        public override bool PointInside(Vector2 Point)
        {
            return Gem.Math.Intersection.PointInPolygonAngle(Points, Point);
        }

        public override void Render(Render.RenderContext Context)
        {
            Context.ImmediateMode.Polygon(Points, Points);
        }

        public override Shape Transform(Matrix M)
        {
            return new PolygonShape(Points.Select(p => Vector2.Transform(p, M)));
        }
    }

    public class SpriteShape : Shape
    {
        private Vector2[] Points;
        private Vector2[] TextureCoords;

        public SpriteShape(params Vector2[] Points)
        {
            this.Points = Points;
            this.TextureCoords = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
        }

        public SpriteShape(IEnumerable<Vector2> Points)
        {
            this.Points = Points.ToArray();
        }

        public override bool PointInside(Vector2 Point)
        {
            return Gem.Math.Intersection.PointInPolygonAngle(Points, Point);
        }

        public override void Render(Render.RenderContext Context)
        {
            Context.ImmediateMode.Polygon(Points, TextureCoords);
        }

        public override Shape Transform(Matrix M)
        {
            return new PolygonShape(Points.Select(p => Vector2.Transform(p, M)));
        }
    }
}
