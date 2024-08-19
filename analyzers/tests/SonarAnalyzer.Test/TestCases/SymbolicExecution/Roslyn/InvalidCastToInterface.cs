using System;
using System.Collections;
using System.Collections.Generic;

public interface IBase { }
public interface INotImplemented { }
public interface INotImplementedWithBase : IBase { }
public interface IImplemented { }
public interface IGeneric<T> { }
public interface IGeneric<TFirst, TSecond> { }

public class ImplementerOfIBase : IBase { }
public class ImplementerOfIImplemented : IImplemented { }
public class ImplementerOfIIGenericInt : IGeneric<int> { }
public class ImplementerOfIIGenericString : IGeneric<string> { }
public class ImplementerOfIIGenericStringObject : IGeneric<string, object> { }
public class ImplementerOfIIGenericStringString : IGeneric<string, string> { }
public class ImplementerOfIIGenericIntString : IGeneric<int, string> { }

public class EmptyClass { }
public class EmptyBase { }
public class InheritsAndImplements : EmptyBase, IBase { }

public class InvalidCastToInterface
{
    public class Nested : EmptyClass, IDisposable
    {
        public void Dispose() { }
    }

    static void Main()
    {
        var empty = new EmptyClass();
        var x = (IBase)empty;       // Noncompliant {{Review this cast; in this project there's no type that extends 'EmptyClass' and implements 'IBase'.}}
//               ^^^^^
        var a = (IImplemented)x;    // Noncompliant {{Review this cast; in this project there's no type that implements both 'IBase' and 'IImplemented'.}}
        x = empty as IBase;
        bool b = empty is IBase;

        var arr = new EmptyClass[10];
        var arr2 = (IBase[])arr;

        var emptyBase = new EmptyBase();
        var y = (IBase)emptyBase;

        IBase i = new InheritsAndImplements();
        var c = (INotImplemented)i; // Compliant, because INotImplemented doesn't have concrete implementation
        var d = (INotImplemented)i; // Compliant
        var e = (INotImplementedWithBase)i;

        var o = (object)true;
        e = (INotImplementedWithBase)o;

        var z = (IDisposable)new EmptyClass();
    }

    public void Generics()
    {
        var list = new List<int>();
        var iEnumerable = (IEnumerable<int>)list;
        var iList = (IList)list;
        var iCollection = (ICollection)list;

        var fromString = new ImplementerOfIIGenericString();
        var toString = (IGeneric<string>)fromString;
        var toInt = (IGeneric<int>)fromString;  // Noncompliant

        var fromStringObject = new ImplementerOfIIGenericStringObject();
        var toStringObject = (IGeneric<string, object>)fromStringObject;
        var toStringString = (IGeneric<string, string>)fromStringObject;    // Noncompliant
        var toIntString = (IGeneric<int, string>)fromStringObject;          // Noncompliant
        var toSingleTyped = (IGeneric<int>)fromStringObject;                // Noncompliant
    }

    public void Dynamic(dynamic d)
    {
        var b = (IBase)d;
    }

    public void Nullable()
    {
        int? i1 = null;
        var ii = (int)i1; // Compliant, this is handled by S3655
    }
}

interface IFoo { }
interface IBar { }

class Foo : IFoo { }
class Bar : IBar { }
class FooBar : IFoo, IBar { }
sealed class FinalBar : IBar { }

class Other
{
    public void Method<T>(T generic) where T : new()
    {
        IFoo ifoo = null;
        IBar ibar = null;
        Foo foo = null;
        Bar bar = null;
        FooBar foobar = null;
        FinalBar finalbar = null;
        object o = null;

        o = (IFoo)bar;      // Noncompliant
        o = (IFoo)ibar;
        o = (Foo)bar;       // Compliant; causes compiler error // Error [CS0030] - invalid cast
        o = (Foo)ibar;
        o = (IFoo)finalbar; // Compliant; causes compiler error // Error [CS0030] - invalid cast
        o = (Bar)generic;   // Compliant; causes compiler error // Error [CS0030] - invalid cast

        o = bar as IFoo;
        o = ibar as IFoo;
        o = ibar as Foo;
        o = generic as Bar;

        o = bar is IFoo;
        o = ibar is IFoo;
        o = bar is Foo;
        o = ibar is Foo;
        o = finalbar is IFoo;
        o = generic is Bar;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9374
namespace Repro_9374
{
    public abstract class Result<TData> { }

    public interface IFailure { }

    public class Error<TData> : Result<TData>, IFailure { }

    public class Test
    {
        public void TestMethod()
        {
            Result<string> error = new Error<string>();
            var castedError = (IFailure)error;              // Noncompliant - FP
        }
    }
}
