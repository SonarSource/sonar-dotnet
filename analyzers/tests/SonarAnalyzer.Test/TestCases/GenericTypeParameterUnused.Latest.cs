T Return<T>(T v) => v; // Compliant

int Return2<T>(int v) => v; // Noncompliant

public interface Interface
{
    int Add<T>(int a, int b); // Compliant
}

public interface AnotherInterface
{
    static int AddStatic<T>(int a, int b)               // Noncompliant
    {
        return a + b;
    }

    static abstract int AddAbstract<T>(int a, int b);   // Compliant

    static virtual int AddVirtual<T>(int a, int b)      // Compliant: T might be used in implementation of the interface
    {
        return a + b;
    }
}

public record Record1 : Interface
{
    public int Add<T>(int a, int b) // Compliant, interface implementation
    {
        return 0;
    }

    T Return<T>(T v) => v; // Compliant

    int Return<T>(int v) => v; // Noncompliant

    public V DoStuff<T, V>(params V[] o) // Noncompliant
    {
        return o[0];
    }
}

public record Record1<T> // Noncompliant {{'T' is not used in the record.}}
{
}

public record Record2<T>(T X) // Compliant
{
}

public record Record3<T>(int X) // Noncompliant
{
}

public interface IUsedInBody<T>
{
    object WithDefaultImplementation() =>
        default(T);
}

public record struct R : Interface
{
    public int Add<T>(int a, int b) // Compliant, interface implementation
    {
        return 0;
    }

    T Return<T>(T v) => v; // Compliant

    int Return<T>(int v) => v; // Noncompliant

    public V DoStuff<T, V>(params V[] o) // Noncompliant
    {
        return o[0];
    }
}

public record struct RecordStruct1<T> // Noncompliant {{'T' is not used in the record.}}
{
}

public record struct RecordStruct2<T>(T X) // Compliant
{
}

public record struct RecordStruct3<T>(int X) // Noncompliant
{
}

public class InterfaceImplementation : AnotherInterface
{
    public static int AddAbstract<T>(int a, int b)      // Compliant: it is implementing the interface.
    {
        return 0;
    }

    public static int AddVirtual<T>(int a, int b)       // Compliant: it is implementing the interface.
    {
        return 0;
    }
}

public class Example<T>(T param) // Compliant
{
    bool IsNull() => param is null;
}

public partial class PartialConstructor<T>  // Compliant
{
    public partial PartialConstructor(T param);
}

public partial class PartialEvent<T>  // Compliant
{
    partial event System.Action<int, T> SomeEvent;
}

public class NoncompliantWithExtensions<T> { }  // Noncompliant

public static class Extensions
{
    public static void FirstExtension<T>(NoncompliantWithExtensions<T> sender) { }  // Compliant, but doesn't make the class compliant

    extension<T>(NoncompliantWithExtensions<T> sender)
    {
        T Extension(T item) => item;    // Doesn't make the class compliant
    }
}

public class Cat;
public class Dog;

public union UnusedParameter<T>(Cat, Dog); // Noncompliant {{'T' is not used in the union.}} NET-4931
//                           ^

public union UsedParameter<T>(Cat, T);

public union PartiallyUsedParameters<TUsed, TUnused>(Cat, TUsed); // Noncompliant {{'TUnused' is not used in the union.}}
//                                          ^^^^^^^

public union UsedInGenericCaseType<T>(Cat, System.Collections.Generic.List<T>);

public union UsedInMember<T>(Cat, Dog)
{
    public T Default() => default;
}

public union UnusedInMember<T>(Cat, Dog) // Noncompliant
{
    public int Count() => 0;
}

public union UsedOnlyInConstraint<T>(Cat, Dog) where T : class; // Noncompliant

public union UsedInConstraintAndCase<T>(Cat, T) where T : class;

public union UnusedButHasGenericMethod<T>(Cat, Dog) // Noncompliant
{
    public TMethod Method<TMethod>(TMethod value) => value;
}

public union GenericMethodWithUnusedParameter(Cat, Dog)
{
    public int Method<TMethod>() => 0; // Noncompliant {{'TMethod' is not used in the method.}}
}

public closed record class Tree<T>; // Compliant NET-4811

public record class Leaf<T>(T Value) : Tree<T>;

public closed class ClosedClass<T>; // Compliant

public partial union PartialUnionUsedInOtherPart<T>(Cat, Dog);  // Compliant, T is used in the other part

public partial union PartialUnionUnused<T>(Cat, Dog);           // Noncompliant
