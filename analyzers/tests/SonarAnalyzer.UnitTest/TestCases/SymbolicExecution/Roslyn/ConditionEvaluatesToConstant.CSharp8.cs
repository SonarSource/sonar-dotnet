using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Diagnostics
{
    public interface IWithDefaultImplementation
    {
        void DoSomething();

        void DoNothing()
        {
            var a = false;
            if (a)  // Noncompliant
            {
                DoSomething(); // never executed
            }
        }
    }

    public class CSharp8
    {
        int SwitchExpression()
        {
            var a = false;
            return a switch { true => 0, false => 1 };          // Noncompliant:    true branch is always false
                                                                // Noncompliant@-1: false branch is always true
        }

        int SwitchExpression_Discard()
        {
            var a = false;
            return a switch { true => 0, _ => 1 };              // Noncompliant:    true branch is always false
                                                                // Noncompliant@-1: default branch is always true
        }

        int SwitchExpression_Results()
        {
            bool a;

            a = false switch { _ => false };                    // Noncompliant
            if (a)                                              // Noncompliant
            {
                return 42;
            }

            a = false switch { false => false, _ => false };    // Noncompliant
            if (a)                                              // Noncompliant
            {
                return 42;
            }

            a = false switch { true => true, _ => false };      // Noncompliant:    true branch is always false
                                                                // Noncompliant@-1: default branch is always true
            if (a)                                              // Noncompliant
            {
                return 42;
            }

            a = 0 switch { 1 => true, _ => false };             // Noncompliant
            if (a)                                              // FN
            {
                return 42;
            }
            return 0;
        }

        public void ParenthesizedVariableDesignation(object arg)
        {
            switch ((arg, true))
            {
                case var (o, b) when b:         // Noncompliant: b is always true
                    if (b) { }                  // Noncompliant: b is always true
                    break;

                case var (o, b) when o != null: // Noncompliant FP: unreachable
                    if (o == null) { }          // Noncompliant FP: unreachable
                    break;

                case var (o, b):                // Noncompliant FP: unreachable
                    if (o == null) { }          // Compliant, no constraint should be added on "o" from the ParenthesizedVariableDesignationSyntax above

                    o = null;
                    if (o == null) { }          // Noncompliant FP: unreachable
                    break;
            }

            var isTrue = true;
            if (isTrue) { } // Noncompliant
        }

        public void ParenthesizedVariableDesignation_Nested(object arg)
        {
            switch ((arg, true, (arg, ("NotNull", 42))))
            {
                case var (argA, b, (argB, (notNull, i))):
                    if (argA == null) { }       // Compliant, no constraint should be added
                    if (argB == null) { }       // Compliant, no constraint should be added
                    if (notNull == null) { }    // FN as we don't propagate the constraint
                    break;
            }

            var isTrue = true;
            if (isTrue) { } // Noncompliant
        }

        public void NestedDeconstructionAssignment()
        {
            var (a, (b, _)) = (true, (true, true));
            if (a) { }  // FN
            if (b) { }  // FN
        }

        int UsingDeclaration_Null()
        {
            using System.IO.MemoryStream ms = null;
            if (ms != null)                                 // Noncompliant
            {        
                return 1;
            }
            return 0;
        }

        int UsingDeclaration_New()
        {
            using var ms = new System.IO.MemoryStream();
            if (ms == null)                                 // Noncompliant
            {        
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
                if (a)  // Noncompliant
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
            if (a == null)                                                  // Noncompliant
            {        
                throw new ArgumentNullException(nameof(a));                 // never executed
            }

            b ??= null;                                                     // FN: NOP
            if (b == null)                                                  // OK
            {
                throw new ArgumentNullException(nameof(b));                 // OK
            }

            if ((c ??= "(empty)") == null)                                  // Noncompliant
            {                        
                throw new ArgumentNullException(nameof(c));                 // never executed
            }

            if ((options.First ??= "(empty)") == null)                      // Noncompliant
            {                                    
                throw new ArgumentNullException(nameof(c));                 // never executed
            }

            if ((options.First ??= options.Second ??= "(empty)") == null)   // Noncompliant
            {                                                       
                throw new ArgumentNullException(nameof(c));                 // never executed
            }

            if ((options.field ??= "(empty)") == null)                      // Noncompliant
            {                                    
                throw new ArgumentNullException(nameof(c));                 // never executed
            }

            var list = new List<string>();
            if ((list[0] ??= "(empty)") == null)                            // Noncompliant
            {                              
                throw new ArgumentNullException(nameof(c));                 // never executed
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
            ret ??= a;                                  // Noncompliant

            ret = notNull;
            ret = "Lorem " + (ret ??= a) + " ipsum";    // Noncompliant

            ret = notNull;
            ret ??= "N/A";                              // Noncompliant

            ret = notEmpty;
            ret ??= "N/A";                              // Noncompliant
//          ^^^

            //Left operand: ret is known to be null
            ret = null;
            ret ??= a;                                  // Noncompliant

            ret = null;
            ret = "Lorem " + (ret ??= a) + " ipsum";    // Noncompliant

            //Right operand: isNull is known to be null, therefore ?? is useless
            ret = a;
            ret ??= null;                               // FN: NOP
            ret ??= isNull;                             // FN: NOP

            //Combo/Fatality
            notNull ??= isNull;                         //Noncompliant
            isNull ??= null;                            //Noncompliant
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
            if (span == null)   // Compliant
            {
                return;
            }
        }

        public void EmptyCollectionCheck2()
        {
            ReadOnlySpan<byte> span = new byte[] { };
            // Check for empty collection with == override
            if (span == null)   // FN
            {            
                return;
            }
        }
    }

    public class Tuples
    {
        public void DoSomething(bool arg)
        {
            var trueValue = true;
            var tuple = (arg, trueValue);

            if (tuple.arg)
            {
            }

            if (tuple.trueValue)    // FN
            {
            }
        }

        public void AssignmentTarget(bool arg)
        {
            var trueValue = true;
            bool a, b;
            (a, b) = (arg, trueValue);

            if (a)
            {
            }

            if (b)  // FN
            {
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4515
    public class Repro_4515
    {
        public void DoWork()
        {
            int? i = GetNullableInt();

            if (i == null && i == 3) // Noncompliant
            {

            }

            void B()
            {
                int? i = GetNullableInt();

                if (i == null && i == 3) // Noncompliant
                {

                }
            }

            int? GetNullableInt() => 42;
        }

        public void DoWorkStatic()
        {
            static void B()
            {
                int? i = GetNullableInt();

                if (i == null && i == 3) // Noncompliant
                {

                }
            }

            static int? GetNullableInt() => 42;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7088
class Repro_7088
{
    void Method()
    {
        int? previous = null;
        StringBuilder? sb = null;

        for (var i = 0; i < 5; i++)
        {
            if (previous == null)
            {
                previous = 667;
                continue;
            }

            if (sb is null) // Noncompliant
            {
                sb = new StringBuilder();
            }

            sb.Append(i);
        }

        if (sb is null) // Noncompliant
        {
            Console.WriteLine("NULL");
        }

        Console.WriteLine(sb);
    }
}
