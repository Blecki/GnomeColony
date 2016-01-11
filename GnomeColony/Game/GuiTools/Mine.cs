using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.GuiTools
{
    public class Mine : GuiTool
    {
        public Mine()
        {
            this.Icon = TileNames.TaskIconMine;
        }

        public override void Apply(Game Game, WorldSceneNode WorldNode)
        {
            if (Game.World.Check(WorldNode.HoverBlock))
            {
                Game.AddTask(new Tasks.Mine(WorldNode.HoverBlock));
            }
        }
    }
}
