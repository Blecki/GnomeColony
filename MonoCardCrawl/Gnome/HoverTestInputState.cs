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
                
        public HoverTest(BlockTemplateSet BlockTemplates, TileSheet TileSheet)
        {
            this.BlockTemplates = BlockTemplates;
            this.TileSheet = TileSheet;
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

            var x = 0;
            foreach (var template in BlockTemplates)
            {
                var child = CreateGuiSprite(new Rectangle(x, 128, 32, 32), template.Value.Preview, TileSheet);
                GuiRoot.uiRoot.AddChild(child);
                x += 32;
            }

            GuiRoot.RenderOnTop = true;
            GuiRoot.DistanceBias = float.NegativeInfinity;

            Game.SceneGraph.Add(GuiRoot);
        }
    
        public override void Update(Game Game)
        {
            GuiRoot.Orientation.SetFromMatrix(Matrix.Invert(Game.Camera.View));
            GuiRoot.Orientation.Position = -Game.Camera.Position;


            if (Game.HoverNode != null)
                Game.HoverNode.SetHover();

            var worldNode = Game.HoverNode as WorldSceneNode;

            if (worldNode != null)
            {
                if (Game.Input.Check("LEFT-CLICK"))
                    Game.AddTask(new Tasks.Build(Game.BlockTemplates[1], worldNode.AdjacentHoverBlock));
                else if (Game.Input.Check("RIGHT-CLICK"))
                    Game.AddTask(new Tasks.Mine(worldNode.HoverBlock));
            }
        }
    }
}
