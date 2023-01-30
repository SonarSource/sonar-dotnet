using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions.Execution;

namespace Tests.Diagnostics
{
    public class DisposableNotDisposedAsync
    {
        private FileStream field_fs1 = new FileStream(@"c:\foo.txt", FileMode.Open);    // Compliant - disposed in a public async method
        private FileStream field_fs2 = File.Open(@"c:\foo.txt", FileMode.Open);         // Compliant - disposed in a public async method
        private FileStream field_fs3 = new FileStream(@"c:\foo.txt", FileMode.Open);    // FN - the method which disposes it is private, and it's not referenced anywhere
        private FileStream field_fs4 = new FileStream(@"c:\foo.txt", FileMode.Open);    // Compliant - disposed in a public async ValueTask method
        private FileStream field_fs5 = new FileStream(@"c:\foo.txt", FileMode.Open);    // Compliant - disposed in a public ValueTask method (without async/await)
        private FileStream field_fs6 = new FileStream(@"c:\foo.txt", FileMode.Open);    // Compliant - disposed in a public ValueTask method (without async/await)

        public async Task DisposeAsynchronously()
        {
            await using (var fs = new FileStream(@"c:\foo.txt", FileMode.Open))         // Compliant - automatically disposed with the async using block
            {
                // do nothing
            }

            FileStream fs2;
            await using (fs2 = new FileStream(@"c:\foo.txt", FileMode.Open))
            {
                // do nothing
            }

            FileStream fs3 = new FileStream(@"c:\foo.txt", FileMode.Open);
            await using (fs3)
            {
                // do nothing
            }

            FileStream fs4 = new FileStream(@"c:\foo.txt", FileMode.Open);
            await using (fs4.ConfigureAwait(false))
            {
                // do nothing
            }

            FileStream fs5;
            await using ((fs5 = new FileStream(@"c:\foo.txt", FileMode.Open)).ConfigureAwait(false))
            {
                var fs5_1 = new FileStream(@"c:\foo.txt", FileMode.Open);               // Noncompliant
                fs5_1 = new FileStream(@"c:\foo.txt", FileMode.Open);                   // Noncompliant

                using (var fs5_2 = new FileStream(@"c:\foo.txt", FileMode.Open))
                {
                    fs5_1 = new FileStream(@"c:\foo.txt", FileMode.Open);               // Noncompliant
                }
            }

            FileStream fs6;
            await using ((fs6 = File.Open(@"c:\foo.txt", FileMode.Open)).ConfigureAwait(false))
            {
                // do nothing
            }

            FileStream fs7 = new FileStream(@"c:\foo.txt", FileMode.Open);
            await using (var ignored = fs7.ConfigureAwait(false));

            FileStream fs8 = new FileStream(@"c:\foo.txt", FileMode.Open);
            await using var ignored2 = fs8.ConfigureAwait(false);

            using var fs9 = new FileStream(@"c:\foo.txt", FileMode.Open);

            await using var fs10 = new FileStream(@"c:\foo.txt", FileMode.Open);

            await using var fs11 = File.Open(@"c:\foo.txt", FileMode.Open);

            var fs12 = new FileStream(@"c:\foo.txt", FileMode.Open);                     // Compliant - asynchronously disposed manually
            await fs12.DisposeAsync();
        }

        public async Task SomePublicAsyncMethod()
        {
            await field_fs1.DisposeAsync().ConfigureAwait(false);            
            await field_fs2.DisposeAsync();
        }

        private async Task SomePrivateAsyncMethod()
        {
            await field_fs3.DisposeAsync();
        }

        public async ValueTask SomePublicAsyncMethodWithValueTask()
        {
            await field_fs4.DisposeAsync();
        }

        public ValueTask SomePublicMethodWithValueTask()
        {
            return field_fs5.DisposeAsync();
        }

        public ValueTask AnotherPublicValueTaskMethod() => field_fs6.DisposeAsync();
    }

    public sealed class ImplementsAsyncDisposable : IAsyncDisposable
    {
        private readonly FileStream stream;

        public ImplementsAsyncDisposable()
        {
            stream = new FileStream(@"c:\foo.txt", FileMode.Open);                      // Compliant - see GitHub issue: https://github.com/SonarSource/sonar-dotnet/issues/5879
        }

        public async ValueTask DisposeAsync()
        {
            await stream.DisposeAsync();
        }
    }

    public class AsyncDisposableTest
    {
        private ImplementsAsyncDisposable stream = new ImplementsAsyncDisposable();     // Compliant - the rule only tracks specific IDisposable / IAsyncDisposable types
    }

    public class FluentAssertionsTest
    {
        public void FluentAssertionTypes()
        {
            var scope = new AssertionScope();                                           // Noncompliant
            var s = new FluentAssertions.Execution.AssertionScope();                    // Noncompliant

            using var _ = new AssertionScope();
            using (var disposed = new AssertionScope())
            {
            }
        }
    }

    public ref struct Struct
    {
        public void Dispose()
        {
        }
    }

    public class Consumer
    {
        public void Method()
        {
            using var x = new Struct();
            var y = new Struct();                                                       // Noncompliant
        }
    }
}
