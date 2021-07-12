using System;

namespace Net5
{
    public class S3247
    {
        class Fruit { public int Prop; }
        class Vegetable { }

        public void Foo(Object x, Object y)
        {
            if ((x, y) is (Fruit f, Vegetable v))
            {
                var vegetable = (Vegetable)v;
                var fruit = (Fruit)f;
                var vegetableY = (Vegetable)y;
                var fruitX = (Fruit)x;
            }
        }
    }
}
