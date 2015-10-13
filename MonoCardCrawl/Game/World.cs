using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using System.Reflection;

namespace Game
{
    public class World
	{
		public CellGrid Grid;
		public Dictionary<String, Tile> TileSet = new Dictionary<String, Tile>();
        public List<Actor> Actors = new List<Actor>();
        public SharpRuleEngine.RuleEngine GlobalRules;
        public CombatGrid CombatGrid;

        public World(int Width, int Height, int Depth)
        {
            Grid = new CellGrid(Width, Height, Depth);

            GlobalRules = new SharpRuleEngine.RuleEngine(SharpRuleEngine.NewRuleQueueingMode.QueueNewRules);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                if (type.FullName.StartsWith("MonoCardCrawl"))
                    foreach (var method in type.GetMethods())
                        if (method.IsStatic && method.Name == "DeclareRules")
                            try { method.Invoke(null, new Object[] { GlobalRules }); }
                            catch (Exception e) { }

            GlobalRules.Check<Actor, CombatCell>("can-traverse").When((a, c) => c.AnchoredActor != null).Do((a, c) => SharpRuleEngine.CheckResult.Disallow);
            GlobalRules.Check<Actor, CombatCell>("can-traverse").Do((a, c) => SharpRuleEngine.CheckResult.Allow);

            
            
            GlobalRules.FinalizeNewRules();
        }

        public void PrepareCombatGrid()
        {
            CombatGrid = CombatGrid.CreateFromCellGrid(Grid);
            CombatGrid.Cells.forAll((c, x, y, z) => { if (c != null) c.Texture = 0; });
        }

        public Tile AddTile(String Name, System.Type TileType, PropertyBag Properties)
        {
            var tile = Activator.CreateInstance(TileType) as Tile;
            if (tile == null) throw new InvalidOperationException();
            tile.World = this;
            tile.Create(Properties);
            TileSet.Upsert(Name, tile);
            return tile;
        }

        public Actor SpawnActor(System.Type ActorType, PropertyBag Properties, Vector3 Position)
        {
            var actor = Activator.CreateInstance(ActorType) as Actor;
            if (actor == null) throw new InvalidOperationException();
            actor.World = this;
            Actors.Add(actor);
            actor.Create(Properties);
            actor.Orientation.Position = Position;
            return actor;
        }

        /// <summary>
        /// The combat grid needs to know which actors are on which cells.
        /// </summary>
        public void PrepareCombatGridForPlayerInput()
        {
            CombatGrid.ClearTemporaryData();

            foreach (var actor in Actors)
            {
                var coord = new Coordinate((int)actor.Orientation.Position.X, (int)actor.Orientation.Position.Y, (int)actor.Orientation.Position.Z);
                if (Grid.check(coord.X, coord.Y, coord.Z) && CombatGrid.Cells[coord.X, coord.Y, coord.Z] != null)
                    CombatGrid.Cells[coord.X, coord.Y, coord.Z].AnchoredActor = actor;
            }
        }
    }
}
