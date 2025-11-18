using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Execution;

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
        await using (var ignored = fs7.ConfigureAwait(false))
            ;

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

class Foo
{
    public void Bar(object cond)
    {
        var fs = new FileStream("", FileMode.Open); // FN, not disposed on all paths
        if (cond is 5)
        {
            fs.Dispose();
        }
        else if (cond is not 599)
        {
            fs.Dispose();
        }
    }

    public void Lambdas()
    {
        Action<int, int> a = static (int v, int w) => {
            var fs = new FileStream("", FileMode.Open); // Noncompliant
        };
        Action<int, int> b = (_, _) => {
            var fs = new FileStream("", FileMode.Open);
            fs.Dispose();
        };
        Action<int, int> с = static (int v, int w) => {
            FileStream fs = new("", FileMode.Open); // Noncompliant
        };
        Action<int, int> в = (_, _) => {
            FileStream fs = new("", FileMode.Open);
            fs.Dispose();
        };
    }
}

record MyRecord
{
    private FileStream field_fs1; // Compliant - not instantiated
    public FileStream field_fs2 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - public
    private FileStream field_fs3 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant {{Dispose 'field_fs3' when it is no longer needed.}}
    private FileStream field_fs4 = new FileStream(@"c:\foo.txt", FileMode.Open); // Compliant - disposed
    private FileStream field_fs5 = new(@"c:\foo.txt", FileMode.Open); // Noncompliant {{Dispose 'field_fs5' when it is no longer needed.}}

    private FileStream backing_field1;
    public FileStream Prop1
    {
        init
        {
            backing_field1 = new FileStream("", FileMode.Open); // Noncompliant
        }
    }

    private FileStream backing_field2;
    public FileStream Prop2
    {
        init
        {
            backing_field2 = new("", FileMode.Open); // Noncompliant
        }
    }

    public void Foo()
    {
        field_fs4.Dispose();

        FileStream fs5; // Compliant - used properly
        using (fs5 = new FileStream(@"c:\foo.txt", FileMode.Open))
        {
            // do nothing but dispose
        }

        using (fs5 = new(@"c:\foo.txt", FileMode.Open))
        {
            // do nothing but dispose
        }

        FileStream fs1 = new(@"c:\foo.txt", FileMode.Open);        // Noncompliant
        var fs2 = File.Open(@"c:\foo.txt", FileMode.Open);         // Noncompliant - instantiated with factory method
        var s = new WebClient();                                   // Noncompliant - another tracked type
    }
}

public class DisposableNotDisposed
{
    private struct InnerStruct
    {
        public InnerStruct() { }

        private FileStream inner_field_fs1 = new FileStream(@"c:\foo.txt", FileMode.Open); // Noncompliant - should be reported on once
    }
}

class FieldKeyWord
{
    public FileStream FS
    {
        get => field;
        private set => field = value;
    } = new(@"c:\foo.txt", FileMode.Open);  // Compliant: technically `field` is a private field but this is like an auto-property, which is out of scope
}
