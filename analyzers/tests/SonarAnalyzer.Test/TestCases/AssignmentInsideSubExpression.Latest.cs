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
            await (a ??= _fooProvider.GetFooAsync()); // Noncompliant FP

        public class FooProvider
        {
            public Task<int?> GetFooAsync() => Task.FromResult<int?>(42);
        }
        private Task<int?> a;
        private readonly FooProvider _fooProvider = new FooProvider();
    }
}
