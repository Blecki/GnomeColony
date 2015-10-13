using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome
{
    public enum TaskStatus
    {
        NotComplete,
        Complete,
        Impossible,
    }

    public class Task
    {
        public Coordinate Location;
        public int MarkerTile { get; protected set; }
        public Gnome AssignedGnome = null;

        public Task(Coordinate Location)
        {
            this.Location = Location;
        }
        
        public virtual TaskStatus QueryStatus(Game Game) { return TaskStatus.NotComplete; }
        public virtual bool QueryValidLocation(Game Game, Coordinate GnomeLocation) { return false; }
        public virtual Task Prerequisite(Game Game, Gnome Gnome) { return null; }
        public virtual BlockTemplate RequiredResource(Game Game) { return null; }
        public virtual void ExecuteTask(Game Game, Gnome Gnome) { }
        public virtual int GnomeIcon() { return 0; }

        protected static bool Adjacent(Coordinate A, Coordinate B)
        {
            if (A.X == B.X && A.Y == B.Y) return false;

            if (A.X == B.X)
            {
                if (A.Y < B.Y - 1 || A.Y > B.Y + 1) return false;
            }
            else if (A.Y == B.Y)
            {
                if (A.X < B.X - 1 || A.X > B.X + 1) return false;
            }
            else return false;

            if (A.Z < B.Z - 2 || A.Z > B.Z + 1) return false;

            return true;
        }

        protected static IEnumerable<Coordinate> EnumerateAdjacent(Coordinate A)
        {
            for (var z = A.Z - 1; z <= A.Z + 2; ++z)
            {
                yield return new Coordinate(A.X - 1, A.Y, z);
                yield return new Coordinate(A.X + 1, A.Y, z);
                yield return new Coordinate(A.X, A.Y - 1, z);
                yield return new Coordinate(A.X, A.Y + 1, z);
            }
        }
    }
}
