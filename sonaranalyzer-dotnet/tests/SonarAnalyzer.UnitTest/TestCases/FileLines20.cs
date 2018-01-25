// Noncompliant {{This file has 20 lines, which is greater than 10 authorized. Split it into smaller files.}}
namespace Tests.Diagnostics
{
    public class FooBar
    {
        public FooBar()
        {
            System
                   .Collections
                   .
                   // WTF
                   Generic

                   .List<int>
                   .
                   // Who writes this kind of code!
                   Enumerator

                   .Equals(null);

            var str = @"
                a
                b";
        }
    }
}

// hello
