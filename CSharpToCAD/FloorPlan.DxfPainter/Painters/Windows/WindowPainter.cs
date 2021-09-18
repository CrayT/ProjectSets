using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YW.Data.SpaceData.Extension;
using YW.Data.SpaceData.Model;
using YW.SDK.FloorPlan.DxfPainter.Config;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using Vector2 = System.Numerics.Vector2;
using YW.Utils.Numerics;

namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    class WindowPainter : IPainter
    {
        public float WallWidth { get; set; } = 12;

        public List<EntityObject> Draw(FloorPlanData.Floor floor)
        {
            var entities = new List<EntityObject>();

            floor.Rooms.ForEach(room =>
            {
                room.Windows.Where(window => window.Depth == 0).ToList().ForEach(window =>
                {
                    var wall = window.GetWall(room);
                    if (wall != null)
                    {
                        WallWidth = wall.Width;
                    }
                    Insert insert = new Insert(Draw(window.ID, window.Length, WallWidth));
                    insert.Rotation = 90 - window.Direction.Theta();
                    insert.Position = window.Middle.ToDxfVector3MM();
                    entities.Add(insert);
                });
            });

            return entities;
        }

        private Block Draw(string blockName, float windowLength, float wallWidth)
        {
            var block = new Block(blockName);

            var xVector = Vector2.UnitX * windowLength / 2;
            var yVector = Vector2.UnitY * wallWidth / 2;

            // 窗框
            var vertices = new Vector2[] 
            {
                -xVector - yVector,
                -xVector + yVector,
                xVector + yVector,
                xVector - yVector
            };
            var line = vertices.ToPolyline();
            line.Color = DxfConfig.Color;
            block.Entities.Add(line);

            // 窗示意
            var line1 = new Line((-xVector + yVector / 3).ToDxfVector2MM(), (xVector + yVector / 3).ToDxfVector2MM());
            line1.Color = DxfConfig.Color;
            block.Entities.Add(line1);

            var line2 = new Line((-xVector - yVector / 3).ToDxfVector2MM(), (xVector - yVector / 3).ToDxfVector2MM());
            line2.Color = DxfConfig.Color;
            block.Entities.Add(line2);

            return block;
        }
    }
}
