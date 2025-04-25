using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public abstract class ParentAbstract
    {
        protected ParentAbstract(ParentAbstract other)
        {
            DoSomething();  // Noncompliant {{Remove this call from a constructor to the overridable 'DoSomething' method.}}
//          ^^^^^^^^^^^
            this.DoSomething();  // Noncompliant
            other.DoSomething(); // Compliant

            var a = this;
            a.DoSomething(); // Not recognized

            var action = new Action(() => { DoSomething(); });
        }

        public abstract void DoSomething();
    }

    public class Parent
    {
        public Parent()
        {
            DoSomething();  // Noncompliant
            DoSomething2();

            var action = new Action(() => { DoSomething(); });
        }

        public virtual void DoSomething() // can be overridden
        {

        }
        public void DoSomething2()
        {

        }
    }

    public class Child : Parent
    {
        private string foo;

        public Child(string foo) // leads to call DoSomething() in Parent constructor which triggers a NullReferenceException as foo has not yet been initialized
        {
            this.foo = foo;
        }

        public override void DoSomething()
        {
            Console.WriteLine(this.foo.Length);
        }
    }

    public class BaseClass
    {
        public BaseClass(int a)
        {
        }

        public virtual int DoSomething() { return 0; }
        public virtual int DoSomething(int a) { return 0; }
    }

    public class InlineBaseThisCall : BaseClass
    {
        public InlineBaseThisCall() : this(DoSomething()) { } // Error [CS0120]
        public InlineBaseThisCall(int a) : base(DoSomething(a)) { } // Error [CS0120]
    }

    // https://sonarsource.atlassian.net/browse/NET-1318
    public class LocalFuncion : BaseClass
    {
        public LocalFuncion() : base(0)
        {
            Local();

            void Local()
            {
                DoSomething(); // FN
            }
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6245
namespace Tests.Diagnostics.Repro_6245
{
    public class Class1
    {
        public Class1()
        {
            DoSomething(); // Noncompliant
        }

        public virtual void DoSomething() { }
    }

    public class ClassA2 : Class1
    {
        public ClassA2()
        {
            DoSomething(); // Noncompliant
        }
    }

    public class ClassA3 : ClassA2
    {
        public string Name { get; set; }

        public ClassA3(string name)
        {
            Name = name;
            DoSomething(); // Noncompliant
        }

        public override void DoSomething()
        {
            Console.WriteLine($"{Name} is null at this point");
        }
    }

    public class ClassB2 : Class1
    {
        public ClassB2()
        {
            DoSomething(); // Compliant
        }

        public sealed override void DoSomething() { }
    }

    public class ClassB3 : ClassB2
    {
        public string Name { get; set; }

        public ClassB3(string name)
        {
            Name = name;
        }
    }

    public sealed class ClassC2 : Class1
    {
        public ClassC2()
        {
            DoSomething(); // Compliant
        }

        public override void DoSomething() { }
    }

    public sealed class ClassD2 : Class1
    {
        public ClassD2()
        {
            DoSomething(); // Compliant
        }
    }
}
