using netDxf.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using netDxf.Blocks;
using YW.Data.SpaceData.Extension;
using YW.SDK.FloorPlan.DxfPainter.Config;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.Utils.Numerics;
using static YW.Data.SpaceData.Model.FloorPlanData;
using YW.SDK.FloorPlan.DxfPainter;
namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    class BayWindowPainter : IPainter
    {
        public float WallWidth { get; set; } = 12;

        public List<EntityObject> Draw(Floor floor)
        {
            var entities = new List<EntityObject>();
            floor.Rooms.ForEach(room =>
            {
                room.Windows.Where(window =>  window.Depth != 0).ToList().ForEach(window =>
                {
                    var wall = window.GetWall(room);
                    var width = wall?.Width ?? WallWidth;
                    Insert insert = new Insert(Draw(window.ID, window.Length, window.Depth, width));
                    insert.Rotation = 90 - window.Direction.Theta();
                    insert.Position = window.Middle.ToDxfVector3MM();
                    entities.Add(insert);
                });
            });
            return entities;
        }

        private Block Draw(string blockName, float length, float depth, float wallWidth = 120)
        {
            depth *= 10;
            var dxfWidth = FloorPlanDxfPainter.DxfUnit * wallWidth;
            var block = new Block(blockName);
            var p1 = new Vector2(-length / 2 + wallWidth / 2, 0);
            var p2 = new Vector2(length / 2 - wallWidth / 2, 0);

            var p11 = p1.ToDxfVector2MM();
            var p22 = p2.ToDxfVector2MM();

            var direction = new netDxf.Vector2(0, 1);

            var t1 = p11 + direction * (depth + dxfWidth / 2);
            var t2 = t1 - direction * dxfWidth;
            var t3 = p22 + direction * (depth + dxfWidth / 2);
            var t4 = t3 - direction * dxfWidth;

            var t5 = t2 + direction * (dxfWidth / 3);
            var t6 = t4 + direction * (dxfWidth / 3);
            var t7 = t1 - direction * dxfWidth / 3;
            var t8 =  t3 - direction * dxfWidth / 3;
            
            //外圈
            List<LwPolylineVertex> vertexes1 = new List<LwPolylineVertex>()
            {
                new LwPolylineVertex( new netDxf.Vector2(t2.X, t2.Y)),
                new LwPolylineVertex( new netDxf.Vector2(t1.X, t1.Y)),
                new LwPolylineVertex( new netDxf.Vector2(t3.X, t3.Y)),
                new LwPolylineVertex( new netDxf.Vector2(t4.X, t4.Y)),
                new LwPolylineVertex( new netDxf.Vector2(t2.X, t2.Y))
            };

            //横线
            List<LwPolylineVertex> vertexes2 = new List<LwPolylineVertex>()
            {
                new LwPolylineVertex( new netDxf.Vector2(t5.X, t5.Y)),
                new LwPolylineVertex( new netDxf.Vector2(t6.X, t6.Y))
            };
            List<LwPolylineVertex> vertexes3 = new List<LwPolylineVertex>()
            {
                new LwPolylineVertex( new netDxf.Vector2(t7.X, t7.Y)),
                new LwPolylineVertex( new netDxf.Vector2(t8.X, t8.Y))
            };
            
            //底部线
            List<LwPolylineVertex> vertexes4 = new List<LwPolylineVertex>()
            {
                new LwPolylineVertex( new netDxf.Vector2(p11.X, p11.Y) - new netDxf.Vector2(0, dxfWidth / 2)),
                new LwPolylineVertex( new netDxf.Vector2(p22.X, p22.Y) - new netDxf.Vector2(0, dxfWidth / 2))
            };
            
            LwPolyline polyline = new LwPolyline(vertexes1, true);
            LwPolyline polyline2 = new LwPolyline(vertexes2, true);
            LwPolyline polyline3 = new LwPolyline(vertexes3, false);
            LwPolyline polyline4 = new LwPolyline(vertexes4, false);
            
            polyline.Color = DxfConfig.Color;
            polyline2.Color = DxfConfig.Color;
            polyline3.Color = DxfConfig.Color;
            polyline4.Color = DxfConfig.Color;
            
            block.Entities.Add(polyline);
            block.Entities.Add(polyline2);
            block.Entities.Add(polyline3);
            block.Entities.Add(polyline4);
            
            return block;
        }
    }
}
