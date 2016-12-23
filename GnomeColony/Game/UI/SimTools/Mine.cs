using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.RenderModule;

namespace Game.GuiTools
{
    public class Mine : GuiTool
    {
        public Mine()
        {
            this.Icon = TileNames.TaskIconMine;
        }

        public override void Apply(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (Sim.World.Check(WorldNode.HoverBlock))
            {
                Sim.AddTask(new Tasks.Mine(WorldNode.HoverBlock));
            }
        }
    }
}
