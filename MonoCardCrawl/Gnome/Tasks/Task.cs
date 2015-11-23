﻿using System;
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
        public int GnomeIcon { get; protected set; }
        public Gnome AssignedGnome = null;
        public bool IsTopLevelTask = false;

        public Task(Coordinate Location)
        {
            this.Location = Location;

            MarkerTile = 0;
            GnomeIcon = TileNames.TaskIconBlank;
        }
        
        public virtual TaskStatus QueryStatus(Game Game) { return TaskStatus.NotComplete; }
        public virtual bool QueryValidLocation(Game Game, Coordinate GnomeLocation) { return false; }
        public virtual Task Prerequisite(Game Game, Gnome Gnome) { return null; }
        public virtual IEnumerable<int> GetRequiredResources() { return Enumerable.Empty<int>(); }
        public virtual void ExecuteTask(Game Game, Gnome Gnome) { }

        public static bool ResourceRequirmentsMet(Cell Cell, Task Task)
        {
            var taskRequirements = Task.GetRequiredResources();
            var cellResources = Cell.Resources;

            if (taskRequirements.Count() != cellResources.Count) return false;

            var compared = taskRequirements.OrderBy(b => b).Zip(cellResources.OrderBy(b => b), (a, b) => a == b);
            if (compared.Count(b => b == false) != 0) return false;

            return true;
        }

        public static List<int> FindUnfilledResourceRequirments(Cell Cell, Task Task)
        {
            var taskRequirements = Task.GetRequiredResources().ToList();
            var cellResources = Cell.Resources;

            foreach (var item in cellResources)
                if (taskRequirements.Contains(item))
                    taskRequirements.Remove(item);

            return taskRequirements;
        }

        public static List<int> FindExcessResources(Cell Cell, Task Task)
        {
            var taskRequirements = Task.GetRequiredResources();
            var cellResources = new List<int>(Cell.Resources);

            foreach (var item in taskRequirements)
                if (cellResources.Contains(item))
                    cellResources.Remove(item);

            return cellResources;
        }

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

        public static bool NoGnomesInArea(Game Game, Coordinate Location)
        {
            for (var z = Location.Z - 1; z <= Location.Z + 3; ++z)
                if (Game.World.check(Location.X, Location.Y, z) && Game.World.CellAt(Location.X, Location.Y, z).PresentActor != null) return false;
            return true;
        }
    }
}
