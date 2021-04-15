using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public bool Foo()
        {
            var b = System.Environment.NewLine?.ToString();
            return true ? FooImpl(true, false) : true;
        }

        public bool FooImpl(bool isMale, bool isMarried)
        {
            var x = isMale ? "Mr. " : isMarried ? "Mrs. " : "Miss ";  // Noncompliant {{Extract this nested ternary operation into an independent statement.}}
//                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var x2 = isMale ? "Mr. " : isMarried ? "Mrs. " : true ? "Miss " : "what? ";
//                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant@-1

            var x3 = isMale ? "Mr. " :
                 isMarried // Noncompliant
                ? "Mrs. "
                : "Miss ";

            return false;
        }
    }
}
