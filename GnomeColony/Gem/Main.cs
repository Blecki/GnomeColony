using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gem
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        private IScreen activeGame = null;
        private IScreen nextGame = null;
        public IScreen Game { get { return activeGame; } set { nextGame = value; } }

        public Gum.RenderData GuiSkin;
        GraphicsDeviceManager graphics;

        public GumInputMapper InputMapper { get; private set; }

        public Main(String startupCommand)
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void LoadContent()
        {
            GuiSkin = new Gum.RenderData(GraphicsDevice, Content, "Content/mono_draw",
                "Content/gnome_colony_skin/sheets.txt");

            InputMapper = new GumInputMapper(Window.Handle);
        }

        protected override void UnloadContent()
        {
            if (activeGame != null)
                activeGame.End();
            activeGame = null;
        }

        private int ticks = 0;
        protected override void Update(GameTime gameTime)
        {
            if (ticks != 0)
            {
                if (nextGame != null)
                {
                    var saveActive = activeGame;
                    if (activeGame != null) activeGame.End();
                    activeGame = nextGame;
                    activeGame.Main = this;
                    activeGame.Begin();
                    nextGame = null;
                }

                if (activeGame != null) activeGame.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
                ticks = 1;
           
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            if (activeGame != null) activeGame.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);
            
            base.Draw(gameTime);
        }
    }
}
