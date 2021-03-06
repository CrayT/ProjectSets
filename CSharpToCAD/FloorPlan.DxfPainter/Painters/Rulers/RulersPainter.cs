using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Elasticsearch.Net.Specification.IndexLifecycleManagementApi;
using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using YW.Data.SpaceData.Extension;
using YW.Data.SpaceData.Model.Types;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.SDK.FloorPlan.DxfPainter.Painters.Compass;
using YW.Utils.Numerics;
using YW.Utils.SystemExtension;
using static YW.Data.SpaceData.Model.FloorPlanData;
using SVector = System.Numerics.Vector2;
namespace YW.SDK.FloorPlan.DxfPainter.Painters.Rulers
{
    public class RulersPainter : IPainter
    {
        private const float GraphMargin  = 400;
        
        private const float WallWidth  = 12;

        private const float IgnoreThread = 10;

        private const float Tolerance = 0.995f;
        
        private const float RulerMarginToWall = 640;
        
        private const float RulerMargin = 200;
        
        private const double Offset = 600;
        
        private static float XMin { get; set; }
        private static float XMax { get; set; }
        private static float YMin { get; set; }
        private static float YMax { get; set; }
        
        private static Segment TopOuterRuler { get; set; } 

        private static Segment LeftOuterRuler { get; set; }

        private static Segment RightOuterRuler { get; set; }

        private static Segment BottomOuterRuler { get; set; }

        private static SVector TopDirection { get; } = new SVector(1, 0);
        
        private static  SVector RightDirection  { get; } = new SVector(0, -1);
        
        private static SVector BottomDirection { get; } = new SVector(-1, 0);
        
        private static SVector LeftDirection { get; } = new SVector(0, 1);

        private static Layer Layer { get; } = new Layer(DxfLayer.RulerLayer) { Color = AciColor.Blue };

        public RulersPainter()
        {
            
        }
        public List<EntityObject> Draw(Floor floor)
        {
            var entities = new List<EntityObject>();

            InitBorderMaxAndMin(floor);
            
            var rooms = floor.Rooms;
            
            //?????? ??????????????????new Vector2(XMin * 10, YMin * 10)
            entities.AddRange(DrawOuterLevel());

            //?????? ????????????????????????
            //???????????????????????????????????????boundingBox(??????????????????????????????????????????)???
            entities.AddRange(DrawMiddleAndInnerLevel(rooms, true));

            //?????? ????????????
            entities.AddRange(DrawMiddleAndInnerLevel(rooms));
            
            //??????
            entities.AddRange(DrawBorder());
            
            return entities;
        }

        private List<EntityObject> DrawOuterLevel()
        {
            var entities = new List<EntityObject>();
            //Left
            entities.Add(DrawRuler( 
                LeftOuterRuler.P1.ToDxfVector2MM() - new Vector2(RulerMarginToWall, 0),
                LeftOuterRuler.P2.ToDxfVector2MM() - new Vector2(RulerMarginToWall, 0)
                , LeftDirection)
            );
            //Top
            entities.Add(DrawRuler(
                TopOuterRuler.P1.ToDxfVector2MM() + new Vector2(0, RulerMarginToWall), 
                TopOuterRuler.P2.ToDxfVector2MM() + new Vector2(0, RulerMarginToWall), 
                TopDirection)
            );
            //Right
            entities.Add(DrawRuler(
                RightOuterRuler.P1.ToDxfVector2MM() + new Vector2(RulerMarginToWall, 0), 
                RightOuterRuler.P2.ToDxfVector2MM() + new Vector2(RulerMarginToWall, 0), 
                RightDirection)
            );
            //Bottom
            entities.Add(DrawRuler(
                BottomOuterRuler.P1.ToDxfVector2MM() - new Vector2(0, RulerMarginToWall), 
                BottomOuterRuler.P2.ToDxfVector2MM() - new Vector2(0, RulerMarginToWall), 
                BottomDirection)
            );
            
            return entities;
        }

        private List<EntityObject> DrawMiddleAndInnerLevel(List<Room> rooms, bool isMiddle = false)
        {
            var entities = new List<EntityObject>();

            int offset = isMiddle ? 1 : 2; //???????????????
            
            foreach(RulerDirection ruler in Enum.GetValues(typeof(RulerDirection)))
            {
                //inner????????????????????????
                var elements = GetElements(rooms, ruler, isMiddle);
                var toProjectSegment = new Segment();
                var direction = new SVector();
                switch (ruler)
                {
                    case RulerDirection.BottomRuler:
                        direction = BottomDirection;
                        toProjectSegment = BottomOuterRuler + new SVector(0, - RulerMarginToWall / 10 + offset * RulerMargin / 10);
                        break;
                    case RulerDirection.LeftRuler:
                        direction = LeftDirection;
                        toProjectSegment = LeftOuterRuler + new SVector( - RulerMarginToWall / 10 + offset * RulerMargin / 10, 0);
                        break;
                    case RulerDirection.RightRuler:
                        direction = RightDirection;
                        toProjectSegment = RightOuterRuler + new SVector(  RulerMarginToWall / 10 - offset * RulerMargin / 10, 0);
                        break;
                    case RulerDirection.TopRuler:
                        direction = TopDirection;
                        toProjectSegment = TopOuterRuler + new SVector(0,  RulerMarginToWall / 10 - offset * RulerMargin / 10);
                        break;
                }
                elements.ForEach(ele =>
                {
                    var p1 = ele.P1.Project(toProjectSegment).ToDxfVector2MM();
                    var p2 = ele.P2.Project(toProjectSegment).ToDxfVector2MM();
                    entities.Add(DrawRuler(p1, p2, direction));
                });
            }

            return entities;
        }

        /// <summary>
        /// ????????????????????????????????????????????????????????????
        /// </summary>
        /// <param name="rooms"></param>
        /// <returns></returns>
        private List<EntityObject> DrawMiddleLevel(List<Room> rooms)
        {
            var entities = new List<EntityObject>();
            foreach (RulerDirection ruler in Enum.GetValues(typeof(RulerDirection)))
            {
                var walls = GetProjectWalls(rooms, ruler);
                var toProjectSegment = new Segment();
                var direction = new SVector();
                switch (ruler)
                {
                    case RulerDirection.BottomRuler:
                        direction = BottomDirection;
                        toProjectSegment = BottomOuterRuler + new SVector(0, - RulerMarginToWall / 10 + RulerMargin / 10);
                        break;
                    case RulerDirection.LeftRuler:
                        direction = LeftDirection;
                        toProjectSegment = LeftOuterRuler + new SVector( - RulerMarginToWall / 10 + RulerMargin / 10, 0);
                        break;
                    case RulerDirection.RightRuler:
                        direction = RightDirection;
                        toProjectSegment = RightOuterRuler + new SVector(  RulerMarginToWall / 10 - RulerMargin / 10, 0);
                        break;
                    case RulerDirection.TopRuler:
                        direction = TopDirection;
                        toProjectSegment = TopOuterRuler + new SVector(0,  RulerMarginToWall / 10 - RulerMargin / 10);
                        break;
                }
                walls.ForEach(wall =>
                {
                    var p1 = wall.P1.Project(toProjectSegment).ToDxfVector2MM();
                    var p2 = wall.P2.Project(toProjectSegment).ToDxfVector2MM();
                    entities.Add(DrawRuler(p1, p2, direction));
                });
            }

            return entities;
        }
        /// <summary>
        /// ???????????????????????????????????????????????????????????????????????????????????????.?????????????????????????????????????????????????????????????????????????????????
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="direction">???????????????</param>
        /// <param name="direction2">?????????????????????????????????????????????</param>
        /// <returns></returns>
        private List<Segment> GetProjectWalls(List<Room> rooms, RulerDirection rulerDirection)
        {
            var edges = new List<Segment>();
            var direction = new Segment();
            
            foreach (var room in rooms)
            {
                var boundingBox = room.GetInnerMaxAndMin();
                var segment = new Segment();
                
                switch (rulerDirection)
                {
                    case RulerDirection.BottomRuler:
                        direction = BottomOuterRuler - new SVector(0, RulerMarginToWall / 10);
                        segment = new Segment(new SVector(boundingBox["max"].X,boundingBox["min"].Y), new SVector(boundingBox["min"].X, boundingBox["min"].Y));
                        break;
                    case RulerDirection.LeftRuler:
                        direction = LeftOuterRuler - new SVector(RulerMarginToWall / 10,0 );
                        segment = new Segment(new SVector(boundingBox["min"].X,boundingBox["min"].Y), new SVector(boundingBox["min"].X, boundingBox["max"].Y));
                        break;
                    case RulerDirection.RightRuler:
                        direction = RightOuterRuler + new SVector(RulerMarginToWall / 10,0) ;
                        segment = new Segment(new SVector(boundingBox["max"].X,boundingBox["min"].Y), new SVector(boundingBox["max"].X, boundingBox["max"].Y));
                        break;
                    case RulerDirection.TopRuler:
                        direction = TopOuterRuler + new SVector(0, RulerMarginToWall / 10);
                        segment = new Segment(new SVector(boundingBox["min"].X,boundingBox["max"].Y), new SVector(boundingBox["max"].X, boundingBox["max"].Y));
                        break;
                }
                edges.Add(segment);
            }

            List<Segment> newEdges = edges.OrderBy(ed => ed.P1.DistanceTo(direction)).ToList();

            var walls = GetProjectSegments(newEdges, direction, rooms, rulerDirection);

            return walls;
        }

        private List<Segment> GetElements(List<Room> rooms, RulerDirection rulerDirection, bool isMiddle = false)
        {
            
            var direction = new Segment();
            var wallIDs = new List<string>();
            switch (rulerDirection)
            {
                case RulerDirection.BottomRuler:
                    direction = BottomOuterRuler - new SVector(0, RulerMargin / 10);
                    break;
                case RulerDirection.LeftRuler:
                    direction = LeftOuterRuler - new SVector(RulerMargin / 10,0 );
                    break;
                case RulerDirection.RightRuler:
                    direction = RightOuterRuler + new SVector(RulerMargin / 10,0) ;
                    break;
                case RulerDirection.TopRuler:
                    direction = TopOuterRuler + new SVector(0, RulerMargin / 10);
                    break;
            }
            
            //??????????????????????????????????????????(??????)???Segment??????????????????????????????
            var allSegments = isMiddle ? new List<Segment>() : rooms.SelectMany(r => r.Doors).Where(d => d.Type != DoorType.OpenSpace).Select(r => new Segment(r.P1, r.P2)).ToList();

            rooms.ForEach(room =>
            {

                //?????????
                if ( !isMiddle )
                {
                    room.Windows.ForEach(window =>
                    {
                        if (window.Depth != 0)
                        {
                            var wall = window.GetWall(room);
                            var polygons = window.GetBayWindowPolygon(room);
                            polygons = polygons.Offset( -wall.Width / 2);
                            var inner = new List<Segment>();
                            for (var i = 0; i < polygons[0].Length - 1; i++)
                            {
                                inner.Add(new Segment(polygons[0][i], polygons[0][i + 1]));
                            }
                            inner.Add(new Segment(polygons[0][3], polygons[0][0]));
                        
                            allSegments.AddRange(inner);
                        }
                        else
                        {
                            allSegments.AddRange(new List<Segment>(){ new Segment(window.P1, window.P2)});
                        }
                    });
                }
                //???????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
                //????????????????????????????????????????????????????????????
                else
                {
                    room.Windows.ForEach( window =>
                    {
                        if ( window.Depth != 0 )
                        {
                            var windowDirection = -window.Direction.Perpendicular();
                            
                            var wall = window.GetWall( room );
                            if ( wall != null )
                            {
                                for (var i = 0; i < room.Walls.Count; i++)
                                {
                                    var cWall = room.Walls[i];
                                    
                                    //?????????????????????????????????
                                    if ( cWall.Direction.IsParallel(windowDirection) )
                                    {
                                        //?????????????????????window????????????????????????
                                        var wallIndex = room.Walls.IndexOf( wall );
                                        if ( SVector.Dot(wall.Direction, cWall.Direction).ApproxEquals(0) ) //??????
                                        {
                                            var find = 0;
                                            if ( wallIndex - i  == 1 || wallIndex - i == -(room.Walls.Count - 1) ) //?????????,??????
                                            {
                                                find = 1;
                                            }
                                            else if ( wallIndex - i == -1 || wallIndex - i == (room.Walls.Count - 1)  ) //??????
                                            {
                                                find = 2;
                                            }
                                            if ( find != 0 )
                                            {
                                                wallIDs.Add(cWall.ID);
                                                var newS = find == 1 ? cWall.ExpandToP2(window.Depth) : cWall.ExpandToP1(window.Depth);
                                                var newWall = new Wall()
                                                {
                                                    P1 = newS.P1,
                                                    P2 = newS.P2,
                                                    ID = UUID.NewUUID()
                                                };
                                                var newL = newWall.GetWallPolygons(room);
                                                var cInner = new Segment(newL[0][3], newL[0][2]); //3 4??????????????????
                                                var a = new List<Segment>() {cInner};
                                                allSegments.AddRange(a);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
                //?????????
                for (var i = 0; i < room.Walls.Count; i++)
                {
                    var wall = room.Walls[i];
                    if ( wallIDs.Contains(wall.ID) ) //???????????????????????????????????????
                    {
                        continue;
                    }
                    var middle = wall.GetWallPolygons(room);
                    var cInner = new Segment(middle[0][3], middle[0][2]); //3 4??????????????????
                    var a = new List<Segment>() {cInner};
                    allSegments.AddRange(a);
                }
            });
            var filterSegments = allSegments.Where(s => SVector.Dot(s.Direction, direction.Direction) > Tolerance).ToList();
            
            List<Segment> newEdges = filterSegments.OrderBy(ed => ed.P1.DistanceTo(direction)).ToList();

            var wallSegments = GetProjectSegments(newEdges, direction, rooms, rulerDirection);
            
            //????????????, ??????????????????????????????????????????????????????, ?????????????????????????????????????????????????????????????????????
            if (!isMiddle)
            {
                var wallWidthList = new List<Segment>();
                wallWidthList.AddRange(direction.SubtractSegment(wallSegments.First()).Where(w => w.Length > IgnoreThread));
                wallSegments.ForEach(w =>
                {
                    wallWidthList = wallWidthList.SelectMany(t => t.SubtractSegment(w)).Where(w => w.Length > IgnoreThread).ToList();
                });
                wallWidthList = wallWidthList.Where(w => w.Length > IgnoreThread).ToList();

                wallSegments.AddRange(wallWidthList);
            }

            return wallSegments;
        }

        //????????????????????????
        private List<Segment> GetProjectSegments(List<Segment> segments, Segment direction, List<Room> rooms, RulerDirection rulerDirection)
        {
            List<Segment> projectEdges = new List<Segment>();
            foreach (var newEdge in segments)
            {
                //????????????????????????
                if (projectEdges.Any(pe => newEdge.P1.IsProjectInSegment(pe)) &&
                    projectEdges.Any(pe => newEdge.P2.IsProjectInSegment(pe)))
                {
                    //TODO
                    continue;
                }
                
                //???????????????, ???????????????P1 ??? P2???????????????
                if ( projectEdges.All(pe => !newEdge.P1.IsProjectInSegment(pe) || newEdge.P1.Project(direction).Equals(pe.P2) ) &&
                     projectEdges.All(pe => !newEdge.P2.IsProjectInSegment(pe) || newEdge.P2.Project(direction).Equals(pe.P1)) )
                {
                    //??????
                    if ( projectEdges.Any(pe => pe.P1.IsProjectInSegment(newEdge) && pe.P2.IsProjectInSegment(newEdge)) )
                    {
                        var corr = projectEdges.FindAll(pe =>
                            pe.P1.IsProjectInSegment(newEdge) && pe.P2.IsProjectInSegment(newEdge));
                        if (corr.Count != 0)
                        {
                            corr = corr.OrderBy(c => c.P1.DistanceTo(newEdge.P1)).ToList();
                            
                            var i = IsBlockedByWall(newEdge.P1, rooms, direction);
                            
                            if (!i)
                            {
                                projectEdges.Add(new Segment(newEdge.P1.Project(direction), corr.First().P1.Project(direction)));
                            }
                            
                            var ii = IsBlockedByWall(newEdge.P2, rooms, direction);
                            if (!ii)
                            {
                                projectEdges.Add(new Segment(corr.Last().P2.Project(direction), newEdge.P2.Project(direction)));
                            }
                            
                        }

                        continue;
                    }
                    //????????????
                    if ( projectEdges.Any(pe => pe.P1.IsProjectInSegment(newEdge)))
                    {
                        var corr = projectEdges.Find(pe => pe.P1.IsProjectInSegment(newEdge));
                        if (corr != null)
                        {
                            var i = IsBlockedByWall(newEdge.P1, rooms, direction);
                            if (!i)
                            {
                                projectEdges.Add(new Segment(newEdge.P1.Project(direction), corr.P1.Project(direction)));
                               
                            }
                            continue;
                            
                        }
                    }
                    if ( projectEdges.Any(pe => pe.P2.IsProjectInSegment(newEdge)))
                    {
                        var corr = projectEdges.Find(pe => pe.P2.IsProjectInSegment(newEdge));
                        if (corr != null)
                        {
                            var i = IsBlockedByWall(newEdge.P1, rooms, direction);
                            //todo ???????????????????????????, ????????????????????????????????????????????????
                            var isO = IsOnOuterWall(newEdge, newEdge.P1, rooms, direction, rulerDirection);
                            if (!i || (i && isO) )
                            {
                                projectEdges.Add(new Segment(corr.P2.Project(direction), newEdge.P2.Project(direction)));
                               
                            }
                            
                            continue;
                        }
                    }
                    //?????????
                    if( projectEdges.All( pe => !pe.P1.IsProjectInSegment(newEdge) && !pe.P2.IsProjectInSegment(newEdge)) )
                    {
                        projectEdges.Add(new Segment(newEdge.P1.Project(direction), newEdge.P2.Project(direction)));
                    }

                    continue;
                }
                //??????????????????????????????????????? + ?????????;
                //??????P2
                if ( projectEdges.All(pe => !newEdge.P1.IsProjectInSegment(pe)) &&
                     projectEdges.Any(pe => newEdge.P2.IsProjectInSegment(pe)))
                {
                    var projectE = new Segment(newEdge.P1.Project(direction), newEdge.P2.Project(direction));
                    //????????????
                    var anotherPs = projectEdges.FindAll(p => p.P1.IsProjectInSegment(newEdge) ).OrderBy(p => p.P1.DistanceTo(projectE.P1)).ToList();

                    var tmp = new List<Segment>();
                    projectEdges.ForEach(p =>
                    {
                        tmp.AddRange(newEdge.SubtractSegment(p));
                    });
                    var s = IsBlockedByWall(newEdge.P1, rooms, direction);
                    
                    if (!s)
                    {
                        projectEdges.Add(new Segment(newEdge.P1.Project(direction), anotherPs.First().P1.Project(direction)));
                    }
                    
                    continue;
                }
                //??????P1
                if ( projectEdges.Any(pe => newEdge.P1.IsProjectInSegment(pe)) &&
                     projectEdges.All(pe => !newEdge.P2.IsProjectInSegment(pe)))
                {
                    
                    var projectE = new Segment(newEdge.P1.Project(direction), newEdge.P2.Project(direction));
                    
                    var p = projectEdges.FindAll(p => p.P2.IsProjectInSegment(projectE)).OrderBy(p => p.P2.DistanceTo(projectE.P2)).ToList();
                    var s = IsBlockedByWall(newEdge.P2, rooms, direction);
                    var isO = IsOnOuterWall(newEdge, newEdge.P1, rooms, direction, rulerDirection);
                    if (!s || (s && isO))
                    {
                        projectEdges.Add(new Segment(p.First().P2.Project(direction), newEdge.P2.Project(direction)));
                    }
                    
                }   
            }
            
            return projectEdges.Where(p => p.Length >= 1).ToList();
        }

        private LinearDimension DrawRuler(Vector2 p1, Vector2 p2, SVector direction)
        {
            var rotation = 0;
            if (direction == LeftDirection)
            {
                rotation = 90;
            }
            else if (direction == BottomDirection)
            {
                rotation = 180;
            }
            else if(direction == RightDirection)
            {
                rotation = 270;
            }
            LinearDimension dim = new LinearDimension(new Line(p1, p2),  Offset, rotation, DimStyle.RulerStyle)
                {Layer = Layer};
            
            return dim;
        }

        //????????????, ????????????
        private void InitBorderMaxAndMin(Floor floor)
        {
            XMin = float.MaxValue;
            XMax = float.MinValue;
            YMin = float.MaxValue;
            YMax = float.MinValue;
            
            floor.Rooms.ForEach(room =>
            {
                var box = room.GetInnerMaxAndMin();
                XMin = Math.Min(XMin, box["min"].X);
                XMax = Math.Max(XMax, box["max"].X);
                YMin = Math.Min(YMin, box["min"].Y);
                YMax = Math.Max(YMax, box["max"].Y);
            });
            //?????????????????????
            TopOuterRuler = new Segment(new SVector(XMin, YMax), new SVector(XMax, YMax));
            LeftOuterRuler = new Segment(new SVector(XMin, YMin), new SVector(XMin, YMax));
            RightOuterRuler = new Segment(new SVector(XMax, YMax), new SVector(XMax, YMin));
            BottomOuterRuler = new Segment(new SVector(XMax, YMin), new SVector(XMin, YMin));
            
            var doors = floor.Rooms.SelectMany(r => r.Doors).ToList().Where(d => d.IsOpened 
                && (d.Type == DoorType.EntranceDoor || d.Type == DoorType.NormalDoor)).ToList();
            var left = false;
            var right = false;
            var top = false;
            var bottom = false;
            doors.ForEach(door =>
            {
                var p = door.P1 - door.Direction.Perpendicular() * door.Length;
                //?????????????????????
                if (p.Y > YMin && p.Y < YMax && p.X > XMin && p.X < XMax)
                {
                    return;
                }
                var rulers = new List<Segment>() {TopOuterRuler, BottomOuterRuler, LeftOuterRuler, RightOuterRuler};
                var rr = rulers.Where(r => p.Project(r).IsOnSegment(r)).ToList().OrderBy(r => p.DistanceTo(p.Project(r)))?.First();
                if (rr != null)
                {
                    if ( !top && rr.Direction == TopDirection)
                    {
                        TopOuterRuler += new SVector(0, p.Y - door.P1.Y);
                        top = true;
                    } 
                    else if ( !bottom && rr.Direction == BottomDirection)
                    {
                        BottomOuterRuler += new SVector(0, p.Y - door.P1.Y);
                        bottom = true;
                    }
                    else if ( !left && rr.Direction == LeftDirection)
                    {
                        LeftOuterRuler += new SVector(p.X - door.P1.X, 0);
                        left = true;
                    }
                    else if ( !right && rr.Direction == RightDirection)
                    {
                        RightOuterRuler += new SVector(p.X - door.P1.X, 0);
                        right = true;
                    }
                }
            });
        }

        private List<EntityObject> DrawBorder()
        {
            
            var entities = new List<EntityObject>();
            
            var xMin = XMin - GraphMargin;
            var xMax = XMax + GraphMargin;
            var yMin = YMin - GraphMargin;
            var yMax = YMax + GraphMargin;
            
            CompassPainter.XMin = xMin * FloorPlanDxfPainter.DxfUnit;
            CompassPainter.XMax = xMax * FloorPlanDxfPainter.DxfUnit;
            CompassPainter.YMin = yMin * FloorPlanDxfPainter.DxfUnit;
            CompassPainter.YMax = yMax * FloorPlanDxfPainter.DxfUnit;
            
            var line = new LwPolyline(new List<LwPolylineVertex>()
            {
                new LwPolylineVertex(new SVector(xMin, yMin).ToDxfVector2MM()),
                new LwPolylineVertex(new SVector(xMin, yMax).ToDxfVector2MM()),
                new LwPolylineVertex(new SVector(xMax, yMax).ToDxfVector2MM()),
                new LwPolylineVertex(new SVector(xMax, yMin).ToDxfVector2MM()),
                new LwPolylineVertex(new SVector(xMin, yMin).ToDxfVector2MM())
            }, true);

            entities.Add(line);
            
            return entities;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public bool IsBlockedByWall(SVector p, List<Room> rooms, Segment direction)
        {
            bool isIntersect = false;
            var cRoom = p.GetRoomBySegment(rooms);
            for (var i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                if (room == cRoom)
                {
                    continue;
                }
                for (var j = 0; j < room.Walls.Count; j++)
                {
                    var wall = room.Walls[j];
                    var nWall = room.Walls.Get(j + 1);
                    var bWall = room.Walls.Get(j - 1);
                    
                    //???????????????????????????????????????????????????
                    var cWidth = wall.Width != 0 ? wall.Width : 12;
                    var nWidth = nWall.Width != 0 ? nWall.Width : 12;
                    var bWidth = bWall.Width != 0 ? bWall.Width : 12;
                    
                    var wallRec = new List<SVector[]>()
                    {
                        wall.ExpandToP1( bWidth / 2).ExpandToP2( nWidth / 2).ExtendSegmentToRetangle( cWidth / 2)
                    };
                    var pP = p.Project(direction);
                    
                    var nP = new Segment(p, pP);
                    var s1 = new Segment(wallRec[0][0], wallRec[0][1]);
                    var s2 = new Segment(wallRec[0][1], wallRec[0][2]);
                    var s3 = new Segment(wallRec[0][2], wallRec[0][3]);
                    var s4 = new Segment(wallRec[0][3], wallRec[0][0]);
                    
                    var is1 = nP.IsIntersect(s1);
                    var is2 = nP.IsIntersect(s2);
                    var is3 = nP.IsIntersect(s3);
                    var is4 = nP.IsIntersect(s4);

                    if (is1 || is2 || is3 || is4)
                    {
                        isIntersect = true;
                        break;
                    }
                }

                if (isIntersect)
                {
                    break;
                }
            }

            return isIntersect;
        }

        /// <summary>
        /// ???p????????? ?????????direction????????????????????????
        /// ??????????????????segment?????????wall?????????
        /// ?????????direction?????????p????????????wall
        /// </summary>
        /// <param name="p"></param>
        /// <param name="rooms"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private bool IsOnOuterWall(Segment s, SVector p, List<Room> rooms, Segment direction, RulerDirection rulerDirection)
        {
            var isOut = false;
            switch (rulerDirection)
            {
                case RulerDirection.LeftRuler:
                    var i1 = IsBlockedByWall(p, rooms, TopOuterRuler);
                    var i2 = IsBlockedByWall(p, rooms, BottomOuterRuler);
                    isOut = i1 || i2;
                    break;
                case RulerDirection.BottomRuler:

                    break;
            }
            return isOut;
        }
    }
}