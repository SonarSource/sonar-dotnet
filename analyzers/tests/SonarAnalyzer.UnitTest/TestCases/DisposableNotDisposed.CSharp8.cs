using System;
using System.IO;
using System.Threading.Tasks;

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

            await using var fs3 = new FileStream(@"c:\foo.txt", FileMode.Open);

            await using var fs4 = File.Open(@"c:\foo.txt", FileMode.Open);

            var fs5 = new FileStream(@"c:\foo.txt", FileMode.Open);                     // Compliant - asynchronously disposed manually
            await fs5.DisposeAsync();
        }

        public async Task SomePublicAsyncMethod()
        {
            await field_fs1.DisposeAsync();
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

    // Compliant
    // see GitHub issue: https://github.com/SonarSource/sonar-dotnet/issues/5879
    public sealed class Test : IAsyncDisposable
    {
        private readonly FileStream stream;

        public Test()
        {
            stream = new FileStream("C://some-path", FileMode.CreateNew);
        }

        public async ValueTask DisposeAsync()
        {
            await stream.DisposeAsync();
        }
    }
}
