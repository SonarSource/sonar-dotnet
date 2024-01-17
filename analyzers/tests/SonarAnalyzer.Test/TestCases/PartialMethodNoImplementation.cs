using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public abstract partial class PartialMethodNoImplementation
    {
        partial void Method(); //Noncompliant {{Supply an implementation for this partial method.}}
//      ^^^^^^^

        void OtherM()
        {
            Method(); //Noncompliant {{Supply an implementation for the partial method, otherwise this call will be ignored.}}
            OkMethod();
            OkMethod2();
            M();
            ArrowMethod("");
        }

        partial void OkMethod();
        partial void OkMethod()
        {
            throw new NotImplementedException();
        }

        partial void OkMethod2()
        {
            throw new NotImplementedException();
        }
        partial void OkMethod2();

        public abstract void M();

        partial void ArrowMethod(string s);
        partial void ArrowMethod(string s) => Console.WriteLine(s);
    }
}
