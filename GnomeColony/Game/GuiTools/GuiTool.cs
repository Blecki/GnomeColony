﻿using System;
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
        public virtual void Apply(Simulation Sim, WorldSceneNode WorldNode) { }

        public virtual void Selected(Game Game, Gem.Gui.UIItem GuiRoot) { }
        public virtual void Deselected(Game Game, Gem.Gui.UIItem GuiRoot) { }
    }
}