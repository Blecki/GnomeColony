using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    /// <summary>
    /// A generic implementation of A*
    /// </summary>
    /// <typeparam name="NODE"></typeparam>
    public class Pathfinding<NODE> where NODE: class
    {
        private Func<NODE, List<NODE>> EnumerateNeighbors;
        private Func<NODE, float> Cost;

        public Pathfinding(Func<NODE, List<NODE>> EnumerateNeighbors, Func<NODE, float> Cost)
        {
            this.EnumerateNeighbors = EnumerateNeighbors;
            this.Cost = Cost;
        }

        public class PathfindingResult
        {
            public Dictionary<NODE, PathNode> VisitedNodes;
            public PathNode FinalNode;
            public bool GoalFound = false;
        }

        internal enum NodeState
        {
            Open,
            Closed
        }

        public class PathNode
        {
            internal PathNode Parent;
            internal float PathCost;
            internal float PredictedCost;
            internal NODE RealNode;
            internal NodeState state = NodeState.Open;

            public List<NODE> ExtractPath()
            {
                var r = new List<NODE>();
                var pathEnd = this;
                while (pathEnd != null)
                {
                    r.Add(pathEnd.RealNode);
                    pathEnd = pathEnd.Parent;
                }

                r.Reverse();
                return r;
            }
        }

        public Pathfinding<NODE>.PathfindingResult Flood(
            NODE From, 
            Func<NODE, bool> Goal,
            Func<NODE, float> Heuristic)
        {
            var result = new Pathfinding<NODE>.PathfindingResult();
            result.VisitedNodes = new Dictionary<NODE, PathNode>();
            var head = new PathNode { Parent = null, RealNode = From, state = NodeState.Closed, PathCost = 0 };
            result.VisitedNodes.Add(From, head);
            var openNodes = new List<PathNode>();
            result.GoalFound = false;           

            while (head != null)
            {
                if (Goal(head.RealNode))
                {
                    result.FinalNode = head;
                    result.GoalFound = true;
                    return result;
                }

                foreach (var newOpenNode in EnumerateNeighbors(head.RealNode))
                {
                    if (result.VisitedNodes.ContainsKey(newOpenNode)) continue;
                    var newNode = new PathNode
                    {
                        Parent = head,
                        RealNode = newOpenNode,
                        state = NodeState.Open,
                        PathCost = head.PathCost + Cost(newOpenNode),
                        PredictedCost = head.PathCost + Cost(newOpenNode) + Heuristic(newOpenNode)
                    };
                    result.VisitedNodes.Add(newOpenNode, newNode);
                    openNodes.Add(newNode); 
                }

                if (openNodes.Count == 0) head = null;
                else
                {
                    head = openNodes.First(n => n.PredictedCost == openNodes.Min(p => p.PredictedCost));
                    //head = openNodes[0];
                    head.state = NodeState.Closed;
                    openNodes.Remove(head);
                    //openNodes.RemoveAt(0);
                }
            }

            return result;
        }
    }
}
