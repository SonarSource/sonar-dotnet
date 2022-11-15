using System;

namespace Tests.TestCases
{
    interface IPoint
    {
        int X { get; set; }
        int Y { get; set; }
    }

    class PointManager<T> where T : IPoint
    {
        readonly T point;  // this could be a struct
        public PointManager(T point)
        {
            this.point = point;
        }

        public void MovePointVertically(int newX)
        {
            point.X >>>= 4; // Noncompliant
        }
    }
}
