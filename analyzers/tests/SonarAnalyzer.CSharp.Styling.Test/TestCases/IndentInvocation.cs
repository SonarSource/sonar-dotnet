using System;
using System.Linq;

public class Sample
{
    private Builder builder;

    public void Method()
    {
        builder         // 0
        .Variable       // Noncompliant {{Indent this member access at line position 13.}}
        .Property       // Noncompliant {{Indent this member access at line position 13.}}
        .Build();       // Noncompliant {{Indent this member access at line position 13.}}

        builder         // 1
         .Variable      // Noncompliant
         .Property      // Noncompliant
         .Build();      // Noncompliant

        builder         // 2
          .Variable     // Noncompliant
          .Property     // Noncompliant
          .Build();     // Noncompliant

        builder         // 3
           .Variable    // Noncompliant
           .Property    // Noncompliant
           .Build();    // Noncompliant

        builder         // 4
            .Variable
            .Property
            .Build();

        builder         // 5
             .Variable      // Noncompliant
        //   ^^^^^^^^^
             .Property      // Noncompliant
        //   ^^^^^^^^^
             .Indexed[424242]   // Noncompliant
        //   ^^^^^^^^
             .Build("Same with arguments");     // Noncompliant
        //   ^^^^^^

        builder
            .Build()
             .Build()   // Noncompliant, too far
            .Build()
           .Build()     // Noncompliant, too close
            .Build();

        builder.Build();
        builder.Build().Build().Build();
        builder.Build()
            .Build().Build();   // Compliant, this is T0027
    }

    public Builder ArrowNoncompliant() =>
        builder
        .Build();       // Noncompliant

    public Builder ArrowCompliant() =>
        builder
            .Build();

    public Builder ArrowNotInScope() =>
        builder.Build();

    public object ArrowInInvocationArgument() =>
        Something(builder
        .Build()        // Noncompliant
            .Build());

    public object ArrowInConstructorArgument() =>
        new Exception(builder
        .Build()    // Noncompliant
            .ToString());

    public Builder ReturnNoncompliant()
    {
        return builder
        .Build()            // Noncompliant, too little
                .Build();   // Noncompliant, too much
    }

    public Builder ReturnCompliant()
    {
        return builder
            .Build()
            .Build()
            .Build()
            .Build("Are we sure it's really built?");
    }

    public void Invocations()
    {
        Something(builder
        .Build()            // Noncompliant {{Indent this member access at line position 13.}}
                .Build(),   // Noncompliant, too far
            "Some other argument");
        Something(builder   // This is bad already
            .Build()
            .Build(),
            "Some other argument");
        global::Sample.Something(builder    // This is bad already
                .Build()    // Noncompliant {{Indent this member access at line position 13.}}
                .Build(),   // Noncompliant
            "Some other argument");
        global::Sample.Something(builder  // This is bad already
            .Build()
            .Build(),
            "Some other argument");
        Something(Something(Something(builder // This is bad already
                .Build()    // Noncompliant {{Indent this member access at line position 13.}}
                .Build(),   // Noncompliant
            "Some other argument")));
        Something(Something(Something(builder // This is bad already
            .Build()
            .Build(),
            "Some other argument")));

        Something(
            builder
            .Build()        // Noncompliant {{Indent this member access at line position 17.}}
            .Build(),       // Noncompliant
            "Some other argument");
        Something(
            builder
                .Build()
                .Build(),
            "Some other argument");

        global::Sample.Something(       // Longer invocation name does not matter
            builder
                .Build()
                .Build(),
            "Some other argument");

        Something(Something(Something(  // This is bad already for other reasons
            builder
                .Build()
                .Build(),
            "Some other argument")));
    }

    public void ObjectInitializer()
    {
        _ = new Builder
        {
            Variable = builder.Build()
                .Build()
            .Build()            // Noncompliant
                    .Build()    // Noncompliant
        };
    }

    public void Lambdas(int[] list, int[] longer)
    {
        list.Where(x => builder
            .Build()                    // Noncompliant, too close
                        .IsTrue());     // Noncompliant, too close
        list.Where(x => builder
                        .IsTrue());     // Noncompliant, too close
        list.Where(x => builder
                             .IsTrue());    // Noncompliant, too far
        list.Where(x => builder             // Simple lambda
                            .IsTrue());
        list.Where((x) => builder           // Parenthesized lambda
                            .IsTrue());
        longer.Where(x => builder           // Simple lambda, longer name
                            .Build()        // Compliant, as long as it's after the builder and aligned to the grid of 4
                            .IsTrue());
        longer.Where((x) => builder         // Parenthesized lambda, longer name
                                .Build()    // Compliant, as long as it's after the builder and aligned to the grid of 4
                                .IsTrue());
        list.Where(x =>
        {
            return builder
                .IsTrue();
        });
    }

    public void If()
    {
        if (builder
        .Build()                // Noncompliant {{Indent this member access at line position 17.}}
                    .IsTrue())  // Noncompliant
        {
        }
        if (builder
                .Build()
                .IsTrue())
        {
        }
    }

    public void While()
    {
        while (builder
        .Build()                // Noncompliant {{Indent this member access at line position 17.}}
                    .IsTrue())  // Noncompliant
        {
        }
        while (builder
                .Build()
                .IsTrue())
        {
        }
    }

    public void For()
    {
        for (var i = 0; builder
                        .Build()                // Noncompliant {{Indent this member access at line position 29.}}
                                .IsTrue(); i++) // Noncompliant
        {
        }
        for (int i = 0; builder
                            .Build()
                            .IsTrue(); i++)
        {
        }
    }

    public void ConditionalAccess()
    {
        builder?
                .Build()?   // Noncompliant
        //      ^^^^^^
        .Build();           // Noncompliant

        builder?
            .Build()?
            .Build();
        builder
        ?.Build()       // Another problem that is out of scope
                ?.Build();
        builder?.Build()?.Build();
    }

    public void Global()
    {
        global::Builder
                .StaticMethod();    // Noncompliant
        global::Builder
            .StaticMethod();
    }

    public void Nested()
    {
        Builder.NestedOnce
                .StaticMethod();    // Noncompliant
        Builder.NestedOnce
            .StaticMethod();
        Builder.NestedOnce.NestedTwice
                .StaticMethod();    // Noncompliant
        Builder.NestedOnce.NestedTwice
            .StaticMethod();
    }

    public static bool Something(object arg, object another = null) => true;
}

public class Builder
{
    public Builder Variable;
    public Builder Property => null;
    public Builder[] Indexed;

    public Builder Build() => this;
    public Builder Build(object arg) => this;
    public static Builder StaticMethod() => null;
    public bool IsTrue() => true;

    public class NestedOnce
    {
        public static Builder StaticMethod() => null;

        public class NestedTwice
        {
            public static Builder StaticMethod() => null;
        }
    }
}
