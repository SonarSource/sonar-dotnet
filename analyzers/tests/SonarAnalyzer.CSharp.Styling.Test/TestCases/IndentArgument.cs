using System;
using System.Linq;

public class Sample
{
    public void Method()
    {
        Invocation(      // 0
        "Argument",     // Noncompliant {{Indent this argument at line position 13.}}
        "Another");     // Noncompliant {{Indent this argument at line position 13.}}

        Invocation(      // 1
         "Argument",    // Noncompliant {{Indent this argument at line position 13.}}
         "Another");    // Noncompliant {{Indent this argument at line position 13.}}

        Invocation(      // 2
          "Argument",   // Noncompliant {{Indent this argument at line position 13.}}
          "Another");   // Noncompliant {{Indent this argument at line position 13.}}

        Invocation(      // 3
           "Argument",  // Noncompliant {{Indent this argument at line position 13.}}
           "Another");  // Noncompliant {{Indent this argument at line position 13.}}

        Invocation(      // 4
            "Argument",
            "Another");

        Invocation(          // 5
             "Argument",    // Noncompliant {{Indent this argument at line position 13.}}
        //   ^^^^^^^^^^
             "Another");    // Noncompliant {{Indent this argument at line position 13.}}
        //   ^^^^^^^^^

        Invocation("Argument");
        Invocation("Argument", "Another");
        Invocation(
            "Argument",
            "Another");
        Invocation("Argument",       // T0028
            "Another");
        Invocation(
            "Argument", "Another"); // T0028
    }

    public bool ArrowNoncompliant() =>
        Invocation(
        "Arg",              // Noncompliant
                "Another"); // Noncompliant

    public bool ArrowCompliant() =>
        Invocation(
            "Arg",
            "Another");

    public bool ArrowNotInScope() =>
        Invocation("Arg", "Another");

    public bool ReturnNoncompliant()
    {
        return Invocation(
        "Arg",              // Noncompliant
                "Another"); // Noncompliant
    }

    public bool ReturnCompliant()
    {
        return Invocation(
            "Arg",
            "Another");
    }

    public void Invocations()
    {
        Invocation("First",
        "Too close",            // Noncompliant {{Indent this argument at line position 13.}}
            "Good",
                "Too far");     // Noncompliant
        Invocation(
            "First",
            "Second");
        global::Sample.Invocation("First",
        "Too close",            // Noncompliant {{Indent this argument at line position 13.}}
            "Good",
                "Too far");     // Noncompliant
        global::Sample.Invocation(
            "First",
            "Second");
        Invocation(Invocation(Invocation("First",   // This is bad already for other reasons
        "Too close",            // Noncompliant {{Indent this argument at line position 13.}}
            "Good",
                "Too far")));   // Noncompliant
        Invocation(Invocation(Invocation(
            "First",
            "Second")));

        Invocation(
            Invocation(
            "Nested too close",         // Noncompliant {{Indent this argument at line position 17.}}
                    "Nested too far"),  // Noncompliant
            "Some other argument");
        Invocation(
            Invocation(
                "First",
                "Second"),
            "Some other argument");
        global::Sample.Invocation(       // Longer invocation name does not matter
            "First",
            "Second");

        Invocation(Invocation(Invocation(   // This is bad already for other reasons
            "First",
            "Second")));

        // Simple lambda
        RegisterNodeAction(c =>
        {                               // Noncompliant
        },
            42);
        RegisterNodeAction(c => { }, 42);
        RegisterNodeAction(c =>
            {
            },
            42);
        // Parenthesized lambda
        RegisterNodeAction((c) =>
        {                               // Noncompliant
        },
            42);
        RegisterNodeAction((c) => { }, 42);
        RegisterNodeAction((c) =>
            {
            },
            42);
        // Expression-body lambda
        AcceptFunc(c =>
        c,                              // Noncompliant
            42);
        AcceptFunc(c => c, 42);
        AcceptFunc(c =>
            c,
            42);

}

    public void Lambdas(int[] list, int[] longer)
    {
        list.Where(x => Invocation(
            "Too close",                        // Noncompliant {{Indent this argument at line position 29.}}
                "Too close",                    // Noncompliant
                    "Too close",                // Noncompliant
                        "Too close",            // Noncompliant
                            "Good",
                                "Too far"));    // Noncompliant
        list.Where(x => Invocation(         // Simple lambda
                        "Too close",        // Noncompliant {{Indent this argument at line position 29.}}
                            "Good"));
        list.Where((x) => Invocation(       // Parenthesized lambda
                        "Too close",        // Noncompliant {{Indent this argument at line position 29.}}
                            "Good"));
        longer.Where(x => Invocation(       // Simple lambda, longer name
                            "Good"));       // Compliant, as long as it's after the invocation and aligned to the grid of 4
        longer.Where((x) => Invocation(     // Parenthesized lambda, longer name
                                "Good"));   // Compliant, as long as it's after the invocation and aligned to the grid of 4

        list.Where(x =>
        {                                   // Noncompliant
            return Invocation(
                "Good");
        });
    }

    public void If()
    {
        if (Invocation(
            "Too close",        // Noncompliant {{Indent this argument at line position 17.}}
                "Good",
                    "Too far")) // Noncompliant
        {
        }
        if (Invocation(
                "Good"))
        {
        }
    }

    public void While()
    {
        while (Invocation(
            "Too close",        // Noncompliant {{Indent this argument at line position 17.}}
                "Good",
                    "Too far")) // Noncompliant
        {
        }
        while (Invocation(
                "Good"))
        {
        }
    }

    public void For()
    {
        for (var i = 0; Invocation(
                        "Too close",                // Noncompliant {{Indent this argument at line position 29.}}
                            "Good",
                                "Too far"); i++)    // Noncompliant
        {
        }
        for (int i = 0; Invocation(
                            "Good"); i++)
        {
        }
    }

    public void Global()
    {
        global::Sample.Invocation(
        "Too close",            // Noncompliant
            "Good",
                "Too far");     // Noncompliant
        global::Sample.Invocation(
            "First",
            "Second");
    }

    public static bool Invocation(params object[] args) => true;
    public static string ReturnString(string arg) => arg;
    public void RegisterNodeAction(Action<object> action, params object[] syntaxKinds) { }
    public void AcceptFunc(Func<object, object> func, params object[] args) { }

    [Obsolete(ReturnString("For coverage"))]    // Error [CS0182] An attribute argument must be a constant expression
    public void Coverage() { }
}
