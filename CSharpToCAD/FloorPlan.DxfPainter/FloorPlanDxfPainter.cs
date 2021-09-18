using netDxf;
using netDxf.Header;
using System.Collections.Generic;
using System.IO;
using YW.Data.SpaceData.Model;
using YW.SDK.FloorPlan.DxfPainter.Extensions;
using YW.SDK.FloorPlan.DxfPainter.Painters;
using YW.SDK.FloorPlan.DxfPainter.Painters.Compass;
using YW.SDK.FloorPlan.DxfPainter.Painters.Rulers;

namespace YW.SDK.FloorPlan.DxfPainter
{
    public class FloorPlanDxfPainter
    {

	    public static float DxfUnit { get; } = 10;

		public MemoryStream Draw(FloorPlanData floorPlanData)
        {
	        // sample: https://csharp.hotexamples.com/zh/examples/netDxf/DxfDocument/Save/php-dxfdocument-save-method-examples.html
			// 教程：https://blog.csdn.net/pistilwu/article/details/103925601
			
			DxfDocument doc = new DxfDocument(DxfVersion.AutoCad2000);
			
			floorPlanData = floorPlanData.Init();
			
			var floor = floorPlanData.Floors[0];
			var entranceDirection = floorPlanData.EntranceDirection;
			CompassPainter.EntranceDirection = entranceDirection;
			
			var painters = new List<IPainter>()
			{
				new WallPainter(),
				new DoorPainter(),
				new EntranceDoorPainter(),
				new WindowPainter(),
				new SlidingDoorPainter(),
				new BayWindowPainter(),
				new RulersPainter(),
				new CompassPainter()
			};
			painters.ForEach(painter => doc.AddEntity(painter.Draw(floor)));

			MemoryStream resultStream = new MemoryStream();

			doc.Save(resultStream); 
			resultStream.Position = 0;

			return resultStream;
		}
    }
}
