public class Sample
{
    Builder builder;

    public void Method()
    {
        builder.Build().Build()
            .Build();
        _ = builder
            .Build().Build()    // Noncompliant {{Move this member access to the next line.}}
        //          ^^^^^^
            .Build().Variable;  // Noncompliant
        //          ^^^^^^^^^
        builder
            .Build().Build();   // Noncompliant
        //          ^^^^^^
        builder.Build().Variable.Build()
            .Build()
            .Build().Build()    // Noncompliant
            .Build().Variable   // Noncompliant
            .Build().Property   // Noncompliant
            .Build()[42]
            .Build();

        builder.Build()
            .Build().Build().Build().Build();
        //          ^^^^^^                  // Noncompliant@-1
        //                   ^^^^^^         // Noncompliant@-2
        //                          ^^^^^^  // Noncompliant@-3

        builder.Build()
            .Variable
            .Build()
            .Property
            .Build()
            .Indexed[42]
            .Build()
            .Property[42]
            .Variable[42]
            .Build()[42]
            .Build();
    }

    public void FluentAssertions()
    {
        builder.Should().BeSomething()
            .And.BeSomething().And.BeSomething();   // Noncompliant
        //                    ^^^^
        builder.Should().BeSomething()
            .And.BeSomething()
            .And            // Allowed
            .BeSomething();
    }

    public void ConditionalAccess()
    {
        builder?
            .Build()?.Build();              // FN, ConditionalAccessExpression and MemberBindingExpression are too different from MemberAccessExpression to deal with
        builder?.Build()?.Build();
    }

    public void Global()
    {
        global::Builder
                .StaticMethod().Build();    // Noncompliant
        //                     ^^^^^^
        global::Builder
            .StaticMethod()
            .Build();
    }

    public void Nested()
    {
        Builder.NestedOnce.NestedTwice
            .StaticMethod().Build();        // Noncompliant
        Builder.NestedOnce.NestedTwice
            .StaticMethod()
            .Build();
    }
}

public class Builder
{
    public Builder Variable;
    public Builder Property => null;
    public Builder[] Indexed;
    public Builder this[int index] => null;

    public Builder Build() => this;
    public static Builder StaticMethod() => null;
    public bool IsTrue() => true;

    // FluentAssertions-like methods
    public Builder And => null; // Special case to simulate FluentAssertions API
    public Builder Should() => null;
    public Builder BeSomething() => null;

    public class NestedOnce
    {
        public class NestedTwice
        {
            public static Builder StaticMethod() => null;
        }
    }
}
