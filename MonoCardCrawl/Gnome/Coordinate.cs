using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Gnome
{
    public struct Coordinate
    {
        public int X;
        public int Y;
        public int Z;

        public Coordinate(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public Vector3 AsVector3()
        {
            return new Vector3(X, Y, Z);
        }
        
        public static bool operator==(Coordinate A, Coordinate B)
        {
            return A.X == B.X && A.Y == B.Y && A.Z == B.Z;
        }

        public static bool operator !=(Coordinate A, Coordinate B)
        {
            return A.X != B.X || A.Y != B.Y || A.Z != B.Z;
        }

    }
}
