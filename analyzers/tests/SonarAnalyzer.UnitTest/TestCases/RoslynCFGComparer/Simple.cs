using System;
using System.Collections.Generic;

public class Sample
{
    public void BinarySyntax()
    {
        var value = "Lorem" + "Ipsum" + "Dolor" + "Samet";
        Console.WriteLine(value);
    }

    public void Invocation(object arg)
    {
        var value = arg.ToString();
        Console.WriteLine(value);
    }

    public int ArrowAdd(int a, int b) => a + b;

    public void EmptyStatements()
    {
        ; ; ; ; ; ; ; ;
    }

    public void VariableDeclaration()
    {
        int a = 1, b = a + 1, c = 1 + 2 + 3;
    }

    public void Throw()
    {
        var a = "aaa";
        throw new Exception("Message");
        var b = "bbb";
    }

    public void VoidReturnBeforeExit()
    {
        var a = "aaa";
        return;
        var unreachable = "bbb";
    }

    public void Index(string[] arr)
    {
        var value = arr[^1];
    }

    public void Range(string[] arr)
    {
        var value = arr[1..4];
    }

    public void DictionaryInitializer()
    {
        var dict = new Dictionary<string, int>() { { "a", 1 }, { "b", 2 } };
        dict["a"] = 42;
    }

    public void PropertyGet(EmptyBase b)
    {
        var a = b.Property;
    }

    public void PropertySet(EmptyBase b)
    {
        b.Property = "Value";
    }

    public void Dynamic(dynamic arg)
    {
        arg.Field = 42;
        var invocation = arg.Value();
        var field = arg.Field;
    }

    public void NamedArguments()
    {
        ArrowAdd(b: 1, a: 100);
    }
}

public class CallingBase : EmptyBase
{
    public CallingBase() : base(40 + 2)
    {
        var lorem = "Ipsum";
    }
}

public class EmptyBase
{
    public string Property { get; set; } = "Default";

    public EmptyBase(int arg) { }
}

public class ImplicitConstructor : ImplicitBase
{
    // There's hidden implicit constructor calling implicit constructor of the base class
}

public class ImplicitBase
{
    public string Property { get; set; } = "Default";
}
