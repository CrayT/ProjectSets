using System.Collections.Generic;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;
using YW.SDK.FloorPlan.DxfPainter.Config;

namespace YW.SDK.FloorPlan.DxfPainter.Painters.Rulers
{
    public class DimStyle
    {
        
        public static DimensionStyle RulerStyle { get
        {
            DimensionStyle RulerStyle = new DimensionStyle("123");
            RulerStyle.TextOutsideAlign = false;
            RulerStyle.TextInsideAlign = false;
            RulerStyle.ArrowSize = 0.1;
            RulerStyle.DimScaleOverall = 500;

            RulerStyle.TextStyle = new TextStyle("True type font", "SimSun.ttf");
            RulerStyle.FitDimLineForce = true;
            RulerStyle.FitDimLineInside = true;
            
            RulerStyle.TextHeight = 0.18;
            RulerStyle.TextColor = AciColor.Cyan;
            RulerStyle.DimLineColor = AciColor.Cyan;
            RulerStyle.ExtLineColor = AciColor.Cyan;
            RulerStyle.TextOffset = 0.05;
            RulerStyle.LengthPrecision = 0;
            
            RulerStyle.DimArrow1 = Arrow;
            RulerStyle.DimArrow2 = Arrow;
            RulerStyle.LeaderArrow = LeaderArrow;
            
            RulerStyle.CenterMarkSize = 0.09;           
            RulerStyle.LeaderArrow = Arrow;
            RulerStyle.TextOutsideAlign = false;
            RulerStyle.DimLine1Off = false;
            RulerStyle.DimLine2Off = false;

            RulerStyle.ExtLine1Off = false;
            RulerStyle.ExtLine2Off = false;
            RulerStyle.ExtLineOffset = 0.5;
            
            RulerStyle.FitTextMove = DimensionStyleFitTextMove.OverDimLineWithoutLeader;
            RulerStyle.FitTextInside = true;
            RulerStyle.FitOptions = DimensionStyleFitOptions.TextAndArrows;
            RulerStyle.SuppressAngularLeadingZeros = true;
            RulerStyle.SuppressAngularTrailingZeros = true;
            RulerStyle.ExtLine1Off = false;
            RulerStyle.ExtLine2Off = false;

            RulerStyle.ExtLineExtend = 0.1; 
            RulerStyle.ExtLineFixed = false; 
            RulerStyle.ExtLineFixedLength  = 1; 
            RulerStyle.DimScaleLinear  = 1; 

            RulerStyle.Tolerances =  new DimensionStyleTolerances();

            RulerStyle.TextHorizontalPlacement = DimensionStyleTextHorizontalPlacement.Centered;
            RulerStyle.TextVerticalPlacement = DimensionStyleTextVerticalPlacement.Above;
            RulerStyle.DimBaselineSpacing = 0.38;
            RulerStyle.DimLineExtend = 0;
            RulerStyle.DimRoundoff = 0;
            RulerStyle.SuppressLinearLeadingZeros = false;
            RulerStyle.SuppressLinearTrailingZeros = false;

            RulerStyle.SuppressZeroFeet = true;
            RulerStyle.SuppressZeroInches = true;
            RulerStyle.TextFractionHeightScale = 1;

            return RulerStyle;
        }}
        
        public static Block Arrow
        {
            get
            {
                var block = new Block("_ArchTick");
                var lv1 = new LwPolylineVertex(new Vector2(-0.5, -0.5));
                var lv2 = new LwPolylineVertex(new Vector2(0.5, 0.5));
                lv1.StartWidth = 0.15;
                lv1.EndWidth = 0.15;
                lv1.Bulge = 0;
                lv1.Position = new Vector2(-0.5, -0.5);
                lv2.Position = new Vector2(0.5, 0.5);
                lv2.StartWidth = 0.15;
                lv2.EndWidth = 0.15;
                lv2.Bulge = 0;
                
                var line1 = new LwPolyline(new List<LwPolylineVertex>(){ lv1, lv2 }, false);
                
                line1.Normal = new Vector3(0, 0, 1);
                line1.Color = DxfConfig.Color;
                block.Entities.Add(line1);
                block.Origin = new Vector3(0, 0, 0);
                return block;
            }
        }
        public static Block LeaderArrow
        {
            get
            {
                var block = new Block("LeaderArrow");
                var lv1 = new LwPolylineVertex(new Vector2(-0.5, -0.5));
                var lv2 = new LwPolylineVertex(new Vector2(0.5, 0.5));
                lv1.StartWidth = 0.15;
                lv1.EndWidth = 0.15;
                lv1.Position = new Vector2(-0.5, -0.5);
                lv2.StartWidth = 0.15;
                lv2.EndWidth = 0.15;
                lv2.Position = new Vector2(0.5, 0.5);
                
                var line = new LwPolyline(new List<LwPolylineVertex>(){ lv1, lv2 }, false);
                
                line.Normal = new Vector3(0, 0, 1);
                line.Color = DxfConfig.Color;
                block.Entities.Add(line);
                block.Origin = new Vector3(0, 0, 0);
                return block;
            }
        }
    }
}