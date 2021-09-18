using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YW.Data.SpaceData.Model;
using YW.Data.SpaceData.Model.Types;
using YW.SDK.FloorPlan.DxfPainter.Config;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.Utils.Numerics;
using Vector2 = System.Numerics.Vector2;
using static YW.Data.SpaceData.Model.FloorPlanData;

namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    class DoorPainter : IPainter
    {
        public List<string> Doors {get;set;} = new List<string>();
        public float WallWidth { get; set; } = 12;
        
        public List<EntityObject> Draw(Floor floor)
        {
            var entities = new List<EntityObject>();
            var ids = new List<String>();
            floor.Rooms.SelectMany(room => room.Doors)
                .Where(door => door.Type == DoorType.NormalDoor || door.Type == DoorType.SingleDoor).ToList().ForEach(door =>
            {
                if( !ids.Contains(door.ID))
                {
                    ids.Add(door.ID);

                    Insert insert = new Insert(Draw(door.ID, door.Length, WallWidth / 2, door.IsStartSide, door.IsOpenSide));
                    insert.Rotation = 90 - door.Direction.Theta();
                    insert.Position = door.Middle.ToDxfVector3MM();
                    entities.Add(insert);

                }
            });

            return entities;
        }

        /// <summary>
        /// 以门的中点为圆心，绘制门
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="doorLength">门的长度</param>
        /// <param name="panelWidth">门板宽度</param>
        /// <param name="isMirror">是否镜像绘制</param>
        /// <returns></returns>
        public Block Draw(string blockName, float doorLength, float panelWidth, bool isMirror = false, bool isOpenSide = false)
        {
            var block = new Block(blockName);
            var startPoint = new Vector2(-doorLength / 2, 0);

            // 绘制门板
            var xVector = Vector2.UnitX * panelWidth;
            var yVector = Vector2.UnitY * doorLength;            

            var vertices = new Vector2[] {
                startPoint,
                startPoint + yVector,
                startPoint + xVector + yVector,
                startPoint + xVector};
            //轴对称 反向
            if (isMirror)
            {
                vertices = vertices.Select(v => new Vector2(-v.X, v.Y)).Reverse().ToArray();
            }
            //朝内开
            if (isOpenSide)
            {
                vertices = vertices.Select(v => new Vector2(v.X, -v.Y)).Reverse().ToArray();
            }
            var line = vertices.ToPolyline();
            line.Color = DxfConfig.Color;
            block.Entities.Add(line);

            // 绘制门开合弧线
            // cad坐标系：x轴正方向为0度，逆时针转
            Vector2 center = startPoint;
            float startAngle = 0;
            float endAngle = 90;

            if ( isMirror )
            {
                center = -center;
                if ( !isOpenSide )
                {
                    startAngle = 90;
                    endAngle = 180;
                } 
                else
                {
                    startAngle = 180;
                    endAngle = 270;
                }
            }
            if( !isMirror )
            {
                if (isOpenSide)
                {
                    startAngle = 270;
                    endAngle = 360;
                }
            }
            var arc = new Arc(center.ToDxfVector2MM(), doorLength * 10, startAngle, endAngle);
            arc.Color = DxfConfig.Color;
            block.Entities.Add(arc);

            return block;
        }
    }
}
