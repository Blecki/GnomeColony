using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Gnome
{
    public class Game : IScreen
    {
        public Gem.Input Input { get; set; }
        public Main Main { get; set; }

        EpisodeContentManager Content;
        float CameraDistance = -12;
        Vector3 CameraFocus = new Vector3(0.0f, 0.0f, 0.0f);
        public RenderContext RenderContext { get; private set; }
        public Gem.Render.FreeCamera Camera { get; private set; }
        public Gem.Render.BranchNode SceneGraph { get; private set; }
        public SceneNode HoverNode { get; private set; }

        private List<InputState> InputStack = new List<InputState>();
        public CellGrid World { get; private set; }
        private List<Actor> Actors;
        private List<Task> Tasks;
        public BlockTemplateSet BlockTemplates = new BlockTemplateSet();
        public TileSheet BlockTiles;
        private WorldSceneNode WorldSceneNode;
        public Pathfinding<Cell> Pathfinding {get; private set;}
        public float ElapsedSeconds { get; private set; }

        public void SetUpdateFlag(Coordinate Coordinate)
        {
            World.MarkDirtyBlock(Coordinate);
        }

        private float CameraYaw = 0.0f;
        private float CameraPitch = 0.0f;

        public void PushInputState(InputState NextState)
        {
            if (InputStack.Count > 0) InputStack.Last().Covered(this);
            InputStack.Add(NextState);
            NextState.EnterState(this);
        }

        public void PopInputState()
        {
            InputStack.Last().LeaveState(this);
            InputStack.RemoveAt(InputStack.Count - 1);
            if (InputStack.Count > 0) InputStack.Last().Exposed(this);
        }

        public void AddTask(Task Task)
        {
            if (World.Check(Task.Location))
            {
                Task.IsTopLevelTask = true;

                var cell = World.CellAt(Task.Location);
                cell.Task = Task;
                Tasks.Add(Task);
                SetUpdateFlag(Task.Location);
            }
        }

        public Task FindTask(Gnome Gnome)
        {
            return Tasks.FirstOrDefault(t => t.AssignedGnome == null);
        }

        public void AbandonTask(Task Task)
        {
            Task.AssignedGnome = null;
            
            if (Task.IsTopLevelTask)
            {
                Tasks.Remove(Task);
                Tasks.Add(Task);
            }
        }

        public Game()
        {
        }

        public void Begin()
        {
            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");

            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);
            Camera = new Gem.Render.FreeCamera(new Vector3(0, 0, 0), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);
            RenderContext.Camera = Camera;

            World = new CellGrid(16, 16, 16);

            BlockTemplates.Add(BlockTypes.Scaffold, new BlockTemplate
            {
                Preview = TileNames.TaskMarkerBuild,
                Top = TileNames.TaskMarkerBuild,
                Side = TileNames.TaskMarkerBuild,
                Bottom = TileNames.TaskMarkerBuild,
                Shape = BlockShape.Cube,
                Solid = false,
                ResourceHeightOffset = -0.5f
            });

            BlockTemplates.Add(BlockTypes.Grass, new BlockTemplate
                {
                    Preview = TileNames.BlockGrassTop,
                    Top = TileNames.BlockGrassTop,
                    Side = TileNames.BlockGrassSide,
                    Bottom = TileNames.BlockDirt,
                    Shape = BlockShape.Cube,
                    ConstructionResources = new int[] {  BlockTypes.Dirt, BlockTypes.Dirt },
                    MineResources = new int[] { BlockTypes.Dirt, BlockTypes.Dirt, BlockTypes.Dirt }
                });

            BlockTemplates.Add(BlockTypes.Dirt, new BlockTemplate
            {
                Preview = TileNames.BlockDirt,
                Top = TileNames.BlockDirt,
                Side = TileNames.BlockDirt,
                Bottom = TileNames.BlockDirt,
                Shape = BlockShape.Cube
            });

            

            World.forAll((t, x, y, z) =>
                {
                    if (z <= 1) t.Block = BlockTemplates[BlockTypes.Grass];
                    else t.Block = null;
                });

            World.CellAt(4, 4, 1).Storehouse = true;

            Actors = new List<Actor>();

            Tasks = new List<Task>();

            SceneGraph = new Gem.Render.BranchNode();

            Camera.Position = CameraFocus + new Vector3(0, -4, 3);

            Main.Input.ClearBindings();
            Main.Input.AddAxis("MAIN-AXIS", new MouseAxisBinding());
            Main.Input.AddBinding("RIGHT", new KeyboardBinding(Keys.Right, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT", new KeyboardBinding(Keys.Left, KeyBindingType.Held));
            Main.Input.AddBinding("UP", new KeyboardBinding(Keys.Up, KeyBindingType.Held));
            Main.Input.AddBinding("DOWN", new KeyboardBinding(Keys.Down, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-LEFT", new KeyboardBinding(Keys.A, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-FORWARD", new KeyboardBinding(Keys.W, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-RIGHT", new KeyboardBinding(Keys.D, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-BACK", new KeyboardBinding(Keys.S, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT-CLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            Main.Input.AddBinding("RIGHT-CLICK", new MouseButtonBinding("RightButton", KeyBindingType.Pressed));

            Main.Input.AddBinding("CAMERA-DISTANCE-TOGGLE", new KeyboardBinding(Keys.R, KeyBindingType.Held));

            Main.ScriptBuilder.DeriveScriptsFrom("Gnome.ScriptBase");

            var tileTexture = Content.Load<Texture2D>("tiles");
            BlockTiles = new TileSheet(tileTexture, 16, 16);

            WorldSceneNode = new WorldSceneNode(World, new WorldSceneNodeProperties
            {
                HiliteTexture = TileNames.HoverHilite,
                TileSheet = BlockTiles,
                BlockTemplates = BlockTemplates
            });

            SceneGraph.Add(WorldSceneNode);

            SceneGraph.Add(new ActorSceneNode(Actors));

            var guiTools = new List<GuiTool>();
            guiTools.Add(new GuiTools.Build());
            guiTools.Add(new GuiTools.Mine());
            guiTools.Add(new GuiTools.MarkStorehouse());

            PushInputState(new HoverTest(BlockTemplates, BlockTiles, guiTools));

            World.PrepareNavigation();
            World.MarkDirtyChunk();
            SceneGraph.UpdateWorldTransform(Matrix.Identity);

            for (int i = 0; i < 4; ++i)
            {
                var gnomeActor = new Gnome(BlockTiles);
                gnomeActor.Location = new Coordinate(0, i, 1);
                Actors.Add(gnomeActor);
            }

            Pathfinding = new Pathfinding<Cell>(
            (cell) =>
            {
                return new List<Cell>(cell.Links.Select(c => c.Neighbor).Where(c => c.CanWalk));
            },
            (cell) => 1.0f);
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
            this.ElapsedSeconds = elapsedSeconds;

            for (var i = 0; i < Tasks.Count; )
            {
                if (Tasks[i].QueryStatus(this) == TaskStatus.Complete)
                {
                    World.CellAt(Tasks[i].Location).Task = null;
                    SetUpdateFlag(Tasks[i].Location);
                    Tasks.RemoveAt(i);
                }
                else
                    ++i;
            }

            World.UpdateDirtyBlocks();

            if (World.ChunkDirty)
            {
                WorldSceneNode.UpdateGeometry();
                World.ClearChunkDirtyFlag();
            }

            if (Main.Input.Check("RIGHT")) CameraYaw += elapsedSeconds;
            if (Main.Input.Check("LEFT")) CameraYaw -= elapsedSeconds;
            if (Main.Input.Check("UP")) CameraPitch += elapsedSeconds;
            if (Main.Input.Check("DOWN")) CameraPitch -= elapsedSeconds;

            if (Main.Input.Check("CAMERA-DISTANCE-TOGGLE"))
                CameraDistance = -24.0f;
            else CameraDistance = -14.0f;

            if (CameraPitch < 0.5f) CameraPitch = 0.5f;
            if (CameraPitch > 1.5f) CameraPitch = 1.5f;

            Camera.Position = CameraFocus + Vector3.Transform(new Vector3(0, -CameraDistance, 0),
                 Matrix.CreateRotationX(CameraPitch) * Matrix.CreateRotationZ(CameraYaw));
            Camera.LookAt(CameraFocus, Vector3.UnitZ);

            foreach (var actor in Actors)
                actor.Update(this);
            
            HoverNode = null;

            var pickVector = Camera.Unproject(new Vector3(Main.Input.QueryAxis("MAIN-AXIS"), 0));
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

            if (InputStack.Count > 0) InputStack.Last().Update(this);
            SceneGraph.UpdateWorldTransform(Matrix.Identity);
        }

        private struct HoverItem
        {
            public SceneNode Node;
            public float Distance;
        }

        public void Draw(float elapsedSeconds)
        {
            var viewport = Main.GraphicsDevice.Viewport;

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
            //RenderContext.SetLight(0, PlayerActor.Orientation.Position + new Vector3(0.0f, -0.2f, 2.0f), 10, new Vector3(1, 0, 0));
            RenderContext.SetLight(1, new Vector3(-6.5f, -6.5f, 6.5f), 20, new Vector3(1, 1, 1));
            RenderContext.SetLight(2, new Vector3(6.5f, 6.5f, 6.5f), 20, new Vector3(1, 1, 1));
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
