using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.RenderModule;

namespace Game.Creative
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
                var cell = Sim.World.CellAt(WorldNode.HoverBlock);
                cell.Block = null;
                Sim.SetUpdateFlag(WorldNode.HoverBlock);
            }
        }
    }
}
