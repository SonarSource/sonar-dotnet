using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IWithDefaultImplementation
    {
        void DoSomething();

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
            return a switch { true => 0, false => 1 };  // FN - switch arms are not constrained
        }

        int SwitchExpression_Discard()
        {
            var a = false;
            return a switch { true => 0, _ => 1 };  // FN - switch arms are not constrained
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

            a = false switch { true => true, _ => false };  // FN - switch arms are not constrained
            if (a)  // FN - switch arms are not constrained
            {
                return 42; // never executed
            }

            a = 0 switch { 1 => true, _ => false }; // FN - switch arms are not constrained
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
                if (a)  // FN Local static functions are not inspected in CFG
                {
                    return 0; // never executed
                }
                return 1;
            }

            if (ReturnFalse())  // FN - content and result value of local static function is not inspected
            {
                return 42;  // never executed
            }

            return UseValueInside();
        }

        void NullCoalesceAssignment(string a, string b, string c, Options options)
        {
            a ??= "(empty)";
            if (a == null)  // Noncompliant
            {               // Secondary
                throw new ArgumentNullException(nameof(a)); // never executed
            }

            b ??= null; // Noncompliant {{Change this expression which always evaluates to 'null'.}}
            if (b == null)  // OK
            {
                throw new ArgumentNullException(nameof(b)); // OK
            }

            if ((c ??= "(empty)") == null)  // Noncompliant
            {                               // Secondary
                throw new ArgumentNullException(nameof(c)); // never executed
            }

            if ((options.First ??= "(empty)") == null)  // Noncompliant
            {                                           // Secondary
                throw new ArgumentNullException(nameof(c)); // never executed
            }

            if ((options.First ??= options.Second ??= "(empty)") == null)  // Noncompliant
            {                                                              // Secondary
                throw new ArgumentNullException(nameof(c)); // never executed
            }

            if ((options.field ??= "(empty)") == null)  // Noncompliant
            {                                           // Secondary
                throw new ArgumentNullException(nameof(c)); // never executed
            }

            var list = new List<string>();
            if ((list[0] ??= "(empty)") == null)  // Noncompliant
            {                                     // Secondary
                throw new ArgumentNullException(nameof(c)); // never executed
            }
        }

        void NullCoalesceAssignment_Useless(string a, string b, string c, string d)
        {
            string isNull = null;
            string notNull = "";
            string notEmpty = "value";
            string ret;

            ret = b ?? a;
            ret = b ?? notNull;
            ret = c ?? notEmpty;
            ret = d ?? "N/A";

            //Left operand: Values notNull, notEmpty and ret are known to be not-null
            ret = notNull;
            ret ??= a;      // Noncompliant

            ret = notNull;
            ret = "Lorem " + (ret ??= a) + " ipsum"; // Noncompliant

            ret = notNull;
            ret ??= "N/A";  // Noncompliant

            ret = notEmpty;
            ret ??= "N/A"; // Noncompliant {{Change this expression which always evaluates to 'not null'; some subsequent code is never executed.}}
//          ^^^

            //Left operand: ret is known to be null
            ret = null;
            ret ??= a;  // Noncompliant

            ret = null;
            ret = "Lorem " + (ret ??= a) + " ipsum"; // Noncompliant

            //Right operand: isNull is known to be null, therefore ?? is useless
            ret = a;
            ret ??= null;    // Noncompliant
            ret ??= isNull;  // Noncompliant {{Change this expression which always evaluates to 'null'.}}
//                  ^^^^^^

            //Combo/Fatality
            notNull ??= isNull;    //Noncompliant [LeftA, RightA]
            isNull ??= null;       //Noncompliant [LeftB, RightB]
        }
    }

    public class Options
    {
        public string First { get; set; }

        public string Second { get; set; }

        public string field;
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/2496
    public class Repro_2496
    {

        public void EmptyCollectionCheck(ReadOnlySpan<byte> span)
        {
            // Check for empty collection with == override
            if (span == null)   // Noncompliant FP S2589 Change this condition so it does not always evaluate to true
            {                   // Secondary FP
                return;
            }
        }

        public void EmptyCollectionCheck2()
        {
            // There's wrong error message for this case.
            // Condition is always evaluated as 'true', not as 'false' as message suggests.
            // Subsequent code is always executed, not 'never' as message suggests.
            ReadOnlySpan<byte> span = new byte[] { };
            // Check for empty collection with == override
            if (span == null)   // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            {                   // Secondary
                return;
            }
        }
    }
}
