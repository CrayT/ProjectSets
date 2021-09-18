using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using YW.Utils.SystemExtension;

namespace YW.SDK.FloorPlan.DxfPainter.Extensions
{
    public static class Vector2Ext
    {
        public static netDxf.Vector2 ToDxfVector2(this Vector2 vector2)
        {
            return new netDxf.Vector2(vector2.X, vector2.Y);
        }

        /// <summary>
        /// 将单位转化为毫米
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static netDxf.Vector2 ToDxfVector2MM(this Vector2 vector2)
        {
            return new netDxf.Vector2(vector2.X, vector2.Y) * 10;
        }

        /// <summary>
        /// 将单位转化为毫米
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static netDxf.Vector3 ToDxfVector3MM(this Vector2 vector2)
        {
            return new netDxf.Vector3(vector2.X, vector2.Y, 0) * 10;
        }

        public static LwPolyline ToPolyline(this Vector2[] points, bool isClosed = true)
        {
            var line = new LwPolyline();
            line.IsClosed = isClosed;

            for (int i = 0; i < points.Length; i++)
            {
                line.Vertexes.Add(new LwPolylineVertex(points[i].ToDxfVector2MM()));
                line.Vertexes.Add(new LwPolylineVertex(points.Get(i + 1).ToDxfVector2MM()));
            }

            return line;
        }
    }
}
