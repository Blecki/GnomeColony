using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome
{
    public class BlockTemplate
    {
        public int Preview = 1;
        public int Top = 1;
        public int Side = 1;
        public int Bottom = 1;

        public Gem.Geo.Mesh NavigationMesh;

        public BlockTemplate() { }

        public BlockTemplate(int Preview, int Top, int Side, int Bottom, Gem.Geo.Mesh NavigationMesh)
        {
            this.Preview = Preview;
            this.Top = Top;
            this.Side = Side;
            this.Bottom = Bottom;
            this.NavigationMesh = NavigationMesh;
        }
    }

    public class BlockTemplateSet : Dictionary<int, BlockTemplate>
    {

    }
}
