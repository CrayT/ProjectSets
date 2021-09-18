using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using YW.Data.SpaceData.Model;
using YW.Data.SpaceData.Model.Types;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.Utils.Numerics;
using System.Numerics;
using YW.SDK.FloorPlan.DxfPainter.Config;
using Vector2 = System.Numerics.Vector2;

namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    public class EntranceDoorPainter : IPainter
    {
        //90以内画单门
        public static float DoorWidthL1 { get; set; } = 90;
        
        //到120画双开(一大一小，1:2)
        public static float DoorWidthL2 { get; set; } = 120;

        public float WallWidth { get; set; } = 12;

        public List<EntityObject> Draw(FloorPlanData.Floor floor)
        {
            var entities = new List<EntityObject>();

            var entranceDoor = floor.Rooms.SelectMany(room => room.Doors)
                .FirstOrDefault(door => door.Type == DoorType.EntranceDoor);
            if (entranceDoor != null)
            {
                entranceDoor.IsOpenSide = false;
                Insert insert = new Insert(Draw(entranceDoor.ID, entranceDoor.Length, WallWidth / 2));
                insert.Rotation = 90 - entranceDoor.Direction.Theta();
                insert.Position = entranceDoor.Middle.ToDxfVector3MM();
                entities.Add(insert);
            }
            
            return entities;
        }

        /// <summary>
        /// 入户门暂定默认朝外
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="doorLength"></param>
        /// <param name="panelWidth"></param>
        /// <returns></returns>
        private Block Draw(string blockName, float doorLength, float panelWidth)
        {
            if (doorLength <= DoorWidthL1)
            {
                //用普通门的接口
                return new DoorPainter().Draw(blockName, doorLength, panelWidth);
            }
            if (doorLength > DoorWidthL1 && doorLength <= DoorWidthL2)
            {
                return DrawDoubleDoor(blockName, doorLength, panelWidth, (float)1/3);
            }
            return DrawDoubleDoor(blockName, doorLength, panelWidth);
        }

        private Block DrawDoubleDoor(string blockName, float doorLength, float panelWidth, float splitRatio = 0.5f)
        {
            var block = new Block(blockName);
            
            //Left Part
            var startP = new Vector2(-doorLength / 2, 0);
            var xVector = Vector2.UnitX * panelWidth;
            var yVector = Vector2.UnitY * doorLength * (1 - splitRatio);
            var vertices = new Vector2[] {
                startP,
                startP + yVector,
                startP + xVector + yVector,
                startP + xVector
            };
            var line = vertices.ToPolyline();
            line.Color = DxfConfig.Color;
            block.Entities.Add(line);
            var arc = new Arc(startP.ToDxfVector2MM(), doorLength * 10 * (1 - splitRatio), 0, 90);
            arc.Color = DxfConfig.Color;
            block.Entities.Add(arc);
            
            //Right Part
            startP = new Vector2(doorLength / 2, 0);
            xVector = Vector2.UnitX * panelWidth;
            yVector = Vector2.UnitY * doorLength * splitRatio;
            vertices = new Vector2[] {
                startP,
                startP + yVector,
                startP - xVector + yVector,
                startP - xVector
            };
            line = vertices.ToPolyline();
            line.Color = DxfConfig.Color;
            block.Entities.Add(line);
            
            arc = new Arc(startP.ToDxfVector2MM(), doorLength * 10 * splitRatio, 90, 180);
            arc.Color = DxfConfig.Color;
            block.Entities.Add(arc);
            return block;
        }
    }
}