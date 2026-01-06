using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class CoalescingAssignment
    {
        public void Test()
        {
            int? val = null;
            SomeMethod(val ??= 1); // Compliant, see e.g. https://stackoverflow.com/a/64666607
            val ??= 1;

            bool? value = null;
            if (value ??= true) { } // Compliant, see. e.g. https://stackoverflow.com/a/64666607
        }
        void SomeMethod(int val) { }
    }

    public class AssignmentInsideSubExpression
    {
        void foo(int a)
        {
        }

        void Foo()
        {
            int i = 0;

            foo(i >>>= 1); // Noncompliant
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-1136
    public class Bar
    {
        private async Task<int?> GetFooNullableAsyncTriggered() =>
            await (a ??= _fooProvider.GetFooAsync()); // Compliant see e.g. https://stackoverflow.com/a/64666607

        public class FooProvider
        {
            public Task<int?> GetFooAsync() => Task.FromResult<int?>(42);
        }
        private Task<int?> a;
        private readonly FooProvider _fooProvider = new FooProvider();
    }
}

namespace CSharp14
{
    public class OverriddenCompoundAssignment
    {
        void Test()
        {
            var a = new C1 { Value = 1 };
            SomeMethod(a += 1); // Noncompliant
        }

        void SomeMethod(C1 c) { }
        class C1
        {
            public int Value;

            public void operator +=(int x)
            {
                Value += x;
            }
        }
    }

    public class NullConditionalAssignment
    {
        void Test()
        {
            var a = new C2();
            if ((bool)a?.Value = false) // Noncompliant
            {

            }

            a?.Value = true;            // Compliant https://sonarsource.atlassian.net/browse/NET-2391
            if (a is not null)
            {
                a.Value = true;        // Compliant
            }
        }
        class C2
        {
            public bool Value;
        }

        public class Nesting
        {
            public Nesting Prop { get; set; }
            public Nesting this[int index] { get { return new Nesting(); } set { } }
        }

        void CompliantTest()
        {
            var prop = new Nesting();
            prop?.Prop = new Nesting();
            prop?.Prop?.Prop = new Nesting();
            prop?.Prop.Prop = new Nesting();
            prop.Prop?.Prop = new Nesting();
            prop.Prop?.Prop?.Prop = new Nesting();
            prop?.Prop.Prop?.Prop = new Nesting();
            prop?.Prop.Prop.Prop = new Nesting();
            var indexer = new Nesting();
            indexer?[0] = new Nesting();
            indexer?[0]?[0] = new Nesting();
            indexer?[0]?[0] = new Nesting();
            indexer?[0]?[0]?[0] = new Nesting();
            indexer?[0]?[0][0] = new Nesting();
            indexer?[0][0]?[0] = new Nesting();
            indexer[0]?[0]?[0] = new Nesting();
            var mixed = new Nesting();
            mixed?[0]?.Prop = new Nesting();
            mixed?.Prop?[0] = new Nesting();
            mixed?[0]?.Prop?[0] = new Nesting();
            mixed?.Prop?[0]?.Prop = new Nesting();

            Action dontMindMeIAmHappyLittleInnocentLambda = () =>
            {
                var prop = new Nesting();
                prop?.Prop = new Nesting();  // Compliant
            };
        }

        void NonCompliantTest()
        {
            var prop = new Nesting();
            SomeMethod(prop?.Prop = new Nesting());             // Noncompliant
            SomeMethod(prop?.Prop?.Prop = new Nesting());       // Noncompliant
            SomeMethod(prop?.Prop.Prop = new Nesting());        // Noncompliant
            SomeMethod(prop.Prop?.Prop = new Nesting());        // Noncompliant
            SomeMethod(prop.Prop?.Prop?.Prop = new Nesting());  // Noncompliant
            SomeMethod(prop?.Prop.Prop?.Prop = new Nesting());  // Noncompliant
            SomeMethod(prop?.Prop?.Prop.Prop = new Nesting());  // Noncompliant
            SomeMethod(prop?.Prop.Prop.Prop = new Nesting());   // Noncompliant
            var indexer = new Nesting();
            SomeMethod(indexer?[0] = new Nesting());            // Noncompliant
            SomeMethod(indexer?[0]?[0] = new Nesting());        // Noncompliant
            SomeMethod(indexer?[0]?[0] = new Nesting());        // Noncompliant
            SomeMethod(indexer?[0]?[0]?[0] = new Nesting());    // Noncompliant
            SomeMethod(indexer?[0]?[0][0] = new Nesting());     // Noncompliant
            SomeMethod(indexer?[0][0]?[0] = new Nesting());     // Noncompliant
            SomeMethod(indexer[0]?[0]?[0] = new Nesting());     // Noncompliant
            var mixed = new Nesting();
            SomeMethod(mixed?[0]?.Prop = new Nesting());        // Noncompliant
            SomeMethod(mixed?.Prop?[0] = new Nesting());        // Noncompliant
            SomeMethod(mixed?[0]?.Prop?[0] = new Nesting());    // Noncompliant
            SomeMethod(mixed?.Prop?[0]?.Prop = new Nesting());  // Noncompliant

            if ((prop?.Prop?.Prop.Prop = new Nesting())) { }    // Noncompliant
                                                                // Error@-1 [CS0029]
            if ((indexer?[0]?[0][0] = new Nesting())) { }       // Noncompliant
                                                                // Error@-1 [CS0029]
        }
        void SomeMethod(Nesting nesting) { }

        public class ConditionalAccessExpressionOutsideSubExpression
        {
            public void Test(Sample sample)
            {
                string mappingSpan = null;
                sample?.Invoke(new Sample(mappingSpan = "7"));  // Noncompliant
            }
            public class Sample(string x)
            {
                public void Invoke(object b) { }
            }
        }
    }
}
