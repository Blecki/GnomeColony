using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class Build : Task
    {
        private BlockTemplate BlockType;
        WaitAction Progress = null;

        public Build(BlockTemplate BlockType, Coordinate Location) : base(Location)
        {
            this.BlockType = BlockType;
            MarkerTile = TileNames.TaskIconBlank;
            GnomeIcon = TileNames.TaskIconBuild;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (Object.ReferenceEquals(Game.World.CellAt(Location).Block, BlockType))
                return TaskStatus.Complete;
            if (!NoGnomesInArea(Game, Location)) return TaskStatus.Impossible;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            var excessResources = FindExcessResources(cell, this);
            var unfilledResources = FindUnfilledResourceRequirments(cell, this);

            // Move excess resources off this tile.

            if (unfilledResources.Count > 0)
                return new FillResourceNeed(this);

            return null;
        }

        public override IEnumerable<int> GetRequiredResources()
        {
            foreach (var resource in BlockType.ConstructionResources)
                yield return resource;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            Gnome.FacingDirection = CellLink.DirectionFromAToB(Gnome.Location, Location);

            if (Progress == null)
            {
                Progress = new WaitAction(2.0f);
                Gnome.NextAction = Progress;
            }
            else if (Progress.Done)
            {
                Progress = null;
                var cell = Game.World.CellAt(Location);
                cell.Block = BlockType;
                cell.Resources.Clear();
                Game.World.MarkDirtyBlock(Location);
            }
        }
    }
}
