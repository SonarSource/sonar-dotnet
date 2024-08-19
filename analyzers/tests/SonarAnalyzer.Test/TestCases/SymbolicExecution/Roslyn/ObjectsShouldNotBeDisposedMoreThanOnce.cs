using System;
using System.IO;
using System.Data.Common;
using System.Collections.Generic;

public interface IWithDispose : IDisposable { }

class Program
{

    public void DisposedTwice()
    {
        var d = new Disposable();
        d.Dispose();
        d.Dispose(); // Noncompliant {{Resource 'd' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
//      ^^^^^^^^^^^
    }

    public void DisposedTwice_Conditional()
    {
        IDisposable d = null;
        d = new Disposable();
        if (d != null)
        {
            d.Dispose();
        }
        d.Dispose(); // Noncompliant {{Resource 'd' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
//      ^^^^^^^^^^^
    }

    private IDisposable disposable;

    public void DisposeField()
    {
        disposable.Dispose();
        disposable.Dispose(); // Noncompliant
    }

    public void DisposedParameters(IDisposable d)
    {
        d.Dispose();
        d.Dispose(); // Noncompliant
    }

    public void DisposePotentiallyNullField(IDisposable d)
    {
        d?.Dispose();
        d?.Dispose(); // Noncompliant
    }

    public void DisposePotentiallyNullField_CheckForNullOnlyOnce_First(IDisposable d)
    {
        d?.Dispose();
        d.Dispose(); // Noncompliant
    }

    public void DisposePotentiallyNullField_CheckForNullOnlyOnce_Second(IDisposable d)
    {
        d.Dispose();
        d?.Dispose(); // Noncompliant
    }

    public void DisposedTwice_Relations()
    {
        IDisposable d = new Disposable();
        var x = d;
        x.Dispose();
        d.Dispose(); // FN, requires relation support
    }

    public void DisposedTwice_Try()
    {
        IDisposable d = null;
        try
        {
            d = new Disposable();
            d.Dispose();
        }
        finally
        {
            d.Dispose(); // Noncompliant
        }
    }

    public void DisposeTwice_CatchFinally(IDisposable d)
    {
        try { }
        catch
        {
            d.Dispose();
        }
        finally
        {
            d.Dispose(); //FN
        }
    }

    public void DisposedTwice_DifferentCase(Disposable d)
    {
        d.DISPOSE();
        d.DISPOSE(); // Compliant, has different case than expected dispose method
    }

    public void DisposedTwice_Array()
    {
        var a = new[] { new Disposable() };
        a[0].Dispose();
        a[0].Dispose(); // FN
    }

    public void Dispose_Stream_LeaveOpenFalse()
    {
        using (MemoryStream memoryStream = new MemoryStream()) // Compliant
        using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: false)) { }
    }

    public void Dispose_Stream_LeaveOpenTrue()
    {
        using (MemoryStream memoryStream = new MemoryStream()) // Compliant
        using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: true)) { }
    }

    public void Disposed_Using_WithDeclaration()
    {
        using (var d = new Disposable()) // Noncompliant
        {
            d.Dispose();
        }
    }

    public void Dispose_NestedUsing()
    {
        using (var d = new Disposable()) // FN
        {
            using (d)
            { }
        }
    }

    public void Disposed_Using_WithExpressions()
    {
        var d = new Disposable();
        using (d) // FN
        {
            d.Dispose();
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/1038
    public void Close_ParametersOfDifferentTypes(IWithDispose withDispose, IDisposable disposable)
    {
        withDispose.Dispose();
        disposable.Dispose();
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/1038
    public void Close_ParametersOfSameType(IWithDispose withDispose1, IWithDispose withDispose2)
    {
        withDispose1.Dispose();
        withDispose2.Dispose();
    }

    public void Close_OneParameterDisposedThrice(IWithDispose withDispose1, IWithDispose withDispose2)
    {
        withDispose1.Dispose();
        withDispose1.Dispose(); // Noncompliant
        withDispose1.Dispose(); // Noncompliant

        withDispose2.Dispose(); // ok - only disposed once
    }
}

public class Disposable : IDisposable
{
    public void Dispose() { }
    public void DISPOSE() { }
}

public class MyClass : IDisposable
{
    public void Dispose() { }
    public static void Dispose(MyClass myclass) { myclass.Dispose(); }

    public void DisposeMultipleTimes()
    {
        Dispose();
        this.Dispose(); // FN
        Dispose(); // FN
    }

    public static void DisposeMultipleTimes(MyClass myclass)
    {
        Dispose(myclass);
        Dispose(myclass); // FN
    }

    public void DoSomething()
    {
        Dispose();
    }
}

class TestLoops
{
    public static void LoopWithBreak(string[] list, bool condition, IWithDispose withDispose)
    {
        foreach (string x in list)
        {
            try
            {
                if (condition)
                {
                    withDispose.Dispose(); // FN
                }
                break;
            }
            catch (Exception)
            {
                continue;
            }
        }
    }

    public static void Loop(string[] list, bool condition, IWithDispose withDispose)
    {
        foreach (string x in list)
        {
            if (condition)
            {
                withDispose.Dispose(); // Noncompliant
            }
        }
    }

    public static void LoopWithCondition(List<Object> toDispose)
    {
        for (int i = toDispose.Count; i < toDispose.Count; i++)
        {
            if (toDispose[i] is IDisposable disposable)
            {
                disposable.Dispose(); // Noncompliant FP
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7497
    public static void LoopOfTuples(List<IDisposable> disposables, List<(int I, IDisposable Disposable)> tuples)
    {
        foreach (var disposable in disposables)
            disposable.Dispose();       // Compliant

        foreach (var (_, disposable) in tuples)
            disposable.Dispose();       // Compliant
    }
}

class UsingDeclaration
{
    public void Disposed_UsingStatement()
    {
        using (var d = new Disposable()) // Noncompliant {{Resource 'd = new Disposable()' has already been disposed explicitly or through a using statement implicitly. Remove the redundant disposal.}}
        {
            d.Dispose();
        }
    }
}

public class Close
{
    public void CloseStreamTwice()
    {
        var fs = new FileStream(@"c:\foo.txt", FileMode.Open);
        fs.Close();
        fs.Close(); // FN - Close on streams is disposing resources
    }

    void CloseTwiceDBConnection(DbConnection connection)
    {
        connection.Open();
        connection.Close();
        connection.Open();
        connection.Close(); // Compliant - close() in DB connection does not dispose the connection object.
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8946
public class Repro_8946
{
    static void Method<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
            if (array[i] is IDisposable d)
            {
                d.Dispose(); // Noncompliant FP
            }
    }
}
