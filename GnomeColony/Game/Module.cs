using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Module
    {
        public virtual void NewEntity(int ID, List<Component> Components) { }
        public virtual void DeleteEntity(int ID) { }
        public virtual Gem.Render.SceneNode CreateSceneNode(Simulation Sim) { return null; }

        public virtual void SimStep() { }
        public virtual void Update(float ElapsedSeconds) { }
    }
}
