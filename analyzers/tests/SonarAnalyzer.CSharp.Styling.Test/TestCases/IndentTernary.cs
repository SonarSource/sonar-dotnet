using System;
using System.Linq;

public class Sample
{
    private bool condition;

    public void Method()
    {
        bool longVariableName;
        longVariableName = condition    // 0
        ? true          // Noncompliant {{Indent this ternary at line position 13.}}
        : false;        // Noncompliant {{Indent this ternary at line position 13.}}

        longVariableName = condition   // 1
         ? true         // Noncompliant
         : false;       // Noncompliant

        longVariableName = condition   // 2
          ? true        // Noncompliant
          : false;      // Noncompliant

        longVariableName = condition   // 3
           ? true       // Noncompliant
           : false;     // Noncompliant

        longVariableName = condition   // 4
            ? true
            : false;

        longVariableName = condition   // 5
             ? true     // Noncompliant
        //   ^^^^^^
             : false;   // Noncompliant
        //   ^^^^^^^

        longVariableName = condition   // One of the branches is too far
             ? true     // Noncompliant
            : false;

        longVariableName = condition   // One of the branches is too far
            ? true
             : false;   // Noncompliant

        longVariableName = condition ? true : false;
        longVariableName = condition
            ? true : false;     // Compliant, this is T0024
        longVariableName = condition ? true
            : false;            // Compliant, this is T0024
    }

    public bool ArrowNoncompliant() =>
        condition
        ? true          // Noncompliant
        : false;        // Noncompliant

    public bool ArrowCompliant() =>
        condition
            ? true
            : false;

    public bool ArrowNotInScope() =>
        condition ? true : false;

    public object ArrowInInvocationArgument() =>
        Something(condition
        ? true          // Noncompliant
            : false);

    public object ArrowInConstructorArgument() =>
        new Nullable<bool>(condition
        ? true  // Noncompliant
            : false);

    public bool ReturnNoncompliant()
    {
        return condition
        ? true  // Noncompliant, too little
                : false; // Noncompliant, too much
    }

    public bool ReturnCompliant()
    {
        return condition
            ? true
            : false;
    }

    public void Invocations()
    {
        Something(condition // This is bad already
        ? true              // Noncompliant {{Indent this ternary at line position 13.}}
            : false,
            "Some other argument");
        Something(condition // This is bad already
            ? true
            : false,
            "Some other argument");
        global::Sample.Something(condition  // This is bad already
                ? true      // Noncompliant {{Indent this ternary at line position 13.}}
                : false,    // Noncompliant
            "Some other argument");
        global::Sample.Something(condition  // This is bad already
            ? true
            : false,
            "Some other argument");
        Something(Something(Something(condition // This is bad already
                ? true      // Noncompliant {{Indent this ternary at line position 13.}}
                : false,    // Noncompliant
            "Some other argument")));
        Something(Something(Something(condition // This is bad already
            ? true
            : false,
            "Some other argument")));

        Something(
            condition
            ? true          // Noncompliant {{Indent this ternary at line position 17.}}
            : false,        // Noncompliant
            "Some other argument");
        Something(
            condition
                ? true
                : false,
            "Some other argument");

        global::Sample.Something(       // Longer invocation name does not matter
            condition
                ? true
                : false,
            "Some other argument");

        Something(Something(Something(  // This is bad already for other reasons
            condition
                ? true
                : false,
            "Some other argument")));
    }

    public void ObjectInitializer()
    {
        _ = new WithProperty
        {
            Value = condition
                ? true
            :false              // Noncompliant
        };
    }

    public void Lambdas(int[] list, int[] longer)
    {
        list.Where(x => condition
            ? true                  // Noncompliant, too close
                        : false);   // Noncompliant, too close
        list.Where(x => condition
                        ? true      // Noncompliant, too close
                        : false);   // Noncompliant, too close
        list.Where(x => condition
                                ? true  // Noncompliant, too far
                             : false);  // Noncompliant, too far
        list.Where(x => condition       // Simple lambda
                            ? true
                            : false);
        list.Where((x) => condition     // Parenthesized lambda
                            ? true
                            : false);
        longer.Where(x => condition         // Simple lambda, longer name
                            ? true          // Compliant, as long as it's after the condition and aligned to the grid of 4
                            : false);
        longer.Where((x) => condition       // Parenthesized lambda, longer name
                                ? true      // Compliant, as long as it's after the condition and aligned to the grid of 4
                                : false);
        list.Where(x =>
        {
            return condition
                ? true
                : false;
        });
    }

    public void If()
    {
        if (condition
        ? true                  // Noncompliant {{Indent this ternary at line position 17.}}
                    : false)    // Noncompliant
        {
        }
        if (condition
                ? true
                : false)
        {
        }
    }

    public void While()
    {
        while (condition
        ? true                  // Noncompliant {{Indent this ternary at line position 17.}}
                    : false)    // Noncompliant
        {
        }
        while (condition
                ? true
                : false)
        {
        }
    }

    public void For()
    {
        for (var i = 0; condition
                        ? true                  // Noncompliant {{Indent this ternary at line position 29.}}
                                : false; i++)   // Noncompliant
        {
        }
        for (int i = 0; condition
                            ? true
                            : false; i++)
        {
        }
    }

    public void ConditionalAccess(Builder builder)
    {
        builder?
            .Build(condition
                ? true
                    : false);       // Noncompliant

        builder?
            .Build(condition
                ? true
                    : false)?       // Noncompliant
            .Build(condition
                ? true
                    : false);       // Noncompliant
        builder?.Build()?.Build(condition
            ? true
                : false);           // Noncompliant
    }

    [Obsolete(true  // Not supported, used for coverage
    ? "true"
    : "false")]
    public static bool Something(bool arg, object another = null) => true;
}

public class Builder
{
    public Builder Build(params object[] args) => this;
}

public class WithProperty
{
    public bool Value { get; set; }
}
