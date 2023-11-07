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
            if (a)              // Noncompliant
            {
                DoSomething();  // Secondary
            }
        }
    }

    public class CSharp8
    {
        int SwitchExpression()
        {
            var a = false;
            return a switch
            {
                true => 0,
            //  ^^^^           Noncompliant
            //          ^      Secondary@-1
                false => 1  // Noncompliant: false branch is always true
            };
        }

        int SwitchExpression_Discard()
        {
            var a = false;
            return a switch
            {
                true => 0,  // Noncompliant: true branch is always false
                            // Secondary@-1
                _ => 1
            };
        }

        int SwitchExpression_Results()
        {
            bool a;

            a = false switch { _ => false };

            if (a)                                  // Noncompliant
            {
                return 42;                          // Secondary
            }

            a = false switch
            {
                false => false,                     // Noncompliant
                _ => false                          // Secondary
            };

            if (a)                                  // Noncompliant
            {
                return 42;                          // Secondary
            }

            a = false switch
            {
                true => true,                       // Noncompliant
                                                    // Secondary@-1
                _ => false
            };

            if (a)                                  // Noncompliant
            {
                return 42;                          // Secondary
            }

            a = 0 switch
            {
                1 => true,                          // Noncompliant
                                                    // Secondary@-1
                _ => false                          // Compliant, we don't raise in default statement
            };

            if (a)                                  // Noncompliant
            {
                return 42;                          // Secondary
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
                return 1;                                   // Secondary
            }
            return 0;
        }

        int UsingDeclaration_New()
        {
            using var ms = new System.IO.MemoryStream();
            if (ms == null)                                 // Noncompliant
            {
                return 1;                                   // Secondary
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
                if (a)                                      // Noncompliant
                {
                    return 0;                               // Secondary
                }
                return 1;
            }

            if (ReturnFalse())                              // FN - content and result value of local static function is not inspected
            {
                return 42;                                  // never executed
            }

            return UseValueInside();
        }

        void NullCoalesceAssignment(string a, string b, string c, Options options)
        {
            a ??= "(empty)";
            if (a == null)                                                  // Noncompliant
            {
                throw new ArgumentNullException(nameof(a));                 // Secondary
            }

            b ??= null;                                                     // FN: NOP
            if (b == null)                                                  // OK
            {
                throw new ArgumentNullException(nameof(b));                 // OK
            }

            if ((c ??= "(empty)") == null)                                  // Noncompliant
            {
                throw new ArgumentNullException(nameof(c));                 // Secondary
            }

            if ((options.First ??= "(empty)") == null)                      // Noncompliant
            {
                throw new ArgumentNullException(nameof(c));                 // Secondary
            }

            if ((options.First ??= options.Second ??= "(empty)") == null)   // Noncompliant
            {
                throw new ArgumentNullException(nameof(c));                 // Secondary
            }

            if ((options.field ??= "(empty)") == null)                      // Noncompliant
            {
                throw new ArgumentNullException(nameof(c));                 // Secondary
            }

            var list = new List<string>();
            if ((list[0] ??= "(empty)") == null)                            // Noncompliant
            {
                throw new ArgumentNullException(nameof(c));                 // Secondary
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
            ret ??= a;
        //  ^^^                                            Noncompliant
        //          ^                                      Secondary@-1

            ret = notNull;
            ret = "Lorem " + (ret ??= a) + " ipsum";    // Noncompliant
                                                        // Secondary@-1

            ret = notNull;
            ret ??= "N/A";                              // Noncompliant
                                                        // Secondary@-1

            ret = notEmpty;
            ret ??= "N/A";                              // Noncompliant
                                                        // Secondary@-1

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
            notNull ??= isNull;                         // Noncompliant
                                                        // Secondary@-1
            isNull ??= null;                            // Noncompliant
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
                Console.WriteLine("This is also unreachable"); // Secondary
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

            if (sb is null)     // Noncompliant FP
            {
                sb = new StringBuilder();
            }

            sb.Append(i);
        }

        if (sb is null) // FN
        {
            Console.WriteLine("NULL");
        }

        Console.WriteLine(sb);
    }
}

public class TestShouldExecute
{
    void CoalesceExpression()
    {
        string a = null;
        string b = a ?? string.Empty; // Noncompliant
    }

    void ConditionalAccessExpression()
    {
        string a = null;
        bool? b = a?.StartsWith("");    // Noncompliant
                                        // Secondary@-1
    }

    void ConditionalExpression()
    {
        bool a = true;
        string b = a ? string.Empty : "Hello";  // Noncompliant
                                                // Secondary@-1
    }

    void DoStatement()
    {
        bool a = true;
        do
        {

        } while (a); // Noncompliant
    }

    void ForStatement()
    {
        bool a = true;
        for (int i = 0; a; i++) { } // Noncompliant
    }

    void WhileStatement()
    {
        bool a = true;
        while (a) { } // Noncompliant
    }

    void IfStatement()
    {
        bool a = true;
        if (a) { } // Noncompliant
    }

    void LogicalAndExpression()
    {
        bool a = false;
        var b = a && true;  // Noncompliant
                            // Secondary@-1
    }

    void LogicalOrExpression()
    {
        bool a = true;
        var b = a || true;  // Noncompliant
                            // Secondary@-1
    }

    void SwitchStatement()
    {
        int i = 10;
        switch (i)
        {
            case 1:         // Noncompliant
            case 2:         // Noncompliant
                break;
        }
    }

    int SwitchExpression()
    {
        var a = true;
        return a switch
        {
            true => 0,  // Noncompliant
            _ => 1      // Secondary
        };
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8094
class Repro8094
{
    public void TestMethod()
    {
        Action? someDelegate = delegate { };
        someDelegate += Callback;
        someDelegate -= Callback;
        if (someDelegate == null) // Noncompliant {{Change this condition so that it does not always evaluate to 'False'.}}
        { }
    }

    private void Callback() { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8149
class Repro_8149
{
    enum UserType
    {
        Internal,
        External,
        Other
    }

    interface IUser
    {
        int Id { get; }
        string LoginName { get; }
        UserType UserType { get; }
    }

    void Check_SwitchExpression(IUser user)
    {
        if (user.UserType switch                    // Noncompliant FP: always false
        {
            UserType.Internal => user.Id == 1,
            UserType.External => user.LoginName == "test",
            _ => false,
        })
        {
            return;
        }

        throw new ApplicationException("not authorized");
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8326
class Repro_8326
{
    void Test(int i, object o)
    {
        _ = i switch
        {
            1 => 1,
            var other => 2          // Noncompliant {{Change this condition so that it does not always evaluate to 'True'.}} FP: var should not raise
        };

        _ = o switch
        {
            1 => 1,
            var other => 2          // Compliant: by accident, o has no SymbolicValue and always branches du to #8324
        };

        if (i is var x1)            // Noncompliant {{Change this condition so that it does not always evaluate to 'True'. Some code paths are unreachable.}}
            Console.WriteLine();
        else
            Console.WriteLine();    // Secondary
    }
}
