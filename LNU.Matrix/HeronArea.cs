﻿using LNU.Matrix.UtilityClasses;
using System;
using TriangleNet.Geometry;

namespace LNU.Matrix
{
    public static class HeronArea
    {
        public static double getArea(Vertex x, Vertex y, Vertex z)
        {
            double lineXY = Math.Sqrt(Math.Pow((x.X - y.X), 2) + Math.Pow((x.Y - y.Y), 2));
            double lineXZ = Math.Sqrt(Math.Pow((x.X - z.X), 2) + Math.Pow((x.Y - z.Y), 2));
            double lineYZ = Math.Sqrt(Math.Pow((z.X - y.X), 2) + Math.Pow((z.Y - y.Y), 2));

            double p = (lineXY + lineXZ + lineYZ) / 2;


            return Math.Sqrt(p * (p - lineYZ) * (p - lineXZ) * (p - lineXY));
        }


        public static BasisStorage getBase(Vertex x, Vertex y, Vertex z)
        {
            Vertex middleCordTemp = new Vertex()
            {
                X = (x.X + y.X) / 2,
                Y = (x.Y + y.Y) / 2
            };

            Vertex middleCord = new Vertex()
            {
                X = (middleCordTemp.X + z.X) / 2,
                Y = (middleCordTemp.Y + z.Y) / 2
            };


            return new BasisStorage()
            {
                FiX = (getArea(middleCord, y, z) / getArea(x, y, z)),
                FiY = (getArea(x, middleCord, z) / getArea(x, y, z)),
                FiZ = (getArea(x, y, middleCord) / getArea(x, y, z)),
            };
        }
    }

}

