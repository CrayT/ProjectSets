using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using netDxf.Tables;
using YW.Data.SpaceData.Extension;
using YW.Data.SpaceData.Model;
using YW.Data.SpaceData.Model.Types;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.SDK.FloorPlan.DxfPainter.Painters.Rulers;
using YW.Utils.Numerics;
using YW.Utils.SystemExtension;
using static YW.Data.SpaceData.Model.FloorPlanData;
using Vector2 = System.Numerics.Vector2;

namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    class WallPainter : IPainter
    {
        public float GraphMargin { get; set; } = 400;
        
        private static float WallLengthLimit { get; set; } = 1;
        
        public List<EntityObject> Draw(Floor floor)
        {

            //考虑墙厚，每段墙都应该去做offset
            //转角处理不太好
            var wallPolygons = new List<Vector2[]>();
            
            floor.Rooms.ForEach(room =>
            {
                if (room.Name != "客厅")
                {
                    //return;
                }
                for (var i = 0; i < room.Walls.Count; i++)
                {
                    var wall = room.Walls[i];
                    var nWall = room.Walls.Get(i + 1);
                    var bWall = room.Walls.Get(i - 1);
                    var theta = nWall.Direction.DegreeTo(wall.Direction);
                    var t = Mathf.Tan((180 - theta) / 2 * Mathf.Deg2Rad);
                    var extendLength = (theta > 91) ? wall.Width / 2 / t : 0;
                    
                    //过滤过短的墙
                    if (wall.Length < WallLengthLimit)
                    {
                        continue;
                    }
                    //思路：分别求出当前墙体和上一个和下一个墙体的相交区，再求出当前墙体切除上一个和下一个重叠区（并将裁减出的边角料剔除），然后合并三个区域
                    //右边是下一个墙厚，左边是上一个墙厚
                    var cWidth = wall.Width;
                    
                    var middle = new List<Vector2[]>()
                    {
                        wall.ExpandToP1( bWall.Width / 2).ExpandToP2( extendLength).ExtendSegmentToRetangle( cWidth / 2)
                    };
                    var nMiddle = new List<Vector2[]>()
                    {
                        nWall.ExpandToP1(extendLength).ExpandToP2( nWall.Width / 2).ExtendSegmentToRetangle( nWall.Width / 2)
                    };
                    var bMiddle = new List<Vector2[]>()
                    {
                        bWall.ExpandSegment( bWall.Width / 2 ).ExtendSegmentToRetangle( bWall.Width / 2)
                    };

                    var cross = middle.Intersect(nMiddle); //下个相交区
                    var bCross = middle.Intersect(bMiddle); //上个相交区
                    
                    var wallL = middle.Difference(nMiddle).Difference(bMiddle); //剪裁剩余部分,保留中心
                    
                    //剔除裁剪出的小拐角
                    if (wallL.Count > 1)
                    {
                        wallL = GetMiddlePolygons(wallL, nMiddle, bMiddle);
                    }
                    
                    wallPolygons = wallPolygons.Union(wallL).Union(cross).Union(bCross);
                }
            });
            floor.Rooms.ForEach(room =>
            {
                if (room.Name != "餐厅")
                {
                    //return;
                }
                room.Windows.Where(window => window.Depth != 0).ToList().ForEach(window =>
                {
                    var wall = window.GetWall(room);
                    var polygons = window.GetBayWindowPolygon(room);
                    var outPolygons = polygons.Offset( wall.Width / 2 );
                    var inPolygons = polygons.Offset( -wall.Width / 2 );
                    wallPolygons = wallPolygons.Union(outPolygons).Difference(inPolygons);
                });
            });
            
            var doorPoints = GetDoorPolygons(floor);

            //飘窗内墙
            var bayWindowPoints = GetBayWindowRec(floor);

            wallPolygons = wallPolygons
                .Difference(bayWindowPoints)
                .Difference(doorPoints)
                .Denoise();
            
            var entities = wallPolygons.Select(
                wallPoints => (EntityObject)wallPoints.ToPolyline()).ToList();
            
            return entities;
        }

        private List<Vector2[]> GetDoorPolygons(Floor floor)
        {
            return floor.Rooms.SelectMany(room => 
            {
                return room.Doors.Select(door =>
                {
                    var wall = door.GetWall(room);
                    //处理门可能在墙里面的异常情况
                    if(wall == null)
                    {
                        return new Vector2[]{ };
                    }
                    Segment segment = door.Type == DoorType.OpenSpace
                        ? ShrinkOpenSpace(room, door, wall.Width)
                        : door;
                    var width = wall.Width;
                    return segment.ExtendSegmentToRetangle( width ); //斜边情况下，extend不足，导致裁减有误
                });
            }).ToList();
        }

        private List<Vector2[]> GetBayWindowRec(Floor floor)
        {
            return floor.Rooms.SelectMany(r =>
            {
                return r.Windows.Where(w => w.Depth != 0).Select(w =>
                {
                    var wall = w.GetWall(r);
                    var width = wall.Width;
                    return w.ShrinkSegment( width / 2 ).ExtendSegmentToRetangle(width / 2); //先内缩半墙厚度，再扩展
                });
            }).ToList();
        }
        /// <summary>
        /// 收缩开放空间两端的点，使得墙体排除开放空间时在拐角处能保留正确的形状
        /// </summary>
        /// <returns></returns>
        private Segment ShrinkOpenSpace(Room room, Door door, float wallWidth)
        {
            Segment segment = new Segment(door.P1, door.P2);
            int wallIndex = room.Walls.FindIndex(wall => wall.ID.Equals(door.ParentId));

            // start
            var theta1 = room.Walls.Get(wallIndex).Theta - room.Walls.Get(wallIndex - 1).Theta;
            theta1 = (theta1 + 360) % 360;
            float d1 = 0;

            if (theta1 != 0)
            {
                d1 = wallWidth / 2 / Mathf.Tan(theta1 * Mathf.Deg2Rad / 2);
            }

            segment.P1 += segment.Direction * d1;

            // end
            var theta2 = room.Walls.Get(wallIndex + 1).Theta - room.Walls.Get(wallIndex).Theta;
            theta2 = (theta2 + 360) % 360;
            float d2 = 0;

            if (theta2 != 0)
            {
                d2 = wallWidth / 2 / Mathf.Tan(theta2 * Mathf.Deg2Rad / 2);
            }

            segment.P2 += -segment.Direction * d2;

            return segment;
        }

        private List<Vector2[]> GetMiddlePolygons(List<Vector2[]> target, List<Vector2[]> lists1,
            List<Vector2[]> lists2)
        {
            var center1 = lists1[0].GetPolyGonCenter();
            var center2 = lists2[0].GetPolyGonCenter();

            var newS = new Segment(center1, center2);
            var centerS = newS.Middle;

            var centerPoints = new List<float>();
            target = target.Where(t => t.Area() > 0.5).ToList();
            for (var i = 0; i < target.Count; i++)
            {
                var curr = target[i];
                var currCenter = curr.GetPolyGonCenter();
                var pC = currCenter.Project(newS);
                centerPoints.Add(pC.DistanceTo(centerS));
            }

            var index = 0;
            var tmpD = centerPoints[0];
            for (var i = 0; i < centerPoints.Count; i++)
            {
                if (tmpD > centerPoints[i])
                {
                    index = i;
                    tmpD = centerPoints[i];
                }
            }
            return new List<Vector2[]>(){target[index]};
        }
    }
}
