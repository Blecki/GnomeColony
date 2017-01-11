using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class GuiTool
    {
        public GuiTool()
        {

        }
        
        public enum HiliteFace
        {
            Top = 1,
            Sides = 2
        }

        public HiliteFace HiliteFaces = HiliteFace.Top | HiliteFace.Sides;

        public virtual void OnSelected(Game Game) { }
        public virtual void OnDeselected(Game Game) { }
        public virtual void Apply(Simulation Sim, WorldRenderer WorldNode) { }
        public virtual void Hover(Simulation Sim, WorldRenderer WorldNode) { }
        public virtual void UnHover() { }
    }
}
