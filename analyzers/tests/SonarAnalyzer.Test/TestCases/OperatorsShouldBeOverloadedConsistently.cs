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

        public static Foo operator *(Foo a, Foo b)
        {
            return new Foo(a.left * b.left, a.right * b.right);
        }

        public static Foo operator /(Foo a, Foo b)
        {
            return new Foo(a.left / b.left, a.right / b.right);
        }

        public static Foo operator %(Foo a, Foo b)
        {
            return new Foo(a.left % b.left, a.right % b.right);
        }
    }

    public class Foo2
    {
        public static object operator +(Foo2 a, Foo2 b) => new object();
        public static object operator -(Foo2 a, Foo2 b) => new object();
        public static object operator *(Foo2 a, Foo2 b) => new object();
        public static object operator /(Foo2 a, Foo2 b) => new object();
        public static object operator %(Foo2 a, Foo2 b) => new object();
        public static object operator ==(Foo2 a, Foo2 b) => new object();
        public static object operator !=(Foo2 a, Foo2 b) => new object();

        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }

    public class Foo3
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

    public class Foo5 // Compliant - Covered by CS0216
    {
        public static object operator !=(Foo5 a, Foo5 b) => new object(); // Error [CS0216] - requires == operator

        public override bool Equals(object obj) => false;
        public override int GetHashCode() => 0;
    }

    public class Foo6
//               ^^^^ Noncompliant {{Provide an implementation for: 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static object operator ==(Foo6 a, Foo6 b) => new object(); // Error [CS0216] - requires != operator
    }

    public class Foo8 // Noncompliant {{Provide an implementation for: 'operator==', 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static Foo8 operator +(Foo8 a, Foo8 b) => new Foo8();
    }

    public class Foo9 // Noncompliant {{Provide an implementation for: 'operator==', 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static Foo9 operator -(Foo9 a, Foo9 b) => new Foo9();
    }

    public class Foo10 // Noncompliant {{Provide an implementation for: 'operator==', 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static Foo10 operator *(Foo10 a, Foo10 b) => new Foo10();
    }

    public class Foo11 // Noncompliant {{Provide an implementation for: 'operator==', 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static Foo11 operator /(Foo11 a, Foo11 b) => new Foo11();
    }

    public class Foo12 // Noncompliant {{Provide an implementation for: 'operator==', 'operator!=', 'Object.Equals' and 'Object.GetHashCode'.}}
    {
        public static Foo12 operator %(Foo12 a, Foo12 b) => new Foo12();
    }

    public class Foo13 // Compliant as the unary operators are overriden
    {
        public static Foo13 operator +(Foo13 a) => new Foo13();
        public static Foo13 operator -(Foo13 a) => new Foo13();
    }
}
