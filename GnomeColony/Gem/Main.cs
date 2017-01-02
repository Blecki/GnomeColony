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
		public EpisodeContentManager EpisodeContent;
        public ScriptBuilder ScriptBuilder;
        private GumInputMapper InputMapper;

        public Gum.Root GuiRoot;
        GraphicsDeviceManager graphics;

        public Input Input { get; private set; }

        public bool ConsoleOpen { get; private set; }

        public void ReportException(Exception e)
        {
        //    ConsoleOpen = true;
        //    Console.WriteLine(e.Message);
        //    Console.WriteLine(e.StackTrace);
        }

        public Main(String startupCommand)
        {
            ConsoleOpen = false;

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            Input = new Input();
            Input.AddAxis("MAIN", new MouseAxisBinding());
            InputMapper = new GumInputMapper(Window.Handle);

        }

        protected override void LoadContent()
        {
			EpisodeContent = new EpisodeContentManager(Content.ServiceProvider, "");
        
            GuiRoot = new Gum.Root(GraphicsDevice, new Point(640, 480), EpisodeContent, "Content/mono_draw",
                "Content/gnome_colony_skin/sheets.txt");
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
                    activeGame.Input = Input;
                    try
                    {
                        activeGame.Begin();
                    }
                    catch (Exception e)
                    {
                        activeGame = saveActive;
                        if (activeGame != null) activeGame.Begin();
                        ReportException(e);
                    }
                    nextGame = null;
                }

                var guiInputQueue = InputMapper.GetInputQueue();
                foreach (var @event in guiInputQueue)
                {
                    if (@event.Message == Gum.InputEvents.KeyPress && @event.Args.KeyValue == '~')
                        ConsoleOpen = !ConsoleOpen;
                    else
                        GuiRoot.HandleInput(@event.Message, @event.Args);
                }

                Input.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
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

            GuiRoot.Draw();

            
            base.Draw(gameTime);
        }
    }
}
