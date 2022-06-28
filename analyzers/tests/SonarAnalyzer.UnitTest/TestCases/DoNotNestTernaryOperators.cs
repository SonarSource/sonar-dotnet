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

            var lambda1 = true // Compliant. Nested in Lambda is valid
                ? new Func<string>(() => true ? "a" : "b")
                : new Func<string>(() => true ? "c" : "d");

            var lambda2 = true
                ? new Func<string, string>(s => true ? "a" : "b")
                : new Func<string, string>(s => true ? "c" : "d");

            var lambda3 = new Func<string, string>(s => isMale ? "Mr. " : isMarried ? "Mrs. " : "Miss "); // Noncompliant

            return false;
        }
    }
}
