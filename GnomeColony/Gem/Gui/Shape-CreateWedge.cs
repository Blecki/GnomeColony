using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Gem.Gui
{
    public partial class Shape
    {
        public static Shape CreateWedge(Vector2 Origin, float StartAngle, float EndAngle, float InnerRadius, float OuterRadius, int Segments)
        {
            var angleDelta = EndAngle - StartAngle;
            var angleStep = angleDelta / Segments;

            var stepMatrix = Matrix.CreateRotationZ(angleStep);
            var startVector = Vector2.Transform(new Vector2(0, 1), Matrix.CreateRotationZ(StartAngle));

            var result = new List<Shape>();

            for (int i = 0; i < Segments; ++i)
            {
                var nextVector = Vector2.Transform(startVector, stepMatrix);
                result.Add(new PolygonShape(
                    Origin + (startVector * InnerRadius),
                    Origin + (startVector * OuterRadius),
                    Origin + (nextVector * OuterRadius),
                    Origin + (nextVector * InnerRadius)
                    ));

                startVector = nextVector;
            }

            return new CompositeShape(result);
        }
    }
}
