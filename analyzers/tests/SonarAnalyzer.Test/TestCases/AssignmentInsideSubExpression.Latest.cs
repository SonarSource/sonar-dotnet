using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
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
        }
        class C2
        {
            public bool Value;
        }
    }
}
