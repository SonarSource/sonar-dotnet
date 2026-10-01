using System;
using System.Collections.Generic;
using System.Linq;

class Service
{
    public Service(int offset)
    {
        int DefaultValue() => 42; // Noncompliant {{Make this local function static.}}
        this.offset = offset + DefaultValue();
    }
    private int offset;
    private static int staticField;

    public int Convert(string input)
    {
        string Normalize(string value) => value.Trim(); // Noncompliant {{Make this local function static.}}
//             ^^^^^^^^^
        int Parse(string value) // Noncompliant
        {
            return int.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        int AddOffset(int value) => value + offset; // Compliant: uses an instance field
        int UseParameter(int value) => value + input.Length; // Compliant: captures the enclosing parameter
        int UseLocal(int value) // Noncompliant
        {
            var result = value + 1;
            return result;
        }

        int Recurse(int value) => value == 0 ? 0 : Recurse(value - 1); // Noncompliant
        Func<int, int> Delegate() => Recurse; // Noncompliant

        int OwnReceiver(Formatter formatter, int value) => formatter.Format(value); // Noncompliant {{Make this local function static.}}
        int NewReceiver(int value) => new Formatter().Format(value); // Noncompliant {{Make this local function static.}}

        int NestedIndependent(int value) // Noncompliant
        {
            int Inner(int nested) => nested + 1; // Noncompliant
            return Inner(value);
        }

        int NestedCapture(int value) // Noncompliant: the nested function only captures a parameter of NestedCapture
        {
            int Inner() => value; // Compliant: captures the parameter of NestedCapture
            return Inner();
        }

        int NestedLambda(int value) // Noncompliant: the lambda only captures a parameter of NestedLambda
        {
            Func<int> inner = () => value;
            return inner();
        }

        int WithLambda(IEnumerable<int> values) => values.Select(x => x + 1).Sum(); // Noncompliant
        int LambdaCapture(IEnumerable<int> values) => values.Select(x => x + input.Length).Sum(); // Compliant: the lambda captures the enclosing parameter
        int LambdaInstance(IEnumerable<int> values) => values.Select(x => x + offset).Sum(); // Compliant: the lambda uses an instance field

        static int StaticLocal(int value) => value + 1;
        return Normalize(input).Length + Parse(input) + AddOffset(1) + UseParameter(1) + UseLocal(1)
            + NewReceiver(1) + NestedIndependent(1) + NestedCapture(1) + NestedLambda(1) + StaticLocal(1) + Delegate().Invoke(1);
    }

    public void ThisAndBase()
    {
        object Self() => this;                              // Compliant: uses this
        int Indexer() => this[0];                           // Compliant: uses this
        string BaseCall() => base.ToString();               // Compliant: uses base
        void Dump() => Helper.Dump(this);                   // Compliant: uses this
        int ThisMember() => this.offset;                    // Compliant: uses this
        int InstanceMethod() => GetHashCode();              // Compliant: implicit this
        void InstanceEvent() => Changed += (_, _) => { };  // Compliant: implicit this
        Func<int> MethodGroup() => GetHashCode;             // Compliant: implicit this
        int StaticLambda() => Apply(static x => x + 1);     // Noncompliant
        int StaticNested()                                  // Noncompliant
        {
            static int Inner(int x) => x;
            return Inner(1);
        }
        int NonStaticNestedWithThis()                       // Compliant: the nested function uses this
        {
            int Inner() => offset;
            return Inner();
        }
    }

    public void Receivers(string s)
    {
        int Chained(string value) => value.Trim().Length;                   // Noncompliant
        void StaticChain(int x) => Console.Out.WriteLine(x);                // Noncompliant
        string Literal() => "a".ToUpper();                                  // Noncompliant
        int? Conditional(string value) => value?.Length;                    // Noncompliant
        int? ConditionalChain(Formatter value) => value?.Other.Format(1);   // Noncompliant
        char Element(string value) => value[0];                             // Noncompliant
        int Cast(object value) => ((string)value).Length;                   // Noncompliant
        Formatter Initializer() => new Formatter { Prop = 1 };              // Noncompliant
        Formatter InitializerCapture() => new Formatter { Prop = offset };  // Compliant: uses an instance field in the initializer value
        object Anonymous() => new { Prop = 1 };                             // Noncompliant
        int[] ArrayInitializer() => new[] { offset = 1 };                   // Compliant: assigns an instance field
        Dictionary<int, int> ElementInitializer() => new Dictionary<int, int> { { 1, offset = 2 } }; // Compliant: assigns an instance field
        Data With(Data value) => value with { Prop = 1 };                   // Noncompliant
        Data WithCapture(Data value) => value with { Prop = offset };       // Compliant: uses an instance field in the initializer value
        int StaticMember() => staticField + Service.staticField;            // Noncompliant
        int CapturedReceiver() => s.Length;                                 // Compliant: captures the enclosing parameter
        int ReceiverField() => offset.GetHashCode();                        // Compliant: uses an instance field
        int FieldOfOther(Service other) => other.offset;                    // Noncompliant
    }

    public void NamedArguments(string input)
    {
        int Named(string value) => int.Parse(s: value); // Noncompliant
        int NamedCapture() => int.Parse(s: input);      // Compliant: captures the enclosing parameter
        bool Pattern(string value) => value is { Length: 1 }; // Noncompliant
        string Name() => nameof(input) + nameof(offset);      // Noncompliant: nameof does not capture
    }

    public void Generic<T>(T parameter) where T : new()
    {
        T Default() => default;              // Noncompliant: enclosing type parameters are allowed in static local functions
        T Create() => new T();               // Noncompliant
        T Capture() => parameter;            // Compliant: captures the enclosing parameter
        TInner Own<TInner>(TInner x) => x;   // Noncompliant
    }

    public void GenericLocalFunctionCall(int parameter)
    {
        T Capturing<T>() => (T)(object)parameter;   // Compliant: captures the enclosing parameter
        int Calls() => Capturing<int>();            // Compliant: calls a generic local function with an explicit type argument
    }

    public void Constants()
    {
        const int Constant = 42;
        var variable = 42;
        int UseConstant() => Constant;       // Noncompliant: constants are not captured
        int UseVariable() => variable;       // Compliant: captures the enclosing local
    }

    public void Siblings(int parameter)
    {
        int Capturing() => parameter;                   // Compliant: captures the enclosing parameter
        int Independent() => 42;                        // Noncompliant
        int CallsCapturing() => Capturing();            // Compliant: the called local function captures
        Func<int> ReferencesCapturing() => Capturing;   // Compliant: a method-group reference inherits Capturing's capture
        int CallsIndependent() => Independent();        // Noncompliant
        int CallsCallsCapturing() => CallsCapturing();  // Compliant: captures transitively
        int Ping(int x) => x == 0 ? 0 : Pong(x - 1);    // Compliant: the called local function captures
        int Pong(int x) => x == 0 ? parameter : Ping(x - 1); // Compliant: captures the enclosing parameter
        int PingIndependent(int x) => x == 0 ? 0 : PongIndependent(x - 1); // Noncompliant
        int PongIndependent(int x) => x == 0 ? 0 : PingIndependent(x - 1); // Noncompliant
    }

    public void DeepNesting(int parameter)
    {
        int Outer()                             // Compliant: a capture two levels down still disqualifies it
        {
            int Middle()                        // Compliant: the nested function captures 'parameter'
            {
                int Innermost() => parameter;   // Compliant: captures the outermost parameter
                return Innermost();
            }
            return Middle();
        }
    }

    public void Diamond(int parameter)
    {
        int Shared() => 42;                                  // Noncompliant: fully independent
        int Left() => Shared();                              // Noncompliant: reaches the independent Shared
        int Right() => Shared();                             // Noncompliant: reaches the independent Shared via a second path
        int Root() => Left() + Right();                      // Noncompliant: both paths resolve through the same independent Shared

        int CapturingShared() => parameter;                  // Compliant: captures the enclosing parameter
        int LeftCapture() => CapturingShared();              // Compliant: transitively captures via CapturingShared
        int RightCapture() => CapturingShared();             // Compliant: transitively captures via CapturingShared
        int RootCapture() => LeftCapture() + RightCapture(); // Compliant: transitively captures via both paths
    }

    public void MixedNestedSiblings(int parameter)
    {
        int Outer()                             // Compliant: CapturingMiddle keeps Outer non-independent
        {
            int CapturingMiddle() => parameter; // Compliant: captures the enclosing parameter
            int IndependentMiddle() => 42;      // Noncompliant: independent even though the enclosing Outer isn't
            return CapturingMiddle() + IndependentMiddle();
        }
    }

    public void OwnVariables(string input)
    {
        int OutVar(string value) => int.TryParse(value, out var result) ? result : 0;       // Noncompliant
        bool PatternVar(object value) => value is string text && text.Length > 0;           // Noncompliant
        int Foreach(int[] values)                                                           // Noncompliant
        {
            var sum = 0;
            foreach (var value in values)
            {
                Func<int> f = () => value + sum;    // Captures only variables of Foreach
                sum += f();
            }
            return sum;
        }
        int ForeachCapture(int[] values)                                                    // Compliant: the lambda captures the enclosing parameter
        {
            var sum = 0;
            foreach (var value in values)
            {
                Func<int> f = () => value + input.Length;
                sum += f();
            }
            return sum;
        }
    }

    public int Property
    {
        get
        {
            int Read() => 42; // Noncompliant
            return Read();
        }
        set
        {
            int Write() => value; // Compliant: captures the implicit value parameter
            offset = Write();
        }
    }

    public void InLambda()
    {
        Func<int, int> lambda = x =>
        {
            int Captures() => x;          // Compliant: captures the lambda parameter
            int Independent(int y) => y;  // Noncompliant
            return Captures() + Independent(1);
        };
    }

    public void AlreadyStatic()
    {
        static int Local(int value) => value; // Compliant: already static
    }

    public void Unresolved()
    {
        int Local() => undefined; // Noncompliant
                                  // Error@-1 [CS0103]
    }

    public int this[int index] => index;

    public event EventHandler Changed;

    private static int Apply(Func<int, int> func) => func(1);

    private sealed class Formatter
    {
        public int Prop { get; set; }
        public Formatter Other => this;
        public int Format(int value) => value;
    }

    private sealed record Data(int Prop);

    private static class Helper
    {
        public static void Dump(object value) { }
    }
}

struct Struct
{
    private static int staticField;

    public void M()
    {
        int UsesStaticField() => staticField;   // Noncompliant
    }
}
