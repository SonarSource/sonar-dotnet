using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyLibrary
{
    public interface IBase
    {
        static abstract string GetStuff(); // Noncompliant
    }

    public class SomeClass : IBase
    {
        public static string GetStuff() => "";
    }
}
