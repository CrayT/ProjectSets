using System.Collections.Generic;
using System.Numerics;
using YW.Data.SpaceData.Extension;
using YW.Data.SpaceData.Model;
using YW.Utils.Numerics;
using YW.Utils.SystemExtension;

namespace YW.SDK.FloorPlan.DxfPainter.Extensions
{
    public static class FloorPlanDataExt
    {
        public static FloorPlanData Init(this FloorPlanData floorPlanData)
        {
            floorPlanData.Floors.ForEach(floor =>
            {
                floor.Rooms.ForEach(room =>
                {
                    for (var i = room.Walls.Count - 1; i > -1 ; i--)
                    {
                        var cW = room.Walls[i];
                        var nW = room.Walls.Get(i - 1);
                        //合并几乎同向 且墙厚相同的墙，可以认为是导出的数据不完美
                        if (cW.Direction.ApproxEquals(nW.Direction) && cW.Width == nW.Width)
                        {
                            var start = i;
                            while (cW.Direction.ApproxEquals(nW.Direction) && cW.Width == nW.Width)
                            {
                                i -= 1;
                                nW = room.Walls.Get(i - 1);
    
                            }
                            nW = room.Walls.Get(i);
                            //合并
                            var newW = new FloorPlanData.Wall()
                            {
                                P1 = nW.P1,
                                P2 =  cW.P2,
                                ID = UUID.NewUUID(),
                                Width = cW.Width
                            };
                            room.Walls.RemoveRange(i, start - i + 1);
                            room.Walls.Insert(i, newW);
                        }
                    }
                });
            });
            return floorPlanData;
        }

        public static List<Vector2[]> GetBayWindowPolygon(this FloorPlanData.Window window, FloorPlanData.Room room)
        {
            var depth = window.Depth;
            var direction = window.Direction.Perpendicular();
            return new List<Vector2[]>()
            {
                new []
                {
                    window.P1,
                    window.P1 - direction * depth,
                    window.P2 - direction * depth,
                    window.P2
                }
            };
        }
        /// <summary>
        /// 墙的polygon
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<Vector2[]> GetWallPolygons(this FloorPlanData.Wall wall, FloorPlanData.Room room)
        {
            var wallWidth = 12;
            var i = room.Walls.IndexOf(wall);
            var nextWall = room.GetNextWall(i, 1);
            var beforeWall = room.GetNextWall(i, -1);

            var cWidth = wall.Width != 0 ? wall.Width : wallWidth;
            var nWidth = nextWall.Width != 0 ? nextWall.Width : wallWidth;
            var bWidth = beforeWall.Width != 0 ? beforeWall.Width : wallWidth;
            var middle = new List<Vector2[]>()
                    
            {
                wall.ExpandToP1( -bWidth / 2).ExpandToP2( -nWidth / 2 ).ExtendSegmentToRetangle( cWidth / 2)
            };

            return middle;
        }
    }
}