using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class MultilineBlocksWithoutBrace
    {
        public static void Tests() { throw new Exception();}
        public static int SomeMethod(int a)
        {
            if (a == 1)
                a++;
//              ^^^^ Secondary
                return a; // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^^^

            if (true)
            { }
            else if (a == 2)
            {

            }
            else if (a == 1)
                a *= 3; // Secondary

                return a; // Noncompliant {{This line will not be executed conditionally; only the first line of this 3-line block will be. The rest will execute unconditionally.}}

            while (true)
                while (true)
                    a++; /*comment */ // Secondary
    /**/            return a; // Noncompliant


//            String empty = "";
            return a + 10;
        }

        public void Test()
        {
            while (true)
                Tests(); // Secondary
                Tests(); // Noncompliant {{This line will not be executed in a loop; only the first line of this 2-line block will be. The rest will execute only once.}}

            while (true)
Tests(); // Secondary
Tests(); // Noncompliant

            if (true)
                Tests();
            Tests();

            while (true)
            {
                Tests();
            }
            Tests();

            if (true)
                Tests(); // Secondary

                Tests(); // Noncompliant

            if (true)
                Tests();
            else
                Tests(); // Secondary
                Tests(); // Noncompliant

            while (true)
                Tests(); // Secondary
   /*comment*/  Tests(); // Noncompliant

            while (true)
                Tests(); // Secondary
            /*comment*/
                Tests(); // Noncompliant
        }

        public void Test2(bool b)
        {
            int i = 1;
            if (true)
                i = 2;
            else
            if (false)
            {
                i = 3;
            }
            i = 4; // Compliant

            if (b)
                i = 2;
            else
            if (b)
                i = 2;
            else
            if (b)
                i = 2;
            else
            if (b)
                i = 2;
            i = 4; // Compliant

            if (i==45)
            {
                ;
            }
            else
            foreach(var j in new[] { 1 })
            {
                    ; ; ;
            }

            var x = b;
            if (x) // Compliant
            {
                ; ;
            }

            if (i==45)
            {
                ;
            }
            else
            foreach(var j in new[] { 1 })
            ; // Secondary

            if (x)  // Noncompliant, but should report only once
            {
                ; ;
            }

            if (true)
                ;
            else
            if (false)
                ;
            ; // Compliant
        }

        void TestIfs(bool a, bool b)
        {
            if (a)
            if (b)
               Console.WriteLine();

            Console.WriteLine();

            if (a)
            while (b)
               Console.WriteLine();

            Console.WriteLine();

            while (a)
            if (b)
               Console.WriteLine();

            Console.WriteLine();
        }

        void TestWeirdAlignment(bool a)
        {
            try {
              if (a)
                Console.WriteLine(); // This statement is aligned with the '{' of the try on purpose to fix https://github.com/SonarSource/sonar-csharp/issues/264
            } finally { }
        }
    }
}
