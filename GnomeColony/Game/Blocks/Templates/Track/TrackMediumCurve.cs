using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class TrackMediumCurve : BlockTemplate
    {
        public TrackMediumCurve()
        {
            Preview = 129;
            Shape = BlockShape.Combined;
            ShowInEditor = true;
        }

        public override void Initialize(BlockSet BlockSet)
        {
            CompositeBlocks = new List<OrientedBlock>();

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = new TrackBase
                    {
                        Shape = BlockShape.Decal,
                        Top = 129,
                        Orientable = true
                    },
                Offset = new Coordinate(0, 0, 0)
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = new TrackBase
                    {
                        Shape = BlockShape.Decal,
                        Top = 130,
                        Orientable = true
                    },
                Offset = new Coordinate(1, 0, 0)
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = new TrackBase
                {
                    Shape = BlockShape.Decal,
                    Top = 161,
                    Orientable = true
                },
                Offset = new Coordinate(0, 1, 0)
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = new TrackBase
                {
                    Shape = BlockShape.Decal,
                    Top = 162,
                    Orientable = true
                },
                Offset = new Coordinate(1, 1, 0)
            });

        }
    }
}
