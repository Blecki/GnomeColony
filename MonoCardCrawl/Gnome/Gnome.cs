using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnome
{
    public class Gnome : Actor
    {
        public Stack<Task> TaskStack = new Stack<Task>();
        public Gem.Render.SceneNode CarriedResourceVisual = null;
        public int CarriedResource = 0;

        private Gem.Render.MeshNode GnomeNode;
        private Gem.Render.MeshNode TaskIcon;
        private TileSheet Sheet;
        public CellLink.Directions FacingDirection = CellLink.Directions.North;

        public Gnome(TileSheet Sheet)
        {
            this.Sheet = Sheet;

            var gnomeMesh = Gem.Geo.Gen.CreateTexturedFacetedCube();
            Gem.Geo.Gen.Transform(gnomeMesh, Matrix.CreateTranslation(0.0f, 0.0f, 0.5f));
            Gem.Geo.Gen.Transform(gnomeMesh, Matrix.CreateScale(0.75f, 0.75f, 1.5f));

            Gem.Geo.Gen.MorphEx(gnomeMesh, (inV) =>
            {
                var r = inV;

                if (r.Normal.Z > 0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeBottom, 1, 1));
                else if (r.Normal.Z < -0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeTop, 1, 1));
                else if (r.Normal.Y > 0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeFront, 1, 2));
                else if (r.Normal.Y < -0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeBack, 1, 2));
                else
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Sheet.TileMatrix(TileNames.GnomeSide, 1, 2));
                return r;
            });

            var rootNode = new Gem.Render.BranchNode(Orientation);
            Renderable = rootNode;

            GnomeNode = new Gem.Render.MeshNode(gnomeMesh, Sheet.Texture, null);
            rootNode.Add(GnomeNode);

            var iconMesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.CreateQuad());
            Gem.Geo.Gen.Transform(iconMesh, Matrix.CreateFromYawPitchRoll(0.0f, Gem.Math.Angle.PI / 2.0f, 0.0f));
            Gem.Geo.Gen.Transform(iconMesh, Matrix.CreateTranslation(0, 0, 2));
            Gem.Geo.Gen.CalculateTangentsAndBiNormals(iconMesh);
            TaskIcon = new Gem.Render.MeshNode(iconMesh, Sheet.Texture, null);
            TaskIcon.InteractWithMouse = false;
            rootNode.Add(TaskIcon);
        }

        public Task PushTask(Task Task)
        {
            Task.AssignedGnome = this;
            TaskStack.Push(Task);
            return Task;
        }

        public override void Update(Game Game)
        {
            base.Update(Game);

            if (CarriedResource == 0 && CarriedResourceVisual != null)
            {
                (this.Renderable as Gem.Render.BranchNode).Remove(CarriedResourceVisual);
                CarriedResourceVisual = null;
            }
            else if (CarriedResource != 0 && CarriedResourceVisual == null)
            {
                CarriedResourceVisual = new Gem.Render.MeshNode(
                    Generate.CreateResourceBlockMesh(Game.BlockTiles, Game.BlockTemplates[CarriedResource]),
                    Game.BlockTiles.Texture, null);
                (this.Renderable as Gem.Render.BranchNode).Add(CarriedResourceVisual);
            }

            // Rotate the carried resource to face the right way.
            if (CarriedResourceVisual != null)
            {
                CarriedResourceVisual.Orientation.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                    (Gem.Math.Angle.PI / 2) * (int)FacingDirection);
                CarriedResourceVisual.Orientation.Position = Vector3.Transform(
                    new Vector3(0.0f, -0.5f, 0.5f), CarriedResourceVisual.Orientation.Orientation);
            }

            if (TaskStack.Count == 0)
            {
                // Find a new task.
                var newTask = Game.FindTask(this);
                if (newTask != null)
                    PushTask(newTask);
                else
                    if (CarriedResource != 0) PushTask(new Tasks.Deposit());
            }
            else
            {
                var currentTask = TaskStack.Peek();

                var status = currentTask.QueryStatus(Game);
                if (status == TaskStatus.Complete)
                    TaskStack.Pop();
                else if (status == TaskStatus.Impossible)
                {
                    // The task, or a prerequisite, is revealed to be impossible. Abandon the task stack entirely.
                    while (TaskStack.Count != 0)
                        Game.AbandonTask(TaskStack.Pop());
                }
                else
                {
                    var prerequisite = currentTask.Prerequisite(Game, this);
                    if (prerequisite != null)
                        PushTask(prerequisite);
                    else
                    {
                        // All prerequisites met.
                        if (currentTask.QueryValidLocation(Game, Location))
                            currentTask.ExecuteTask(Game, this);
                        else
                            PushTask(new WalkTask(currentTask));
                    }
                }
            }

            // Display correct task icon
            var taskIconIndex = TileNames.TaskIconBlank;
            if (TaskStack.Count != 0)
                taskIconIndex = TaskStack.Peek().GnomeIcon;
            TaskIcon.UVTransform = Sheet.TileMatrix(taskIconIndex);
            
            // Orient task icon toward camera
            var cameraPos = Game.Camera.GetPosition();
            var cameraDelta = this.Orientation.Position - cameraPos;
            var billboardAngle = Gem.Math.Vector.AngleBetweenVectors(new Vector2(0, 1), new Vector2(cameraDelta.X, cameraDelta.Y));
            TaskIcon.Orientation.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, billboardAngle);

            // Face gnome correct direction
            GnomeNode.Orientation.Orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ,
                (Gem.Math.Angle.PI / 2) * (int)FacingDirection);
        }
    }
}
