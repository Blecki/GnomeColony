using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    /// <summary>
    /// Represent all the transformations that can be applied to an object.
    /// </summary>
    public class Euler
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Quaternion Orientation = Quaternion.Identity;

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(Scale)
                    * Matrix.CreateFromQuaternion(Orientation)
                    * Matrix.CreateTranslation(Position);
            }
        }

        public void SetFromMatrix(Matrix M)
        {
            M.Decompose(out Scale, out Orientation, out Position);
        }
    }
}
