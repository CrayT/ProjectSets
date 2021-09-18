using System.Collections.Generic;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;
using YW.Data.SpaceData.Model;

namespace YW.SDK.FloorPlan.DxfPainter.Painters.Compass
{
    public class CompassPainter : IPainter
    {
        public static float XMin {get; set; }
        public static float XMax {get; set; }
        public static float YMin {get; set; }
        public static float YMax {get; set; }

        private static float Margin { get; } = 2000;

        private static float Radius { get; } = 400;

        public static float EntranceDirection { get; set; } = 0;

        private static Vector3 CompassPosition => new Vector3(XMin + Margin, YMax - Margin, 0);
        
        public List<EntityObject> Draw(FloorPlanData.Floor floor)
        {
            var entities = new List<EntityObject>();
            
            Insert insert = new Insert(Draw());
            insert.Rotation = -EntranceDirection; //逆时针为正
            insert.Position = CompassPosition;

            //文字不需要旋转，需要单独
            Insert textInsert = new Insert(DrawT());
            textInsert.Rotation = 0;
            textInsert.Position = CompassPosition;
            
            entities.Add(insert);
            entities.Add(textInsert);
            
            return entities;
        }

        private Block Draw()
        {
            Block block = new Block("Compass");
            
            block.Entities.AddRange(DrawArc());
            block.Entities.AddRange(DrawTriangle());
            block.Entities.Add(DrawHatch());
            return block;
        }

        private Block DrawT()
        {
            Block block = new Block("Text");
            block.Entities.Add(DrawText());
            return block;
        }
        /// <summary>
        /// 绘制外圈三段圆弧
        /// </summary>
        /// <returns></returns>
        private List<Arc> DrawArc()
        {
            var arcLists = new List<Arc>();
            var arc1 = new Arc(new Vector2(), Radius, 106, 224);
            
            var arc2 = new Arc(new Vector2(), Radius, 246, 294);
            
            var arc3 = new Arc(new Vector2(), Radius, 316, 74);
            
            arcLists.Add(arc1);
            arcLists.Add(arc2);
            arcLists.Add(arc3);
            
            return arcLists;
        }
        /// <summary>
        /// 绘制内部线段
        /// </summary>
        /// <returns></returns>
        private List<LwPolyline> DrawTriangle()
        {
            var triLists = new List<LwPolyline>();
            LwPolyline line1 = new LwPolyline(new List<LwPolylineVertex>()
            {
                new LwPolylineVertex( new Vector2(0, -133)),
                new LwPolylineVertex( new Vector2(0, 800)),
                new LwPolylineVertex( new Vector2(404, -709)),
                new LwPolylineVertex( new Vector2(0, -133))
            }, false);
            
            triLists.Add(line1);

            return triLists;
        }

        private Hatch DrawHatch()
        {
            HatchPattern pattern = new HatchPattern("ANSI", new List<HatchPatternLineDefinition>()
            {
                new HatchPatternLineDefinition(){Delta =  new Vector2(1,50)}
            });
            pattern.Scale = 1;
            pattern.Angle = 30;
 
            LwPolyline poly = new LwPolyline();
            poly.Vertexes.Add(new LwPolylineVertex(0, -133));
            poly.Vertexes.Add(new LwPolylineVertex(0, 800));
            poly.Vertexes.Add(new LwPolylineVertex(-404, -709));
            poly.Vertexes.Add(new LwPolylineVertex(0, -133));
            poly.IsClosed = true;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {poly})
            };
            Hatch hatch = new Hatch(pattern, boundary, true);
            return hatch;
        }

        private Text DrawText()
        {
            Text text = new Text("北", new Vector2(-140, 1000), 200);
            text.Style = new TextStyle("True type font","Arial.ttf");
            text.Color = AciColor.Cyan;
            return text;
        }
    }
}