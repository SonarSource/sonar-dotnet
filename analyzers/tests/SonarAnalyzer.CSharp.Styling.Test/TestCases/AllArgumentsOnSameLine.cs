using System;
using System.Linq;
using System.Collections.Generic;

[Obsolete(
    "Because I said so",
    true, DiagnosticId = "ID0001")] // Noncompliant
//        ^^^^^^^^^^^^^^^^^^^^^^^
class IAmObsolete { }

[Obsolete("Yes", true, DiagnosticId = "ID0001")]
class AlsoObolete { }

[Obsolete(
    "Yes",
    true,
    DiagnosticId = "ID0001")]
class LastOneObsolete { }

[Obsolete("Yes", // Noncompliant {{All arguments should be on the same line or on separate lines.}}
    //    ^^^^^
    true,
    DiagnosticId = "ID0001")]
class ILiedObsolete { }

class ArgumentsOnSameLine
{
    static void Method(params object[] args) { }

    static void TypedMethod<T1, T2, T3, T4>() { }

    static void RefOutInMethod(int a, in int b, ref int c, out int d) { d = 0; }
    static void OptionalParameters(int a = 0, int b = 0, int c = 0, int d = 0) { d = 0; }

    public string this[params object[] args] => null;

    void Compliant(List<string> list, int a)
    {
        Method();
        Method(1);
        Method(1, 2);
        Method(1, 2, 3);
        Method(
            1, 2, 3, 4); // Noncompliant {{All arguments should be on the same line or on separate lines.}}

        Method(
            1,
            2,
            3);

        Method(
            1,
            list
                .Where(x => x.Length > 4)
                .Select(x => x + x)
                .ToArray(),
            42);

        RefOutInMethod(0, in a, ref a, out a);
        RefOutInMethod(
            0, in a, ref a, out a); // Noncompliant
        RefOutInMethod(
            0,
            in a,
            ref a,
            out a);

        _ = this[1, 2, 3];
        _ = this[
            1,
            2,
            3];

        TypedMethod<int, int, int, int>();
        TypedMethod<
            int,
            int,
            int,
            int>();

        OptionalParameters(0, 0, 0, 0);
        OptionalParameters(d: 0, c: 0, b: 0, a:0);
        OptionalParameters(
            d: 0,
            c: 0,
            b: 0,
            a: 0);
    }

    void NonCompliant(List<string> list, int a)
    {
        Method(
            1,
            2,
            3, 42); // Noncompliant {{All arguments should be on the same line or on separate lines.}}
        //     ^^

        Method(1,   // Noncompliant
            2,
            3, 4,   // Noncompliant
            5,
            6, 7);  // Noncompliant

        Method(
            1,
            2,
            3, 4,   // Noncompliant
            5,
            6, 7);  // Noncompliant

        Method(1,   // Noncompliant {{All arguments should be on the same line or on separate lines.}}
        //     ^
            2,
            3);
        Method(1, list              // Noncompliant
                                    // Noncompliant@-1
            .Where(x => x.Length > 4), 42); // Noncompliant

        Method(1,                   // Noncompliant
            2, 3);                  // Noncompliant

        RefOutInMethod(0,           // Noncompliant
            in a, ref a,            // Noncompliant
            out a);
        RefOutInMethod(
            0,
            in a, ref a,            // Noncompliant
            out a);

        _ = this[1,                 // Noncompliant
            2, 3];                  // Noncompliant
        _ = this[
                1, 2, 3];           // Noncompliant
        _ = this[
            1,
            2, 3];                  // Noncompliant

        TypedMethod<int,            // Noncompliant
            int, int, int>();       // Noncompliant
                                    // Noncompliant@-1
        TypedMethod<
            int, int, int, int>();  // Noncompliant
        TypedMethod<
            int,
            int, int,               // Noncompliant
            int>();

        OptionalParameters(d: 0,    // Noncompliant
            c: 0,
            b: 0,
            a: 0);
        OptionalParameters(
            d: 0,
            c: 0, b: 0,             // Noncompliant
            a: 0);
    }

    void LocalFunction()
    {
        Local(1, 2, 3);
        Local(
            1,
            2,
            3);
        Local(
            1,
            2, 3); // Noncompliant

        void Local(params object[] args) { }
    }

    void Lambdas(Func<int, int, int, bool> predicate, Action<int, int, int> action)
    {
        predicate(1, 2, 3);
        predicate(
            1,
            2,
            3);
        predicate(1,    // Noncompliant
            2,
            3);
        predicate(
            1,
            2, 3);      // Noncompliant

        action(1, 2, 3);
        action(
            1,
            2,
            3);
        action(1,       // Noncompliant
            2,
            3);
        action(
            1,
            2, 3);      // Noncompliant

        var lambda = (int a, int b, int c) => { };

        lambda(1, 2, 3);
        lambda(
            1,
            2,
            3);
        lambda(1,   // Noncompliant
            2,
            3);
        lambda(
            1,
            2, 3);  // Noncompliant
    }
}

public class RuleRegistration
{
    public void Initialize()
    {
        RegisterSonarWhateverAnalysisContext(c =>
            {
                // something
            }, 0, 0);   // Noncompliant
                        // Noncompliant@-1
        RegisterSonarWhateverAnalysisContext(context =>
            {
                // something
            },
            0,
            0);
        RegisterSonarWhateverAnalysisContext(whateverContext =>
            {
                // something
            },
            0,
            0);
        RegisterSonarWhateverReportingContext(c =>
            {
                // something
            },
            0,
            0);
        RegisterSonarWhateverReportingContextActionNotFirst("first", c =>   // Noncompliant
            {
                // something
            },
            "last");
        RegisterSonarSomething(c =>             // Noncompliant
            {
                // something
            },
            0,
            0);
        RegisterSomethingAnalysisContext(c =>
            {
                // something
            },
            0,
            0);
        RegisterSomethingReportingContext(c =>  // Noncompliant
            {
                // something
            },
            0,
            0);
        RegisterSonarSomethingContext(c =>      // Noncompliant
            {
                // something
            },
            0,
            0);
        RegisterSonarWhateverReportingContext(c => { }, 0, 0);
        RegisterSonarWhateverReportingContext(
            c => { },
            0,
            0);
        RegisterSonarWhateverReportingContext(
            c =>
            {
            },
            0,
            0);
        // Noncompliant@+1
        UnrelatedContext((c =>
        {
        }, 42),
            0,
            0);
    }

    protected void RegisterSonarWhateverAnalysisContext(Action<SonarWhateverAnalysisContext> action, params object[] kind) { }
    protected void RegisterSonarWhateverReportingContext(Action<SonarWhateverReportingContext> action, params object[] kind) { }
    protected void RegisterSonarWhateverReportingContextActionNotFirst(string first, Action<SonarWhateverReportingContext> action, string last) { }
    protected void RegisterSonarWhateverReportingContext(Func<SonarWhateverReportingContext, Action<SonarWhateverReportingContext>> action, params object[] kind) { }
    protected void RegisterSonarSomething(Action<SonarSomething> action, params object[] kind) { }
    protected void RegisterSomethingAnalysisContext(Action<SomethingAnalysisContext> action, params object[] kind) { }
    protected void RegisterSomethingReportingContext(Action<SomethingReportingContext> action, params object[] kind) { }
    protected void RegisterSonarSomethingContext(Action<SonarSomethingContext> action, params object[] kind) { }
    protected void UnrelatedContext((Action<SonarWhateverReportingContext>, int) tuple, params object[] kind) { }

    // Well-known expected classes patterns
    public class SonarWhateverAnalysisContext { }
    public class SonarWhateverReportingContext { }
    public class SomethingAnalysisContext { }
    // Unexpected types
    public class SonarSomething { }
    public class SomethingReportingContext { }
    public class SonarSomethingContext { }
}
