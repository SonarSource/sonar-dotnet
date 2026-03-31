using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ExpressionComplexity
    {
        public bool M()
        {
            var a1 = false;
            var b1 = false ? (true ? (false ? (true ? 1 : 0) : 0) : 0) : 1; // Noncompliant
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var c1 = (!(true || false || true || false || false)); // Noncompliant {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}
            //       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var d1 = true && false && true && false && true && true; // Noncompliant
            var d2 = true && !(false && true && false) && !true && true; // Noncompliant

            bool? v1 = null, v2 = null, v3 = null, v4 = null, v5 = null;

            var h = v1 ??= v2 ??= v3 ??= v4 ??= v5; // Noncompliant

            var m = true && true && true && call(true && true && true && true && true, true, true) && true;
            //                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                          [iss1] {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}
            //      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 [iss2] {{Reduce the number of conditional operators (4) used in the expression (maximum allowed 3).}}

            call(
                a =>
                a = ((a1 ? false : true) || a1 || true && false && true || false)); // Noncompliant

            var n = (true && true && true) == (true && true && true); // Noncompliant
            n = true && true && true && (((true ? new object() : new object()) as bool?) ?? true); // Compliant. The ? : expression is inside the binary as expression. The as expression starts a new root.

            for (var i = a1 ? (b1==0 ? (c1 ? (d1 ? 1 : 1) : 1) : 1) : 1; i < 1; i++) {} // Noncompliant

            bool[] foo = {
                true && true && true && true && true, // Noncompliant
                true && true && true && true
            };

            var foo2 = new List<bool>
            {
                true && true && true && true && true, // Noncompliant
                true && true && true && true
            };

            var x = new Dictionary<string, bool>
            {
                { "a", true && true && true && true && true }, // Noncompliant
                { "b", true && true && true && true }
            };

            var e2 = true | false | true | false;

            var a2 = false ? (true ? (false ? 1 : 0) : 0) : 1;
            var a3 = (true && true && true && true) ? true : true;                 // Noncompliant
            var a4 = true ? (true && true && true && true) : true;                 // Noncompliant
            var a5 = true ? true : (true && true && true && true);                 // Noncompliant
            var a6 = true ? true ? true ? true ? true : true : true : true : true; // Noncompliant
            var a7 = true ? true ? true ? true : true : true : true && true;       // Noncompliant

            var foo3 = new Action(delegate () {
                bool a = true && true;
                bool b = true && true;
                bool c = true && true;
                bool d = true && true;
                bool e = true && true;
                bool f = true && true && true && true && true; // Noncompliant
            });

            var f2 = new Foo2 {
                a = true && true,
                b = true && true,
                c = true && true,
                d = true && true,
            };

            var cnt = 42;
            switch (cnt)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    break;
            }

            return call(
                true && true && true,
                true && true && true,
                true && // Noncompliant
                true &&
                true &&
                true &&
                true &&
                true);
        }

        private bool call(bool v1, bool v2, bool v3)
        {
            throw new NotImplementedException();
        }

        private bool call(Func<object, object> p0)
        {
            throw new NotImplementedException();
        }
    }

    public class Foo2
    {
        public bool a { get; set; }
        public bool b { get; set; }
        public bool d { get; set; }
        public bool c { get; set; }
    }
}

// https://sonarsource.atlassian.net/browse/USER-1097
public class Repro_NET2428
{
    public string Prop1 { get; }
    public string Prop2 { get; }
    public string Prop3 { get; }
    public string Prop4 { get; }
    public string Prop5 { get; }

    public Repro_NET2428(string prop1, string prop2, string prop3, string prop4, string prop5)
    {
        Prop1 = prop1;
        Prop2 = prop2;
        Prop3 = prop3;
        Prop4 = prop4;
        Prop5 = prop5;
    }

    public override bool Equals(object obj)
    {
        var other = obj as Repro_NET2428;
        return this.Prop1.Equals(other.Prop1, StringComparison.OrdinalIgnoreCase)   // Compliant
               && this.Prop2.Equals(other.Prop2, StringComparison.OrdinalIgnoreCase)
               && this.Prop3.Equals(other.Prop3, StringComparison.OrdinalIgnoreCase)
               && this.Prop4.Equals(other.Prop4, StringComparison.OrdinalIgnoreCase)
               && this.Prop5.Equals(other.Prop5, StringComparison.OrdinalIgnoreCase);
    }
}

public class With_IEquatable : System.IEquatable<With_IEquatable>
{
    public string A { get; }
    public string B { get; }
    public string C { get; }
    public string D { get; }

    bool System.IEquatable<With_IEquatable>.Equals(With_IEquatable other) =>
        other != null
        && A == other.A
        && B == other.B
        && C == other.C
        && D == other.D; // Compliant - explicit IEquatable<T>.Equals implementation

    public static bool Equals(With_IEquatable a, With_IEquatable b) =>
        a != null && b != null // Noncompliant
        && a.A == b.A
        && a.B == b.B
        && a.C == b.C
        && a.D == b.D;
}

public struct With_Struct : System.IEquatable<With_Struct>
{
    public int A { get; }
    public int B { get; }
    public int C { get; }
    public int D { get; }
    public int E { get; }

    public bool Equals(With_Struct other) =>
        A == other.A
        && B == other.B
        && C == other.C
        && D == other.D
        && E == other.E; // Compliant
}

public class With_MixedOperators : System.IEquatable<With_MixedOperators>
{
    public string A { get; }
    public string B { get; }
    public string C { get; }
    public string D { get; }

    public bool Equals(With_MixedOperators other) =>
        ((string.IsNullOrEmpty(A) && string.IsNullOrEmpty(other.A)) || string.Equals(A, other.A)) // Compliant - mixed && and || inside Equals
        && string.Equals(B, other.B)
        && string.Equals(C, other.C)
        && ((string.IsNullOrEmpty(D) && string.IsNullOrEmpty(other.D)) || string.Equals(D, other.D));
}

public class With_GenericEquals<T, Y> : System.IEquatable<With_GenericEquals<T, Y>>
{
    public T A { get; }
    public Y B { get; }
    public string C { get; }
    public string D { get; }

    public bool Equals(With_GenericEquals<T, Y> other) =>
        other != null
        && System.Collections.Generic.EqualityComparer<T>.Default.Equals(A, other.A) // Compliant - generic type Equals
        && System.Collections.Generic.EqualityComparer<Y>.Default.Equals(B, other.B)
        && string.Equals(C, other.C)
        && string.Equals(D, other.D);
}

// https://sonarsource.atlassian.net/browse/NET-2438 - Equals helper not formally overriding Object.Equals or implementing IEquatable<T>
public class With_PrivateEqualsHelper
{
    public string A { get; }
    public string B { get; }
    public string C { get; }
    public string D { get; }

    private bool Equals(With_PrivateEqualsHelper other) =>
        (ReferenceEquals(A, other.A) || string.Equals(A, other.A)) // Noncompliant
        && (ReferenceEquals(B, other.B) || string.Equals(B, other.B))
        && (ReferenceEquals(C, other.C) || string.Equals(C, other.C))
        && (ReferenceEquals(D, other.D) || string.Equals(D, other.D));

    public override bool Equals(object obj) =>
        obj is With_PrivateEqualsHelper other && Equals(other);
}

// Equals method where the class does not implement IEquatable<T>
public class With_EqualsWithoutInterface
{
    public string A { get; }
    public string B { get; }
    public string C { get; }
    public string D { get; }
    public string E { get; }

    public bool Equals(With_EqualsWithoutInterface other) =>
        other != null // Noncompliant
        && string.Equals(A, other.A)
        && string.Equals(B, other.B)
        && string.Equals(C, other.C)
        && string.Equals(D, other.D)
        && string.Equals(E, other.E);
}

// Equals method where IEquatable is implemented for a base type, not the concrete type
public interface IMyRepresentation { }

public class With_EqualsForBaseType : IMyRepresentation, System.IEquatable<IMyRepresentation>
{
    public string A { get; }
    public string B { get; }
    public string C { get; }
    public string D { get; }
    public string E { get; }

    public bool Equals(With_EqualsForBaseType other) =>
        other != null // Noncompliant
        && string.Equals(A, other.A)
        && string.Equals(B, other.B)
        && string.Equals(C, other.C)
        && string.Equals(D, other.D)
        && string.Equals(E, other.E);

    public bool Equals(IMyRepresentation other) =>  // Compliant - implements IEquatable<IMyRepresentation>.Equals
        other is With_EqualsForBaseType typed && Equals(typed);
}
