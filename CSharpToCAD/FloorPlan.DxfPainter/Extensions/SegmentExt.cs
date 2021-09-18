using System.Collections.Generic;
using System.Numerics;
using YW.Data.SpaceData.Model;
using YW.Data.SpaceData.Model.Mesh;
using YW.Utils.Numerics;
using YW.Utils.SystemExtension;

namespace YW.SDK.FloorPlan.DxfPainter.Extensions
{
    public static class SegmentExt
    {
        /// <summary>
        /// 内墙内，获取点所在房间,通过判断点是否在内墙线上
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rooms"></param>
        /// <returns></returns>
        public static FloorPlanData.Room GetRoomBySegment(this Vector2 v, List<FloorPlanData.Room> rooms)
        {
            
            for (var i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                
                for (var j = 0; j < room.Walls.Count; j++)
                {
                    var wall = room.Walls[j];
                    var nWall = room.Walls.Get(j + 1);
                    var bWall = room.Walls.Get(j - 1);

                    //右边是下一个墙厚，左边是上一个墙厚
                    var cWidth = wall.Width != 0 ? wall.Width : 12;
                    var nWidth = nWall.Width != 0 ? nWall.Width : 12;
                    var bWidth = bWall.Width != 0 ? bWall.Width : 12;

                    var wallRec = new List<Vector2[]>()
                    {
                        wall.ExpandToP1(-bWidth / 4).ExpandToP2(-nWidth / 4) //偏移1/4 防止精度问题导致判断出错
                            .ExtendSegmentToRetangle(cWidth / 2)
                    };

                    var s1 = new Segment(wallRec[0][0], wallRec[0][1]);
                    var s2 = new Segment(wallRec[0][1], wallRec[0][2]);
                    var s3 = new Segment(wallRec[0][2], wallRec[0][3]);
                    var s4 = new Segment(wallRec[0][3], wallRec[0][0]);

                    var is1 = v.IsOnSegment(s1);
                    var is2 = v.IsOnSegment(s2);
                    var is3 = v.IsOnSegment(s3);
                    var is4 = v.IsOnSegment(s4);
                    var isIn = v.IsInPolygon(wallRec[0]);

                    if (is1 || is2 || is3 || is4 || isIn)
                    {
                        return room;
                    }
                }
            }
            
            return rooms.Find(r => v.IsInPolygon(r.Middle[0]));
        }

        public static Vector2 GetPolyGonCenter(this Vector2[] polyGon)
        {
            var center1 = new Vector2();

            for (var i = 0; i < polyGon.Length; i++)
            {
                center1 += polyGon[i];
            }

            center1 = center1 / polyGon.Length;
            return center1;
        }
    }
}