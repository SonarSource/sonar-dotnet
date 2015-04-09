using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class RightCurlyBraceStartsLine
    {
        private void doSomething() { throw new Exception();}
        private void doSomething(object[] os) { throw new Exception();}
        public RightCurlyBraceStartsLine()
        {
        }

        public void f1()
        {
            doSomething();} // Noncompliant

        public void f2()
        {
            if (true)
            {
            doSomething();} // Noncompliant

            var f = new Action(delegate {
                                       doSomething();}); // Noncompliant

            {
            doSomething();} // Noncompliant

            if (true) { doSomething(); }
        }


        public void f3()
        {
            doSomething(new[] { new Foo (),
                                new Foo ()});

            List<int> d = new List<int> { 0, 1, 2, 3, 4,
                                        5, 6, 7, 8, 9 };

            var b = new { A = 1, B = 2,
                                C = 3, D = 4};

            var c = new DummyCl { A = 1, B = 2,
                                C = 3, D = 4};
        } 
    }

    public class DummyCl
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
    }
}
