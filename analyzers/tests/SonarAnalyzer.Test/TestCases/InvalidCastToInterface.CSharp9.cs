using System;
using System.Text;

var standalone = new StandaloneClass();
var casted = (ISomething)standalone;    // Noncompliant, this part is not based on SE

var implementing = new ImplementingClass();
casted = (ISomething)implementing;

int? nullable = 42;
var i = (int)nullable;

nullable = null;
i = (int)nullable; // FN {{Nullable is known to be empty, this cast throws an exception.}};

void TopLevelLocalFunction()
{
    var localstandalone = new StandaloneClass();
    var localcasted = (ISomething)localstandalone;    // Noncompliant, this part is not based on SE

    var localimplementing = new ImplementingClass();
    localcasted = (ISomething)localimplementing;

    int? localnullable = 42;
    var i = (int)localnullable;

    localnullable = null;
    i = (int)localnullable; // SE part
}

public interface ISomething { }
public class StandaloneClass { }
public class ImplementingClass : ISomething { }

public class Sample
{
    private string field;

    public void TargetTypedNew()
    {
        StandaloneClass standalone = new();
        var casted = (ISomething)standalone;    // Noncompliant, this part is not based on SE

        ImplementingClass implementing = new();
        casted = (ISomething)implementing;

        int? nullable = 42;
        var i = (int)nullable;

        nullable = null;
        i = (int)nullable; // FN, can't build CFG for this method
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            var standalone = new StandaloneClass();
            var casted = (ISomething)standalone;     // Noncompliant

            var implementing = new ImplementingClass();
            casted = (ISomething)implementing;

            int? nullable = 42;
            var i = (int)nullable;

            nullable = null;
            i = (int)nullable; // SE part
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            var standalone = new StandaloneClass();
            var casted = (ISomething)standalone;     // Noncompliant, this part is not based on SE

            var implementing = new ImplementingClass();
            casted = (ISomething)implementing;

            int? nullable = 42;
            var i = (int)nullable;

            nullable = null;
            i = (int)nullable;  // SE part
        }
    }
}

public record StandaloneRecord { }
public record ImplementingRecord : ISomething { }

public record Record
{
    public void MethodWithClasses()
    {
        var standalone = new StandaloneClass();
        var casted = (ISomething)standalone;     // Noncompliant

        var implementing = new ImplementingClass();
        casted = (ISomething)implementing;

        int? nullable = 42;
        var i = (int)nullable;

        nullable = null;
        i = (int)nullable; // SE part
    }

    public void MethodWithRecords()
    {
        var standalone = new StandaloneRecord();
        var casted = (ISomething)standalone;     // Noncompliant

        var implementing = new ImplementingRecord();
        casted = (ISomething)implementing;
    }
}

public partial class Partial : ISomething
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        var standalone = new StandaloneClass();
        var casted = (ISomething)standalone;     // Noncompliant

        var implementing = new ImplementingClass();
        casted = (ISomething)implementing;

        var implementingPartial = new Partial();
        casted = (ISomething)implementingPartial;

        int? nullable = 42;
        var i = (int)nullable;

        nullable = null;
        i = (int)nullable; // SE part
    }
}
