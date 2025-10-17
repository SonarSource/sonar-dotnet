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
        return this.Prop1.Equals(other.Prop1, StringComparison.OrdinalIgnoreCase)   // Noncompliant, FP we should not raise in Equals method
               && this.Prop2.Equals(other.Prop2, StringComparison.OrdinalIgnoreCase)
               && this.Prop3.Equals(other.Prop3, StringComparison.OrdinalIgnoreCase)
               && this.Prop4.Equals(other.Prop4, StringComparison.OrdinalIgnoreCase)
               && this.Prop5.Equals(other.Prop5, StringComparison.OrdinalIgnoreCase);
    }
}
