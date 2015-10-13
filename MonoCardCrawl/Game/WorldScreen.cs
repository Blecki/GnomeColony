using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class WorldScreen : IScreen
    {
        public Gem.Input Input { get; set; }
        public Main Main { get; set; }

        EpisodeContentManager Content;
        float CameraDistance = -12;
        Vector3 CameraFocus = new Vector3(8.0f, 5.0f, 3.0f);
        public RenderContext RenderContext { get; private set; }
        public Gem.Render.FreeCamera Camera { get; private set; }
        public World World;
        public Gem.Render.BranchNode SceneGraph { get; private set; }
        private Actor PlayerActor;
        public SceneNode HoverNode { get; private set; }

        private List<InputState> InputStack = new List<InputState>();

        public void PushInputState(InputState NextState)
        {
            if (InputStack.Count > 0) InputStack.Last().Covered(this, World);
            InputStack.Add(NextState);
            NextState.EnterState(this, World);
        }

        public void PopInputState()
        {
            InputStack.Last().LeaveState(this, World);
            InputStack.RemoveAt(InputStack.Count - 1);
            if (InputStack.Count > 0) InputStack.Last().Exposed(this, World);
        }

        public WorldScreen()
        {
        }

        public void Begin()
        {
            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");
            
            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);
            Camera = new Gem.Render.FreeCamera(new Vector3(0, 0, 0), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);
            RenderContext.Camera = Camera;

            World = new World(16, 10, 3);

            var blockTile = World.AddTile("block", typeof(Game.Tiles.BlockTile), new TilePropertyBag
                {
                    Texture = RenderContext.White
                });

            var floorTile = World.AddTile("floor", typeof(Game.Tiles.FloorTile), new TilePropertyBag
                {
                    Texture = Content.Load<Texture2D>("floor01")
                });

            var rampTile = World.AddTile("ramp", typeof(Game.Tiles.RampTile), new TilePropertyBag
                {
                    Texture = Content.Load<Texture2D>("floor01")
                });

            
            World.Grid.ForEachTile((t, x, y, z) =>
                {
                    if (x == 0 || x == 15 || y == 9)
                        t.Tile = blockTile;
                    else if (z == 0)
                        t.Tile = floorTile;
                });

            World.Grid.CellAt(7, 3, 1).Tile = floorTile;
            World.Grid.CellAt(7, 4, 0).Tile = rampTile;
            World.Grid.CellAt(8, 4, 1).Tile = floorTile;
            World.Grid.CellAt(8, 3, 1).Tile = floorTile;

            SceneGraph = new Gem.Render.BranchNode();

            var testActorProperties = new Actors.AnimatedSpritePropertyBag
            {
                Sprite = Content.Load<Texture2D>("char"),
                NormalMap = RenderContext.NeutralNormals,
                DropShadow = Content.Load<Texture2D>("shadow"),
                Height = 1.5f,
                Width = 1.0f,
                Animations = new Gem.AnimationSet(
                    new Gem.Animation("idle", 0.15f, 0),
                    new Gem.Animation("run", 0.15f, 1, 2, 3, 4, 5, 6)),
                SpriteSheet = new Gem.SpriteSheet(4, 4),
                HiliteOnHover = true,
                HoverOverlay = Content.Load<Texture2D>("outline")
            };

            var commandSet = new List<Input.PlayerCommand>();
            testActorProperties.Upsert("commands", commandSet);

            var walkCommand = new Input.PlayerCommand(World);
            walkCommand.Check("can-walk", "actor", "cell", "path");
            walkCommand.Perform("do-walk", "actor", "path");
            testActorProperties.Upsert("walk-command", walkCommand);

            World.GlobalRules.Check<Actor, CombatCell, Pathfinding<CombatCell>.PathNode>("can-walk")
                .When((a, c, n) => n.PathCost > a.Properties.GetPropertyAsOrDefault<int>("turn-energy") || n.PathCost < 1)
                .Do((a, c, n) => SharpRuleEngine.CheckResult.Disallow);
            World.GlobalRules.Check<Actor, CombatCell, Pathfinding<CombatCell>.PathNode>("can-walk")
                .Do((a, c, n) => SharpRuleEngine.CheckResult.Allow);

            World.GlobalRules.Perform<Actor, Pathfinding<CombatCell>.PathNode>("do-walk")
                .Do((a, p) =>
                {
                    var energy = a.Properties.GetPropertyAs<int>("turn-energy");
                    energy -= (int)p.PathCost;
                    a.Properties.Upsert("turn-energy", energy);
                    a.NextAction = new Actors.Actions.WalkPath(p.ExtractPath());
                    PushInputState(new Input.WaitForIdle(a));
                    return SharpRuleEngine.PerformResult.Continue;
                });

            PlayerActor = World.SpawnActor(typeof(Actors.AnimatedSpriteActor),
                testActorProperties,
                new Vector3(4.5f, 4.5f, 0.25f));

            World.SpawnActor(typeof(Actors.AnimatedSpriteActor), testActorProperties, new Vector3(4.5f, 6.5f, 0.25f));

            Camera.Position = CameraFocus + new Vector3(0, -4, 3);
            //Camera.LookAt(CameraFocus);
            Camera.Position = CameraFocus + (Camera.GetEyeVector() * CameraDistance);

            Main.Input.AddBinding("RIGHT", new KeyboardBinding(Keys.Right, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT", new KeyboardBinding(Keys.Left, KeyBindingType.Held));
            Main.Input.AddBinding("UP", new KeyboardBinding(Keys.Up, KeyBindingType.Held));
            Main.Input.AddBinding("DOWN", new KeyboardBinding(Keys.Down, KeyBindingType.Held));
            Main.Input.AddBinding("CLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            Main.Input.AddBinding("GRID-UP", new KeyboardBinding(Keys.Q, KeyBindingType.Pressed));
            Main.Input.AddBinding("GRID-DOWN", new KeyboardBinding(Keys.E, KeyBindingType.Pressed));

            Main.Input.AddBinding("CAMERA-DISTANCE-TOGGLE", new KeyboardBinding(Keys.R, KeyBindingType.Held));

            Main.ScriptBuilder.DeriveScriptsFrom("MonoCardCrawl.ScriptBase");

            var StaticWorld = WorldModel.CreateStaticGeometryBuffers(World, Main.GraphicsDevice);
            SceneGraph.Add(StaticWorld);
            SceneGraph.Add(new ActorSceneNode(World));

            World.PrepareCombatGrid();

            PushInputState(new Input.TurnScheduler(World.Actors));
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
            //if (Main.Input.Check("GRID-UP")) Grid.Orientation.Position.Z += 1;
            //if (Main.Input.Check("GRID-DOWN")) Grid.Orientation.Position.Z -= 1;

            if (Main.Input.Check("RIGHT")) Camera.Yaw(elapsedSeconds);
            if (Main.Input.Check("LEFT")) Camera.Yaw(-elapsedSeconds);
            if (Main.Input.Check("UP")) Camera.Pitch(elapsedSeconds);
            if (Main.Input.Check("DOWN")) Camera.Pitch(-elapsedSeconds);

            if (Main.Input.Check("CAMERA-DISTANCE-TOGGLE")) CameraDistance = -24.0f;
            else CameraDistance = -14.0f;

            Camera.Position = CameraFocus + (Camera.GetEyeVector() * CameraDistance);

            foreach (var actor in World.Actors)
                actor.Update(World, elapsedSeconds);

            HoverNode = null;

            var pickVector = Camera.Unproject(new Vector3(Main.Input.QueryAxis("MAIN"), 0));
            var pickRay = new Ray(Camera.Position, pickVector - Camera.Position);
            var hoverItems = new List<HoverItem>();
            SceneGraph.CalculateLocalMouse(pickRay, (node, distance) => hoverItems.Add(new HoverItem { Node = node, Distance = distance }));

            if (hoverItems.Count > 0)
            {
                var nearestDistance = float.PositiveInfinity;
                foreach (var hoverItem in hoverItems)
                    if (hoverItem.Distance < nearestDistance) nearestDistance = hoverItem.Distance;
                HoverNode = hoverItems.First(item => item.Distance <= nearestDistance).Node;
            }

            if (InputStack.Count > 0) InputStack.Last().Update(this, World);
        }

        private struct HoverItem
        {
            public SceneNode Node;
            public float Distance;
        }

        public void Draw(float elapsedSeconds)
        {
            var viewport = Main.GraphicsDevice.Viewport;

            SceneGraph.UpdateWorldTransform(Matrix.Identity);
            SceneGraph.PreDraw(elapsedSeconds, RenderContext);
                       
            Main.GraphicsDevice.SetRenderTarget(null);
            Main.GraphicsDevice.Viewport = viewport;
            Main.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Main.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Main.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            RenderContext.Camera = Camera;
            RenderContext.Color = Vector3.One;
            RenderContext.Alpha = 1.0f;
            RenderContext.ClipAlpha = 0.2f;
            RenderContext.LightingEnabled = true;
            RenderContext.UVTransform = Matrix.Identity;
            RenderContext.World = Matrix.Identity;
            RenderContext.SetLight(0, PlayerActor.Orientation.Position + new Vector3(0.0f, -0.2f, 2.0f), 10, new Vector3(1, 0, 0));
            RenderContext.SetLight(1, new Vector3(8.5f, 5.5f, 7.5f), 10, new Vector3(0, 1, 0));
            RenderContext.SetLight(2, new Vector3(13.5f, 3.5f, 3.5f), 10, new Vector3(0, 0, 1));
            RenderContext.ActiveLightCount = 3;
            RenderContext.Texture = RenderContext.White;
            RenderContext.NormalMap = RenderContext.NeutralNormals;
            RenderContext.ApplyChanges();

            Main.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0xFFFFFF, 0);
            SceneGraph.Draw(RenderContext);
            RenderContext.LightingEnabled = true;
            

            RenderContext.World = Matrix.Identity;
            RenderContext.Texture = RenderContext.White;
            
            //World.NavMesh.DebugRender(RenderContext);
            //if (HitFace != null) 
            //    World.NavMesh.DebugRenderFace(RenderContext, HitFace);
        }
    }
}
