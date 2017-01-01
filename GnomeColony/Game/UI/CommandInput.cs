using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game.RenderModule;

namespace Game
{
    public class CommandInput : GuiInputState
    {
        private Simulation Sim;
        private BlockSet Blocks;
        private List<GuiTool> Tools;
        private GuiTool SelectedTool;
                
        public CommandInput(Simulation Sim, List<GuiTool> Tools)
        {
            this.Sim = Sim;
            this.Blocks = Sim.Blocks;
            this.Tools = Tools;
        }

        public override void EnterState(Game Game)
        {
            base.EnterState(Game);
            
            var y = 8;
            foreach (var tool in Tools)
            {
                var child = CreateGuiSprite(new Rectangle(8, y, 64, 64), tool.Icon, Blocks.Tiles);
                child.Properties[0].Values.Upsert("click-action", new Action(() =>
                    {
                        if (SelectedTool != null) SelectedTool.Deselected(Sim, GuiRoot.uiRoot);
                        SelectedTool = tool;
                        tool.Selected(Sim, GuiRoot.uiRoot);
                    }));
                GuiRoot.uiRoot.AddChild(child);
                y += 68;
            }

            SelectedTool = Tools[0];
        }
    
        public override void Update(Game Game)
        {
            if (!Game.Main.IsActive) return;
            base.Update(Game);

            if (Game.HoverNode is WorldSceneNode)
            {
                if (SelectedTool != null)
                {
                    var hoverNormal = (Game.HoverNode as WorldSceneNode).HoverNormal;
                    var hoverSide = GuiTool.HiliteFace.Sides;
                    if (hoverNormal.Z > 0) 
                        hoverSide = GuiTool.HiliteFace.Top;

                    if ((SelectedTool.HiliteFaces & hoverSide) == hoverSide)
                    {
                        Game.HoverNode.SetHover();
                        SelectedTool.Hover(Sim, Game.HoverNode as WorldSceneNode);
                        if (Game.Input.Check("LEFT-CLICK"))
                            SelectedTool.Apply(Sim, Game.HoverNode as WorldSceneNode);
                    }
                }
            }
        }
    }
}
