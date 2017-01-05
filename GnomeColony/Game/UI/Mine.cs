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
        }

        public override void Apply(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (Sim.World.Check(WorldNode.HoverBlock))
            {
                var cell = Sim.World.CellAt(WorldNode.HoverBlock);
                cell.Template = null;
                Sim.SetUpdateFlag(WorldNode.HoverBlock);
            }
        }
    }
}
