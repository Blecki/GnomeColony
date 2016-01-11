using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public static class GridTraversal
    {
        /**
         * Call the callback with (position, face) of all blocks along the line
         * segment from point 'origin' in vector direction 'direction' of length
         * 'radius'. 'radius' may be infinite, but beware infinite loop in this case.
         *
         * 'face' is the normal vector of the face of that block that was entered.
         *
         * If the callback returns a true value, the traversal will be stopped.
         */
        public static void Raycast(
            Vector3 origin,
            Vector3 direction,
            float radius,
            Func<int, int, int, Vector3, bool> callback)
        {
            // From "A Fast Voxel Traversal Algorithm for Ray Tracing"
            // by John Amanatides and Andrew Woo, 1987
            // <http://www.cse.yorku.ca/~amana/research/grid.pdf>
            // <http://citeseer.ist.psu.edu/viewdoc/summary?doi=10.1.1.42.3443>
            // Extensions to the described algorithm:
            //   • Imposed a distance limit.
            //   • The face passed through to reach the current cube is provided to
            //     the callback.

            // The foundation of this algorithm is a parameterized representation of
            // the provided ray,
            //                    origin + t * direction,
            // except that t is not actually stored; rather, at any given point in the
            // traversal, we keep track of the *greater* t values which we would have
            // if we took a step sufficient to cross a cube boundary along that axis
            // (i.e. change the integer part of the coordinate) in the variables
            // tMaxX, tMaxY, and tMaxZ.

            // Cube containing origin point.
            var x = FloorToInt(origin.X);
            var y = FloorToInt(origin.Y);
            var z = FloorToInt(origin.Z);

            // Break out direction vector.
            var dx = direction.X;
            var dy = direction.Y;
            var dz = direction.Z;

            // Direction to increment x,y,z when stepping.
            var stepX = signum(dx);
            var stepY = signum(dy);
            var stepZ = signum(dz);

            // See description above. The initial values depend on the fractional
            // part of the origin.
            var tMaxX = intbound(origin.X, dx);
            var tMaxY = intbound(origin.Y, dy);
            var tMaxZ = intbound(origin.Z, dz);

            // The change in t when taking a step (always positive).
            var tDeltaX = stepX / dx;
            var tDeltaY = stepY / dy;
            var tDeltaZ = stepZ / dz;

            // Buffer for reporting faces to the callback.
            var face = new Vector3();

            // Avoids an infinite loop.
            if (dx == 0 && dy == 0 && dz == 0)
                throw new Exception("Ray-cast in zero direction!");

            // Rescale from units of 1 cube-edge to units of 'direction' so we can
            // compare with 't'.
            radius /= (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

            while (true)
            {
                // Invoke the callback, unless we are not *yet* within the bounds of the
                // world.
                if (callback(x, y, z, face))
                    break;

                // tMaxX stores the t-value at which we cross a cube boundary along the
                // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
                // chooses the closest cube boundary. Only the first case of the four
                // has been commented in detail.
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        if (tMaxX > radius)
                            break;
                        // Update which cube we are now in.
                        x += stepX;
                        // Adjust tMaxX to the next X-oriented boundary crossing.
                        tMaxX += tDeltaX;
                        // Record the normal vector of the cube face we entered.
                        face = new Vector3(-stepX, 0, 0);
                    }
                    else
                    {
                        if (tMaxZ > radius)
                            break;
                        z += stepZ;
                        tMaxZ += tDeltaZ;
                        face = new Vector3(0, 0, -stepZ);
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        if (tMaxY > radius)
                            break;
                        y += stepY;
                        tMaxY += tDeltaY;
                        face = new Vector3(0, -stepY, 0);
                    }
                    else
                    {
                        // Identical to the second case, repeated for simplicity in
                        // the conditionals.
                        if (tMaxZ > radius)
                            break;
                        z += stepZ;
                        tMaxZ += tDeltaZ;
                        face = new Vector3(0, 0, -stepZ);
                    }
                }
            }
        }

        private static float intbound(float s, float ds)
        {
            // Some kind of edge case, see:
            // http://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game#comment160436_49423
            var sIsInteger = Math.Round(s) == s;
            if (ds < 0 && sIsInteger)
                return 0;

            return (float)((ds > 0 ? Math.Ceiling(s) - s : s - Math.Floor(s)) / Math.Abs(ds));
        }

        private static int signum(float x)
        {
            return x > 0 ? 1 : x < 0 ? -1 : 0;
        }

        private static int FloorToInt(float n)
        {
            return (int)Math.Floor(n);
        }
    }
}
