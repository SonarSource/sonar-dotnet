using System;
using System.IO;
using System.Data.Common;

public interface IInterface1 : IDisposable { }

class Program
{
    public void DisposedTwice()
    {
        var d = new Disposable();
        d.Dispose();
        d.Dispose(); // Noncompliant {{Resource 'd' has already been disposed explicitly or implicitly through a using statement. Please remove the redundant disposal.}}
    }

    public void DisposedTwice_Conditional()
    {
        IDisposable d = null;
        d = new Disposable();
        if (d != null)
        {
            d.Dispose();
        }
        d.Dispose(); // Noncompliant {{Resource 'd' has already been disposed explicitly or implicitly through a using statement. Please remove the redundant disposal.}}
//      ^^^^^^^^^^^
    }

    public void DisposedTwice_AssignDisposableObjectToAnotherVariable()
    {
        IDisposable d = new Disposable();
        var x = d;
        x.Dispose();
        d.Dispose(); // FN
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

    public void DisposedTwice_Array()
    {
        var a = new[] { new Disposable() };
        a[0].Dispose();
        a[0].Dispose(); // FN
    }

    public void Dispose_Stream_LeaveOpenFalse()
    {
        using (MemoryStream memoryStream = new MemoryStream()) // Compliant
        using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: false))
        {
        }
    }

    public void Dispose_Stream_LeaveOpenTrue()
    {
        using (MemoryStream memoryStream = new MemoryStream()) // Compliant
        using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: true))
        {
        }
    }

    public void Disposed_Using_WithDeclaration()
    {
        using (var d = new Disposable()) // Noncompliant
        {
            d.Dispose();
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

    public void Disposed_Using_Parameters(IDisposable param1)
    {
        param1.Dispose();
        param1.Dispose(); // Noncompliant
    }

    public void Close_OneParameterDisposedTwice(IInterface1 instance1, IInterface1 instance2)
    {
        instance1.Dispose();
        instance1.Dispose(); // Noncompliant
        instance1.Dispose(); // Noncompliant

        instance2.Dispose(); // ok - only disposed once
    }
}

public class Disposable : IDisposable
{
    public void Dispose() { }
}

public class MyClass : IDisposable
{
    public void Dispose() { }

    public void DisposeMultipleTimes()
    {
        Dispose();
        this.Dispose(); // FN
        Dispose(); // FN
    }

    public void DoSomething()
    {
        Dispose();
    }
}

class TestLoops
{
    public static void LoopWithBreak(System.Collections.Generic.IEnumerable<string> list, bool condition, IInterface1 instance1)
    {
        foreach (string x in list)
        {
            try
            {
                if (condition)
                {
                    instance1.Dispose(); // FN
                }
                break;
            }
            catch (Exception)
            {
                continue;
            }
        }
    }

    public static void Loop(System.Collections.Generic.IEnumerable<string> list, bool condition, IInterface1 instance1)
    {
        foreach (string x in list)
        {
            if (condition)
            {
                instance1.Dispose(); // Noncompliant
            }
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
