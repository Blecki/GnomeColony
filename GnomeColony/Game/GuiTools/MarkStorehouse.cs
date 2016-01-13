using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.GuiTools
{
    public class MarkStorehouse : GuiTool
    {
        public MarkStorehouse()
        {
            this.Icon = TileNames.TaskIconMarkStorehouse;
            this.HiliteFaces = HiliteFace.Top;
        }

        public override void Apply(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (Sim.World.Check(WorldNode.HoverBlock))
            {
                var cell = Sim.World.CellAt(WorldNode.HoverBlock);
                cell.SetFlag(CellFlags.Storehouse, true);
                Sim.SetUpdateFlag(WorldNode.HoverBlock);
            }
        }
    }
}
