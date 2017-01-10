using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Game
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

        public static Coordinate operator+(Coordinate A, Coordinate B)
        {
            return new Coordinate(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
        }

        private static int __abs(int I)
        {
            if (I < 0) return I * -1;
            return I;
        }

        public static bool Adjacent(Coordinate A, Coordinate B)
        {
            if (A.X == B.X && A.Y == B.Y && (__abs(A.Z - B.Z) == 1)) return true;
            if (A.X == B.X && (__abs(A.Y - B.Y) == 1) && A.Z == B.Z) return true;
            if ((__abs(A.X - B.X) == 1) && A.Y == B.Y && A.Z == B.Z) return true;
            return false;
        }
    }
}
