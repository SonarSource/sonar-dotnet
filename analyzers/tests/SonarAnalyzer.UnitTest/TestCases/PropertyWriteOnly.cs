namespace Tests.Diagnostics
{
    public class PropertyWriteOnly
    {
        public int Foo  // Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
//                 ^^^
        {
            set
            {
                // ... some code ...
            }
        }
        public int Foo2
        {
            get
            {
                return 1;
            }
            set
            {
                // ... some code ...
            }
        }
    }

    public struct PropertyWriteOnlyStruct
    {
        public int Foo  // Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
//                 ^^^
        {
            set
            {
                // ... some code ...
            }
        }
        public int Foo2
        {
            get
            {
                return 1;
            }
            set
            {
                // ... some code ...
            }
        }
    }
}

namespace ReproIssue2390
{
    public class A
    {
        protected int m = 5;
        public virtual int M
        {
            get { return m; }
            set { m = value; }
        }
    }
    public class B : A
    {
        public override int M // compliant, getter is in base class
        {
            set { m = value + 1; }
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            B b = new B();
            System.Console.WriteLine(b.M);
        }
    }
}
