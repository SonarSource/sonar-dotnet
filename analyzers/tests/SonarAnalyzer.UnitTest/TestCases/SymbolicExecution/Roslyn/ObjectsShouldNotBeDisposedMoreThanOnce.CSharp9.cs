using System;
using System.IO;

var topLevel = new MemoryStream();
topLevel.Dispose();
topLevel.Dispose(); // FIXME Non-compliant

using var top = new MemoryStream(); // Compliant

void TopLevelLocalFunction()
{
    var local = new MemoryStream();
    local.Dispose();
    local.Dispose();    // FIXME Non-compliant
}

public class Sample
{
    public void TargetTypedNew()
    {
        MemoryStream ms = new();
        ms.Dispose();
        ms.Dispose();   // FIXME Non-compliant, can't build CFG for this method
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            var ms = new MemoryStream();
            ms.Dispose();
            ms.Dispose();    // FIXME Non-compliant
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            var ms = new MemoryStream();
            ms.Dispose();
            ms.Dispose();    // FIXME Non-compliant
        }
    }
}

public record Record : IDisposable
{
    public void Method()
    {
        var ms = new MemoryStream();
        ms.Dispose();
        ms.Dispose();    // FIXME Non-compliant
    }

    public void Dispose() => throw new NotImplementedException();

    public static void DisposeRecord()
    {
        var r = new Record();
        r.Dispose();
        r.Dispose();    // FIXME Non-compliant
    }
}

public partial class Partial
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        var ms = new MemoryStream();
        ms.Dispose();
        ms.Dispose();    // FIXME Non-compliant
    }
}

namespace TartetTypedConditional
{
    public interface ISomething { }
    public class First : ISomething, IDisposable { public void Dispose() { } }
    public class Second : ISomething, IDisposable { public void Dispose() { } }

    public class Sample
    {
        public void Go(bool condition)
        {
            var a = new First();
            var b = new Second();
            a.Dispose();
            IDisposable result = condition ? a : b;
            result.Dispose();  // FIXME Non-compliant
        }
    }
}

