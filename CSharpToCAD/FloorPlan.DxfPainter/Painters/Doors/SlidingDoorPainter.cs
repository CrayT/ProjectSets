using System.Collections.Generic;
using System.Linq;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using static YW.Data.SpaceData.Model.FloorPlanData;
using netDxf.Tables;
using YW.Data.SpaceData.Extension;
using YW.Data.SpaceData.Model.Types;
using YW.SDK.FloorPlan.DxfPainter.Config;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.Utils.Numerics;
using Vector2 = System.Numerics.Vector2;

namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    public class SlidingDoorPainter: IPainter
    {
        public float WallWidth { get; set; } = 12;

        public List<EntityObject> Draw(Floor floor)
        {
            var entities = new List<EntityObject>();

            floor.Rooms.ForEach(room =>
            {
                room.Doors.Where(d => d.Type == DoorType.SlidingDoor).ToList().ForEach(door =>
                {
                    var wall = door.GetWall(room);
                    var width = wall?.Width ?? WallWidth;
                    Insert insert = new Insert(Draw(door.ID, door.Length, width * 10));
                    insert.Rotation = 90 - door.Direction.Theta();
                    insert.Position = door.Middle.ToDxfVector3MM();
                    entities.Add(insert);
                });
            });
            return entities;
        }

        private Block Draw(string blockName, float length, float wallWidth = 120)
        {
            var block = new Block(blockName);
            var p1 = new Vector2(-length / 2, 0);
            var p2 = new Vector2(length / 2, 0);

            var p11 = p1.ToDxfVector2MM();
            var p22 = p2.ToDxfVector2MM();

            var direction = new netDxf.Vector2(0, 1);
            
            var t1 = p11 + direction * wallWidth / 3;
            var t2 = direction * wallWidth / 3;
            var t3 = new Vector2();
            var t4 = p11;
            
            var t5 = p22;
            var t6 = p22 - direction * wallWidth / 3;
            var t7 =  -direction * wallWidth / 3;
            
            List<LwPolylineVertex> vertexes1 = new List<LwPolylineVertex>();
            List<LwPolylineVertex> vertexes2 = new List<LwPolylineVertex>();
            
            //左半
            LwPolylineVertex v1 = new LwPolylineVertex( new netDxf.Vector2(t1.X, t1.Y));
            LwPolylineVertex v2 = new LwPolylineVertex( new netDxf.Vector2(t2.X, t2.Y));
            LwPolylineVertex v3= new LwPolylineVertex( new netDxf.Vector2(t3.X, t3.Y));
            LwPolylineVertex v4 = new LwPolylineVertex( new netDxf.Vector2(t4.X, t4.Y));
            
            //右半
            LwPolylineVertex v5 = new LwPolylineVertex( new netDxf.Vector2(t5.X, t5.Y));
            LwPolylineVertex v6 = new LwPolylineVertex( new netDxf.Vector2(t6.X, t6.Y));
            LwPolylineVertex v7 = new LwPolylineVertex( new netDxf.Vector2(t7.X, t7.Y));

            vertexes1.AddRange(new List<LwPolylineVertex>(){ v1, v2, v3, v4, v1 });
            
            vertexes2.AddRange(new List<LwPolylineVertex>(){ v5, v6, v7, v3, v5 });

            LwPolyline polyline = new LwPolyline(vertexes1, true);
            LwPolyline polyline2 = new LwPolyline(vertexes2, true);
            
            polyline.Color = DxfConfig.Color;
            polyline2.Color = DxfConfig.Color;
            
            block.Entities.Add(polyline);
            block.Entities.Add(polyline2);
            return block;
        }
    }
}