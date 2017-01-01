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
        public int Icon { get; protected set; }
        public virtual void Apply(Simulation Sim, RenderModule.WorldSceneNode WorldNode) { }
        public virtual void Hover(Simulation Sim, RenderModule.WorldSceneNode WorldNode) { }
        public virtual void Selected(Simulation Sim, Gem.Gui.UIItem GuiRoot) { }
        public virtual void Deselected(Simulation Sim, Gem.Gui.UIItem GuiRoot) { }
    }
}
