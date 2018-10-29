using System;

namespace MyLibrary
{
    public class Foo
//               ^^^ Noncompliant {{Provide an implementation for: 'operator==', 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        private int left;
        private int right;

        public Foo(int l, int r)
        {
            this.left = l;
            this.right = r;
        }

        public static Foo operator +(Foo a, Foo b)
        {
            return new Foo(a.left + b.left, a.right + b.right);
        }

        public static Foo operator -(Foo a, Foo b)
        {
            return new Foo(a.left - b.left, a.right - b.right);
        }
    }

    public class Foo2
    {
        public static object operator +(Foo2 a, Foo2 b) => new object();
        public static object operator -(Foo2 a, Foo2 b) => new object();
        public static object operator ==(Foo2 a, Foo2 b) => new object();
        public static object operator !=(Foo2 a, Foo2 b) => new object();

        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }

    public class Foo3
//               ^^^^ Noncompliant {{Provide an implementation for: 'operator+'.}}
    {
        public static object operator -(Foo3 a, Foo3 b) => new object();
        public static object operator ==(Foo3 a, Foo3 b) => new object();
        public static object operator !=(Foo3 a, Foo3 b) => new object();

        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }


    public class Foo4
    {
        public static object operator ==(Foo4 a, Foo4 b) => new object();
        public static object operator !=(Foo4 a, Foo4 b) => new object();

        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }

    public class Foo5
//               ^^^^ Noncompliant {{Provide an implementation for: 'operator=='.}}
    {
        public static object operator !=(Foo5 a, Foo5 b) => new object(); // Error [CS0216] - requires == operator

        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }

    public class Foo6
//               ^^^^ Noncompliant {{Provide an implementation for: 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static object operator ==(Foo6 a, Foo6 b) => new object(); // Error [CS0216] - requires != operator
    }
}
