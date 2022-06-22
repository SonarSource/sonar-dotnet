using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    interface IPoint
    {
        int X { get; set; }
        int Y { get; set; }
        string Tag { get; set; }
    }

    partial class PointManager<T> where T : class, IPoint
    {
        readonly T point;  // this could be a struct
        public PointManager(T point)
        {
            this.point = point;
            this.point.X = 1;       // Compliant, we are in the constructor
            this.point.Tag ??= "";  // Compliant, we are in the constructor
        }

        public void MovePointVertically(int newX)
        {
            point.X = newX; //Fixed
            point.X++; //Fixed
            ++point.X; //Fixed
            Console.WriteLine(point.X);
            var i = point.X = newX; //Fixed
            i = point.X++;          //Fixed
            point.Tag ??= "value";  //Fixed
        }
    }

    partial class PointManager<T> where T : class, IPoint
    {

    }

    partial class PointManager<T>
    {

    }

    class PointManager2<T> where T : class, IPoint
    {
        readonly T point;  // this can only be a class
        public PointManager2(T point)
        {
            this.point = point;
        }

        public void MovePointVertically(int newX)
        {
            point.X = newX;  // this assignment is guaranteed to work
            Console.WriteLine(point.X);
        }
    }

    class PointManager3<T> where T : struct, IPoint
    {
        readonly T point;  // this could be a struct
        public PointManager3(T point)
        {
            this.point = point;
            this.point.X = 1; // Compliant, we are in the constructor
        }

        public void MovePointVertically(int newX)
        {
            point.X = newX; // Compliant // Error [CS1648]
            point.X++;      // Compliant // Error [CS1648]
            Console.WriteLine(point.X);
        }
    }

    class P2<A, B> where A : class, IPoint where B : A
    {
        readonly A pointA;
        readonly B pointB;

        public P2(A a, B b)
        {
            this.pointA = a;
            this.pointB = b;
        }

        public void Add(int i)
        {
            pointA.X += i;
            pointB.X += i;   // Compliant
        }
    }

    class SelfReferencing2<T> where T : SelfReferencing2<T>, IPoint
    {
        readonly T pointA;

        public void Add(int i)
        {
            pointA.X += i; // Compliant
        }
    }

    class PublicField<T> where T : IPoint
    {
        public readonly T point;
    }

    class PublicFieldAccessor<T> where T : class, IPoint
    {
        public PublicFieldAccessor()
        {
            var a = new PublicField<T>();
            a.point.X = 1; // Fixed
        }
    }
}
