using System;

class DefaultLambdaParameters
{
    void SingleDefaultArgument()
    {
        var f = (int i = 42) => i;
        f();       // Compliant
        f(40);     // Compliant
        f(42);     // Noncompliant
        f(41 + 1); // Noncompliant, expression results into the default value
    }

    void MultipleDefaultArguments()
    {
        var f = (int i = 41, int j = 42) => i + j;
        f();       // Compliant
        f(42);     // Compliant
        f(42, 42); // Noncompliant
        f(41, 42); // Multiple violations
        //^^
        //    ^^@-1
    }

    void NamedArguments()
    {
        var f = (int i = 41, int j = 42, int z = 43) => i;
        f(i: 42); // Error [CS1746] The delegate '<anonymous delegate>' does not have a parameter named 'i'
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8096
namespace Repro_8096
{
    class PrimaryConstructors
    {
        void SingleDefaultArgument()
        {
            _ = new C1();       // Compliant
            _ = new C1(41);     // Compliant
            _ = new C1(42);     // Noncompliant
            _ = new C1(41 + 1); // Noncompliant, expression results into the default value
        }

        void MultipleDefaultArguments()
        {
            _ = new C1();       // Compliant
            _ = new C2(42);     // Compliant
            _ = new C2(42, 42); // Noncompliant
            _ = new C2(41, 42); // multiple violations
            //         ^^
            //             ^^@-1
        }

        void NamedArguments()
        {
            _ = new C2(j: 42);  // Noncompliant
        }

        class C1(int i = 42);
        class C2(int i = 41, int j = 42);
        class C3() : C1(42);    // Noncompliant
    }

    class ConstructorInitializerBase
    {
        public ConstructorInitializerBase(int i = 42) { }
        public ConstructorInitializerBase(bool _) : this(42) { } // Noncompliant

        class Derived : ConstructorInitializerBase
        {
            public Derived() : base(42) { }                      // Noncompliant
        }
    }
}
