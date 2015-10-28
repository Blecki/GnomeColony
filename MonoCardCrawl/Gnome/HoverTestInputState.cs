using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnome
{
    public class HoverTest : InputState
    {
        private Gem.Gui.GuiSceneNode GuiRoot;
        private BlockTemplateSet BlockTemplates;
        private TileSheet TileSheet;
        private List<GuiTool> Tools;
        private GuiTool SelectedTool;
                
        public HoverTest(BlockTemplateSet BlockTemplates, TileSheet TileSheet, List<GuiTool> Tools)
        {
            this.BlockTemplates = BlockTemplates;
            this.TileSheet = TileSheet;
            this.Tools = Tools;
        }

        private static Gem.Gui.UIItem CreateGuiSprite(Rectangle Position, int TileIndex, TileSheet TileSheet)
        {
            return new Gem.Gui.UIItem(
                Gem.Gui.Shape.CreateSprite(Position.X, Position.Y, Position.Width, Position.Height),
                new Gem.Gui.GuiProperties
                {
                    Image = TileSheet.Texture,
                    ImageTransform = TileSheet.TileMatrix(TileIndex)
                });
        }

        public override void EnterState(Game Game)
        {
            var guiQuad = Gem.Geo.Gen.CreateQuad();
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateScale(30, 15, 1));
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateTranslation(0, -8, 0));
            GuiRoot = new Gem.Gui.GuiSceneNode(guiQuad, Game.Main.GraphicsDevice, 1024, 512);
            GuiRoot.uiRoot.AddPropertySet(null, new Gem.Gui.GuiProperties { Transparent = true });

            /*
            var x = 0;
            foreach (var template in BlockTemplates)
            {
                var child = CreateGuiSprite(new Rectangle(x, 128, 32, 32), template.Value.Preview, TileSheet);
                GuiRoot.uiRoot.AddChild(child);
                x += 32;
            }
            */

            var y = 8;
            foreach (var tool in Tools)
            {
                var child = CreateGuiSprite(new Rectangle(8, y, 64, 64), tool.Icon, TileSheet);
                child.Properties[0].Values.Upsert("click-action", new Action(() => SelectedTool = tool));
                GuiRoot.uiRoot.AddChild(child);
                y += 68;
            }

            GuiRoot.RenderOnTop = true;
            GuiRoot.DistanceBias = float.NegativeInfinity;

            Game.SceneGraph.Add(GuiRoot);

            SelectedTool = Tools[0];
        }
    
        public override void Update(Game Game)
        {
            GuiRoot.Orientation.SetFromMatrix(Matrix.Invert(Game.Camera.View));
            GuiRoot.Orientation.Position = -Game.Camera.Position;
            
            if (!Game.Main.IsActive) return;

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
                        if (Game.Input.Check("LEFT-CLICK"))
                            SelectedTool.Apply(Game, Game.HoverNode as WorldSceneNode);
                    }
                }
            }
            else if (Game.HoverNode is Gem.Gui.GuiSceneNode)
            {
                Game.HoverNode.SetHover();
                var action = Game.HoverNode.GetClickAction();
                if (action != null && Game.Input.Check("LEFT-CLICK"))
                    action();
            }
            
        }
    }
}
