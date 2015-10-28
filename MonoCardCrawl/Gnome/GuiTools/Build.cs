using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.GuiTools
{
    public class Build : GuiTool
    {
        public Build()
        {
            this.Icon = TileNames.TaskIconBuild;
        }

        public override void Apply(Game Game, WorldSceneNode WorldNode)
        {
            if (Game.World.Check(WorldNode.AdjacentHoverBlock))
            {
                Game.AddTask(new Tasks.Build(Game.BlockTemplates[1], WorldNode.AdjacentHoverBlock));
                var cell = Game.World.CellAt(WorldNode.AdjacentHoverBlock);
                if (cell.Block != null) return;
                cell.Block = Game.BlockTemplates[BlockTypes.Scaffold];
            }
        }
    }
}
