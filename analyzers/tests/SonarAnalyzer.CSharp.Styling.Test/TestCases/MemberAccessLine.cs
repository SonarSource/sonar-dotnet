using System.Linq;

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

    public void Prerequisites() // These nodes start a multi-line chain
    {
        builder
            .Build().Build();          // Noncompliant
        builder
            .Variable.Build();          // Noncompliant
        builder
            .Build()[42].Build();       // Noncompliant
        builder
            .Variable[42].Build();      // Noncompliant
    }

    public void FluentAssertions()
    {
        builder.Should().BeSomething()
            .And.BeSomething().And.BeSomething();
        builder.Should().BeSomething()
            .And.BeSomething()
            .And            // Allowed
            .BeSomething();
        builder.Build("Something long")
            .Should().BeSomething();

        builder
            .And.Variable.Should().BeSomething();
        builder
            .And.Variable.Build().Should().BeSomething();
        builder
            .Subject.Variable.Should().BeSomething();
        builder
            .Which.Variable.Should().BeSomething();
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

    public void ContinuesLines()
    {
        var typicallyForImmutableField = new int[]
            {
                0,
                1,
                2
            }.ToList();

        _ = builder
            .Build("""
                This is a long argument
                """).Variable;  // Noncompliant, because there's .Build() already in the chain

        _ = builder.Build("""
            This is a long argument
            """).Variable.Build();  // Useful for TestSnippet(...).Model.Compilation
    }
}

public class Builder
{
    public Builder Variable;
    public Builder Property => null;
    public Builder[] Indexed;
    public Builder this[int index] => null;

    public Builder Build(params object[] args) => this;
    public static Builder StaticMethod() => null;
    public bool IsTrue() => true;

    // FluentAssertions-like methods
    public Builder And => null;
    public Builder Which => null;
    public Builder Subject => null;
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
