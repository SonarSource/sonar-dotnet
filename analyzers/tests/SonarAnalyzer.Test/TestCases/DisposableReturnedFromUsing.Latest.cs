using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

record R
{
    public FileStream Method(string path)
    {
        using var fs1 = File.Create(path); // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'fs1'.}}

        using var fs2 = File.Create(path);

        return fs1;
    }
}

public class DisposableReturnedFromUsing
{
    public FileStream Method(string path)
    {
        using var fs1 = File.Create(path); // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'fs1'.}}
//      ^^^^^

        using var fs2 = File.Create(path);

        return fs1;
    }

    public FileStream MethodSingleNoncompliantVariables(string path)
    {
        // Noncompliant@+1 {{Remove the 'using' statement; it will cause automatic disposal of 'fs1'.}}
        using FileStream fs1 = File.Create(path), fs2 = File.Create(path);

        if (path != null)
            return fs1;
        return null;
    }

    public FileStream MethodMultipleNoncompliantVariables(string path)
    {
        // Noncompliant@+1 {{Remove the 'using' statement; it will cause automatic disposal of 'fs1' and 'fs2'.}}
        using FileStream fs1 = File.Create(path), fs2 = File.Create(path);

        if (path != null)
            return fs1;
        return fs2;
    }

    public FileStream MethodWithSwitch(string x)
    {
        using var fs1 = File.Create(x);
        var result = x switch
        {
            "" => fs1,
            "1" => null
        };
        return result; // FN, we don't track aliasing
    }

    public ref struct Struct
    {
        public void Dispose()
        {
        }
    }

    public Struct Foo()
    {
        using (var disposableRefStruct = new Struct()) // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'disposableRefStruct'.}}
        {
            return disposableRefStruct;
        }
    }

    public Struct Bar()
    {
        using var disposableRefStruct = new Struct(); // Noncompliant

        return disposableRefStruct;
    }

    public Struct FooBar()
    {
        using var notReturnedDisposableRefStruct = new Struct();
        var notUsingRefStruct = new Struct();
        return notUsingRefStruct;
    }

    public Struct BarFoo()
    {
        using var foo = new Struct(); // FN - we do not track alias variables
        var bar = foo;
        return bar;
    }
}

class ConditionalAssignment
{
    FileStream stream;

    FileStream Method(ConditionalAssignment x)
    {
        using (x?.stream = File.Create(""))
        {
            return x.stream;    // FN
        }
    }
}
