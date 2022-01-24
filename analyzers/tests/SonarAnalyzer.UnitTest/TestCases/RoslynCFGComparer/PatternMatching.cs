using System;

public class Sample
{
    public void ConstantAndNull(object a, object b)
    {
        var ret = a is 10 && b is null;
    }

    public void IsType(object a)
    {
        var ret = a is string str && str.Length > 0;
    }

    public void IsTypeInConditionChain(object a)
    {
        if (a is string str
            && str.GetType() is { } type
            && type.BaseType is Type baseType
            && baseType.IsAbstract)
        {
            var ret = "value";
        }
    }

    public string SwitchStatementIs(object arg)
    {
        switch (arg)
        {
            case TypeA a:
                return a.ToString();
            case TypeB b:
                return b.ToString();
            case TypeC c:
                return c.ToString();
            default:
                return null;
        }
    }

    public string SwitchStatementIsWhen(object arg)
    {
        switch (arg)
        {
            case TypeA a when a.ValueA == 0:
                return a.ToString();
            case TypeB b when b.ValueB == 0:
                return b.ToString();
            case TypeC c when c.ValueC == 0:
                return c.ToString();
            default:
                return null;
        }
    }

    public string PositionalPattern(object arg)
    {
        return arg switch
        {
            Deconstructed(0, _) => "A is zero",
            Deconstructed(_, 0) => "B is zero",
            _ => "No zero"
        };
    }

    public string PropertyPattern(Wrapper arg)
    {
        return arg switch
        {
            { RootValue: 0 } => "RootValue is zero",
            { A: { ValueA: 0 } } => "A is zero",
            { B: { ValueB: 0 } } => "A is zero",
            { C: { ValueC: 0 } } => "A is zero",
            _ => "Default"
        };
    }

    public string TuplePattern((int, int) tuple)
    {
        return tuple switch
        {
            (0, _) => "First is zero",
            (_, 0) => "Second is zero",
            _ => "No zero"
        };
    }
}

public class Base { }
public class TypeA : Base { public int ValueA; }
public class TypeB : Base { public int ValueB; }
public class TypeC : Base { public int ValueC; }
public class Wrapper
{
    public int RootValue;
    public TypeA A;
    public TypeB B;
    public TypeC C;
}

public class Deconstructed
{
    public int A;
    public int B;
    public void Deconstruct(out int a, out int b)
    {
        a = A;
        b = B;
    }
}
