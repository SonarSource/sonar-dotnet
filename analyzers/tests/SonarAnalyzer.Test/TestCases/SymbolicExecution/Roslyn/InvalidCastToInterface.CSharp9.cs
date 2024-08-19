using System;
using System.Text;

var standalone = new StandaloneClass();
var casted = (ISomething)standalone;    // Noncompliant {{Review this cast; in this project there's no type that extends 'StandaloneClass' and implements 'ISomething'.}}

var implementing = new ImplementingClass();
casted = (ISomething)implementing;

void TopLevelLocalFunction()
{
    var localstandalone = new StandaloneClass();
    var localcasted = (ISomething)localstandalone;    // Noncompliant

    var localimplementing = new ImplementingClass();
    localcasted = (ISomething)localimplementing;
}

public interface ISomething { }
public class StandaloneClass { }
public class ImplementingClass : ISomething { }

public class Sample
{
    public void TargetTypedNew()
    {
        StandaloneClass standalone = new();
        var casted = (ISomething)standalone;    // Noncompliant

        ImplementingClass implementing = new();
        casted = (ISomething)implementing;
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            var standalone = new StandaloneClass();
            var casted = (ISomething)standalone;     // Noncompliant

            var implementing = new ImplementingClass();
            casted = (ISomething)implementing;
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            var standalone = new StandaloneClass();
            var casted = (ISomething)standalone;     // Noncompliant

            var implementing = new ImplementingClass();
            casted = (ISomething)implementing;
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
    }
}
