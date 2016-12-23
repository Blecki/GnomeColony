using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class GuiInputState : InputState
    {
        public Gem.Gui.GuiSceneNode GuiRoot { get; private set; }
        private Gem.Render.OrthographicCamera GUICamera;

        public GuiInputState()
        { 
        
        }

        public static Gem.Gui.UIItem CreateGuiSprite(Rectangle Position, int TileIndex, TileSheet TileSheet)
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
            GUICamera = new Gem.Render.OrthographicCamera(Game.Main.GraphicsDevice.Viewport);
            var renderTree = new Game.RenderTree
            {
                Camera = GUICamera,
                SceneGraph = new Gem.Render.BranchNode()
            };
            GUICamera.focus = Vector2.Zero;

            Game.RenderTrees.Add(renderTree);

            var guiQuad = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.CreateQuad());
            Gem.Geo.Gen.Transform(guiQuad, Matrix.CreateScale(800, -600, 1.0f));
            Gem.Geo.Gen.CalculateTangentsAndBiNormals(guiQuad);
            GuiRoot = new Gem.Gui.GuiSceneNode(guiQuad, Game.Main.GraphicsDevice, 800, 600);
            GuiRoot.uiRoot.AddPropertySet(null, new Gem.Gui.GuiProperties { Transparent = true });

            GuiRoot.RenderOnTop = true;
            GuiRoot.DistanceBias = float.NegativeInfinity;

            renderTree.SceneGraph.Add(GuiRoot);
        }
    
        public override void Update(Game Game)
        {
            if (!Game.Main.IsActive) return;

            if (Game.HoverNode is Gem.Gui.GuiSceneNode)
            {
                Game.HoverNode.SetHover();
                var action = Game.HoverNode.GetClickAction();
                if (action != null && Game.Input.Check("LEFT-CLICK"))
                    action();
            }            
        }
    }
}
