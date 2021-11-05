using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IAnotherInterface
    {
        static abstract void DoSomething(string value);
        static abstract void DoSomethingElse(string value);
    }

    public class AnotherClass : IAnotherInterface
    {
        public static void DoSomething(string renamedParam) // Noncompliant
        {
        }

        public static void DoSomethingElse(string value)
        {
        }
    }

}
