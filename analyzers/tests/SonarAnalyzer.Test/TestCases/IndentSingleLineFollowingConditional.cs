using System;

namespace Tests.Diagnostics
{
    class RspecExample
    {
        private void DoTheThing() { /* no-op */ }
        private void DoTheOtherThing() { /* no-op */ }
        private void SomethingElseEntirely() { /* no-op */ }
        private void Foo() { /* no-op */ }

        public void RSpec(bool condition)
        {
            if (condition)  // Noncompliant
//          ^^^^^^^^^^^^^^
            DoTheThing();
//          ^^^^^^^^^^^^^ Secondary

            DoTheOtherThing();
            SomethingElseEntirely();

            Foo();

            // Compliant slution
            if (condition)
                DoTheThing();

            DoTheOtherThing();
            SomethingElseEntirely();

            Foo();
        }

    }

    class Non_CompliantProgram
    {
        public void Non_compliant_OneOfEach_Messages()
        {
            int total = 0;
            string data = "abc";

            do                          // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'do'}}
//          ^^
            total = total + 1;          // trivia not included in secondary location for single line statements...
//          ^^^^^^^^^^^^^^^^^^ Secondary
            while (total < 10);

            while (total < 20)          // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'while'}}
//          ^^^^^^^^^^^^^^^^^^
           total = total + 1;           // trivia not included in secondary location for single line statements...
//         ^^^^^^^^^^^^^^^^^^ Secondary

            for(int i = 0; i < 10; i++) // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'for'}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^
       total = total + i;               // trivia not included in secondary location for single line statements...
//     ^^^^^^^^^^^^^^^^^^ Secondary

            foreach(char item in data)  // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'foreach'}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^
            total++;                    // trivia not included in secondary location for single line statements...
//          ^^^^^^^^ Secondary

            if (total < 100)            // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'if'}}
//          ^^^^^^^^^^^^^^^^
            total = 100;                // trivia not included in secondary location for single line statements...
//          ^^^^^^^^^^^^ Secondary
            else                        // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'else'}}
//          ^^^^
            total = 200;
//          ^^^^^^^^^^^^ Secondary
        }

        public int SpecialElseIfCase(int i)
        {
            if (i > 100)
                return 1;
            else if (i > 200)
                return 2;  // compliant - common pattern used in Akka, and used Ember and Nancy too.

            if (i > 300)
                return 3;
            else if (i > 400) // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'else if'}}
//          ^^^^^^^^^^^^^^^^^
            return 4;
//          ^^^^^^^^^ Secondary

            if (i > 400)
                return 4;
            else if (i > 500) // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'else if'}}
//          ^^^^^^^^^^^^^^^^^
      return 5;
//    ^^^^^^^^^ Secondary

            return 6;
        }

        public int OtherElseIfCombinations(int i)
        {
            if (i > 100)
                return 1;
            else  // Noncompliant
//          ^^^^
            if (i > 200)
//          ^^^^^^^^^^^^ Secondary
//          ^^^^^^^^^^^^ @-1
            return 2;
//          ^^^^^^^^^ Secondary

            return 0;
        }

        public int Non_compliant_NestedMixedConditionals()
        {
            int total = 0;
            string data = "abc";

            do  // Noncompliant
//          ^^
            while (total < 10)                  // comments are included in highlighting for compound statements
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
//          ^^^^^^^^^^^^^^^^^^ @-1
            for (int i = 0; i < 10; i++)        // comments are included in highlighting for compound statements
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1
            foreach (var item in data)          // comments are included in highlighting for compound statements
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1
            if (total < 1000)                   // comments are included in highlighting for compound statements
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
//          ^^^^^^^^^^^^^^^^^ @-1
            do                                  // comments are included in highlighting for compound statements
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
//          ^^ @-1
            total++;
//          ^^^^^^^^ Secondary
            while (total < 10);
            while (total < 100);

            return total;
        }
    }

    class CompliantProgram
    {
        public int Compliant_MixedConditionals()
        {
            int total = 0;
            string data = "abc";

            do
                while (total < 10)
                    for (int i = 0; i < 10; i++)
                        foreach (var item in data)
                            if (total < 1000)
                                do
                                    total++;
                                while (total < 10);
            while (total < 100);

            return total;
        }

        public int Compliant_MixedConditionals_WithBraces()
        {
            int total = 0;
            string data = "abc";

            // Indentation is irrelevant if there are braces
            do
            {
            while (total < 10)
            {
            for (int i = 0; i < 10; i++)
            {
            foreach (var item in data)
            {
            if (total < 1000)
            {
            do
            {
            total++;
            } while (total < 10);
            }
            else
            {
            if (total < 2000)
            {
            total++;
            }
            else
            {
            total--;
            }
            }
            }
            }
            }
            } while (total < 100);
            return total;
        }

        public int Compliant_NestedIf(int value)
        {
            int result = 0;
            if (value > 1)
                    if (value > 2)
                            if (value > 3)
                                result = 1;

            return result;
        }

        public int Compliant_NestedIfElse(int value)
        {
            int result = 0;
            if (value > 1)
                if (value > 2)
                    if (value > 3)
                        result = 3;
                    else
                        result = -3;
                else
                    result = -2;
            else
                result = -1;

            return result;
        }

        public int Compliant_NestedFor()
        {
            int total = 0;
            for (int i = 0; i < 1; i++)
             for (int j = 0; j < 1; j++)
              for (int k = 0; k < 1; k++)
                        /* no-op */
               total = i + j + k;

            return total;
        }

        public int Compliant_NestedForEach()
        {
            int total = 0;
            string data = "abcde";
            foreach (var a in data)
                foreach (var b in data)
                    foreach (var c in data)
                        total++;
            return total;
        }

        public int Compliant_NestedDo()
        {
            int total = 0;

            do
                do
                    do
                        total++;
                    while (total < 10);
                while (total < 20);
            while (total < 30);

            return total;
        }

        public int Compliant_NestedWhile()
        {
            int total = 0;

            while (total < 10)
                while (total < 20)
                    while (total < 30)
                        total++;

            return total;
        }

        public int Compliant_StatementOnSameLine()
        {
            int total = 0;
            string data = "abc";
            int notPartOfTheStatement = 0;

            do total = total + 1; while (total < 10);
         notPartOfTheStatement++; // not part of the control statement

            while (total < 20) total = total + 1;
       notPartOfTheStatement++; // not part of the control statement

            for (int i = 0; i < 10; i++) total = total + i;
    notPartOfTheStatement++; // not part of the control statement

            foreach (char item in data) total++;
notPartOfTheStatement++; // not part of the control statement

            if (total < 100) total = 100; notPartOfTheStatement++; // not part of the control statement

            return total + notPartOfTheStatement;
        }
    }
}
