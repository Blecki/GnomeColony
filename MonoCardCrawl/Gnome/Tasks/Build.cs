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

        public Build(BlockTemplate BlockType, Coordinate Location) : base(Location)
        {
            this.BlockType = BlockType;
            MarkerTile = 1;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (Object.ReferenceEquals(Game.World.CellAt(Location).Block, BlockType))
                return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override BlockTemplate RequiredResource(Game Game)
        {
            return BlockType;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            if (!cell.Resource.Filled) return new FillResourceNeed(BlockType, Location);
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            cell.Block = BlockType;
            cell.Resource = null;
            Game.World.MarkDirtyBlock(Location);
        }

        public override int GnomeIcon()
        {
            return 482;
        }
    }
}
