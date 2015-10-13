using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Game
{
    public class NavigationMesh
	{
        //public static NavigationMesh Create(World World)
        //{
        //    var r = new NavigationMesh();
        //    r.Mesh = World.Grid.GenerateNavigationMesh();
        //    return r;
        //}

        public static NavigationMesh Create(Gem.Geo.EdgeMesh Mesh)
        {
            var r = new NavigationMesh();
            r.Mesh = Mesh;
            return r;
        }

		private Gem.Geo.EdgeMesh Mesh;

        public Gem.Geo.EdgeMesh.RayIntersectionResult RayIntersection(Ray Ray)
        {
            return Mesh.RayIntersection(Ray);
        }

        private Gem.Pathfinding<Gem.Geo.EMFace> pathfinder = new Pathfinding<Gem.Geo.EMFace>(
            (f) =>
            {
                var r = new List<Gem.Geo.EMFace>();
                foreach (var e in f.edges)
                    foreach (var n in e.Neighbors)
                        if (n != null && !Object.ReferenceEquals(n, f) && !r.Contains(n)) r.Add(n);
                return r;
            }, (a) => 1);

        public void DebugRender(Gem.Render.RenderContext Context)
        {
            Context.Color = new Vector3(1, 0, 0);
            Context.LightingEnabled = false;
            Context.ApplyChanges();
            foreach (var edge in Mesh.Edges)
                Context.DrawLineIM(Mesh.Verticies[edge.Verticies[0]], Mesh.Verticies[edge.Verticies[1]]);
        }

        public void DebugRenderFace(Gem.Render.RenderContext Context, Gem.Geo.EMFace Face)
        {
            Context.Color = new Vector3(0, 1, 1);
            Context.LightingEnabled = false;
            Context.ApplyChanges();
            foreach (var edge in Face.edges)
                Context.DrawLineIM(Mesh.Verticies[edge.Verticies[0]], Mesh.Verticies[edge.Verticies[1]]);
        }

		public PathFindingResult FindPath(Vector3 from, Vector3 to)
		{
			var r = new PathFindingResult();
			r.PathFound = false;
			var actorFace = Mesh.RayIntersection(new Ray(from + Vector3.UnitZ, -Vector3.UnitZ));
			if (actorFace == null) return r;
			var destinationFace = Mesh.FaceAt(to);
			if (destinationFace == null) return r;
			var pathResult = pathfinder.Flood(actorFace.face,
				(f) => { return Object.ReferenceEquals(f, destinationFace); }, f => 1.0f);
			if (!pathResult.GoalFound) return r;
			r.PathPoints.Add(from);
            var path = pathResult.FinalNode.ExtractPath();
			for (int i = 0; i < path.Count - 1; ++i)
			{
				var a = path[i];
				var b = path[i + 1];
				var sharedEdge = Gem.Geo.EdgeMesh.FindSharedEdge(a, b);
				if (sharedEdge == null) throw new InvalidProgramException("Pathfinder found invalid path");
				r.PathPoints.Add(Mesh.FindEdgeCenter(sharedEdge));
			}
			r.PathPoints.Add(to);
			r.PathFound = true;

			//r.PathPoints = SmoothPath(r.PathPoints, (l) => { r.DebugTracePoints = l; });
			return r;
		}

		internal List<Vector3> SmoothPath(List<Vector3> input, Action<List<Vector3>> DebugTracePoints = null)
		{
			var output = new List<Vector3>();
			var debugPoints = new List<Vector3>();
			output.Add(input[0]);
			for (int i = 1; i < input.Count - 1; ++i)
			{
				if (!Mesh.CanTrace(output[output.Count - 1], input[i + 1],
					(v) => { debugPoints.Add(v); }))
					output.Add(input[i]);
			}
			output.Add(input[input.Count - 1]);
			if (DebugTracePoints != null) DebugTracePoints(debugPoints);
			return output;
		}
    }
}
