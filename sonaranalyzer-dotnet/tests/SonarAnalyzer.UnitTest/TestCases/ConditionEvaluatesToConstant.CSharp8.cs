using System;

namespace Tests.Diagnostics
{
    public interface IWithDefaultImplementation
    {
        void DoSomething();

        //Default interface methods
        void DoNothing()
        {
            var a = false;
            if (a)  // Noncompliant
            {       // Secondary
                DoSomething(); // never executed
            }
        }
    }

    public class CSharp8
    {
        int SwitchExpression()
        {
            var a = false;
            return a switch { true => 0, false => 1 };  // FN - switch arms are not inspected
        }

        int SwitchExpression_Discard()
        {
            var a = false;
            return a switch { true => 0, _ => 1 };  // FN - switch arms are not inspected
        }

        int SwitchExpression_Results()
        {
            bool a;

            a = false switch { _ => false };
            if (a)  // Noncompliant
            {       // Secondary
                return 42; // never executed
            }

            a = false switch { false => false, _ => false };
            if (a)  // Noncompliant
            {       // Secondary
                return 42; // never executed
            }

            a = false switch { true => true, _ => false };
            if (a)  // FN - switch arms are not constrained
            {
                return 42; // never executed
            }

            a = 0 switch { 1 => true, _ => false };
            if (a)  // FN - switch arms are not constrained
            {
                return 42; // never executed
            }
            return 0;
        }

        int UsingDeclaration_Null()
        {
            using System.IO.MemoryStream ms = null;
            if (ms != null) // Noncompliant
            {               // Secondary
                return 1;
            }
            return 0;
        }

        int UsingDeclaration_New()
        {
            using var ms = new System.IO.MemoryStream();
            if (ms == null) // Noncompliant
            {               // Secondary
                return 1;
            }
            return 0;
        }

        int StaticLocalFunction()
        {
            static bool ReturnFalse()
            {
                return false;
            }
            static int UseValueInside()
            {
                var a = false;
                if (a)  // FN Static local functions are not inspected in CFG
                {
                    return 0; // never executed
                }
                return 1;
            }

            if (ReturnFalse())  // FN - content and resutl value of local static funcition is not inspected
            {
                return 42;  // never executed
            }

            return UseValueInside();
        }

        void NullCoalesceAssignment(string a, string b, string c)
        {
            a ??= "(empty)";
            if (a == null)  // Noncompliant
            {               // Secondary
                throw new ArgumentNullException(nameof(a)); // never executed
            }

            b ??= null;
            if (b == null)  // OK
            {
                throw new ArgumentNullException(nameof(b)); // OK
            }

            if ((c ??= "(empty)") == null)  // Noncompliant
            {                               // Secondary
                throw new ArgumentNullException(nameof(c)); // never executed
            }
        }

    }
}
