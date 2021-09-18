using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using netDxf.Blocks;
using static YW.Data.SpaceData.Model.FloorPlanData;

namespace YW.SDK.FloorPlan.DxfPainter.Painters
{
    interface IPainter
    {
        public List<EntityObject> Draw(Floor floor);
        
    } 
}
