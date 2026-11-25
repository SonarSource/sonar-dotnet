using System;
using System.Collections.Generic;

// https://github.com/SonarSource/sonar-dotnet/issues/1513
public interface ISample1<T> // Compliant, T cannot be in or out when used in a tuple
{
    (T, object) Process(T item);
}
public interface ISample2<T> // Compliant, T cannot be in or out when used in a tuple
{
    void Process((T, object) item);
}
public interface ISample3<T> // Noncompliant {{Add the 'in' keyword to parameter 'T' to make it 'contravariant'.}}
{
    void Process(T item);
}
public interface ISample4<T> // Noncompliant {{Add the 'out' keyword to parameter 'T' to make it 'covariant'.}}
{
    T Process(object item);
}

public static class Extensions
{
    extension<T>(ISample3<T> sample)
    {
        T Extension(T item) => item;    // Doesn't make the interface compliant
    }

    extension<T>(ISample4<T> sample)
    {
        T Extension(T item) => item;    // Doesn't make the interface compliant
    }
}

public union Result<T>(T, Exception);

public interface IUnionProducer<T> // Compliant, the union struct is invariant in T
{
    Result<T> Get();
}

// https://sonarsource.atlassian.net/browse/NET-5170
public interface IStructProducer<T> where T : struct // Compliant, variance conversions only exist for reference type arguments
{
    T[] Values { get; }
}
public interface IStructProducerOut<out T> where T : struct
{
    T[] Values { get; }
}
public interface IStructConsumer<T> where T : struct // Compliant
{
    void Consume(T item);
}
public interface IClassProducer<T> where T : class // Noncompliant {{Add the 'out' keyword to parameter 'T' to make it 'covariant'.}}
{
    T[] Values { get; }
}
public interface IClassConsumer<T> where T : class // Noncompliant {{Add the 'in' keyword to parameter 'T' to make it 'contravariant'.}}
{
    void Consume(T item);
}
public interface INullableClassProducer<T> where T : class? // Noncompliant
{
    T Get();
}
public interface IUnmanagedProducer<T> where T : unmanaged // Compliant
{
    T Get();
}
public interface IStructEnumProducer<T> where T : struct, Enum // Compliant
{
    T Get();
}
public interface IEnumProducer<T> where T : Enum // Compliant, System.Enum is the only reference type argument, so no variance conversion can apply
{
    T Get();
}
public interface IClassEnumProducer<T> where T : class, Enum // Compliant
{
    T Get();
}
public interface IEnumInterfaceProducer<T> where T : Enum, IComparable // Compliant
{
    T Get();
}
public interface ITransitiveEnumProducer<T, U> where T : U where U : Enum // Compliant
{
    T Get();
}
public interface IDelegateProducer<T> where T : Delegate // Noncompliant, delegate types are reference types with conversions between them
{
    T Get();
}
public interface IStructInterfaceProducer<T> where T : struct, IComparable // Compliant
{
    T Get();
}
public interface IInterfaceProducer<T> where T : IComparable // Noncompliant
{
    T Get();
}
public interface ITransitiveClassProducer<T, U> where T : U where U : class // Noncompliant
{
    T Get();
}
public interface INotNullProducer<T> where T : notnull // Noncompliant
{
    T Get();
}
public interface INewProducer<T> where T : new() // Noncompliant
{
    T Get();
}
public interface IRefStructProducer<T> where T : allows ref struct // Noncompliant, reference type arguments are still allowed
{
    T Get();
}
public interface IRefStructProducerOut<out T> where T : allows ref struct
{
    T Get();
}
public interface IStructRefStructProducer<T> where T : struct, allows ref struct // Compliant
{
    T Get();
}
public interface INullableStructProducer<T> where T : struct // Compliant
{
    T? Get();
}
public interface IStructUnionProducer<T> where T : struct // Compliant
{
    Result<T> Get();
}

// mixed parameter lists: only the class-constrained parameter is reported
public interface IMixed<TStruct, TClass> where TStruct : struct where TClass : class // Noncompliant {{Add the 'out' keyword to parameter 'TClass' to make it 'covariant'.}}
{
    TClass Get(TStruct key);
}
public interface IMixedBothValueTypes<T1, T2> where T1 : struct where T2 : unmanaged // Compliant
{
    T1 Get(T2 key);
}

// shapes that were reported through variant base interfaces / generic arguments before the fix
public interface IStructSequence<T> : IEnumerable<T> where T : struct // Compliant
{
}
public interface IStructEnumerableProducer<T> where T : struct // Compliant
{
    IEnumerable<T> Get();
}
public interface IStructActionConsumer<T> where T : struct // Compliant
{
    void Run(Action<T> action);
}

// class-type constraints keep variance useful
public interface IExceptionProducer<T> where T : Exception // Noncompliant
{
    T Get();
}
public closed class Shape;
public interface IClosedProducer<T> where T : Shape // Noncompliant
{
    T Get();
}

// the constraint is declared on one part only
public partial interface IPartialStruct<T> where T : struct // Compliant
{
    T[] Get();
}
public partial interface IPartialStruct<T>
{
}

public delegate T StructProducerDelegate<T>() where T : struct; // Compliant
public delegate void StructConsumerDelegate<T>(T item) where T : struct; // Compliant
public delegate T UnmanagedProducerDelegate<T>() where T : unmanaged; // Compliant
public delegate T StructEnumProducerDelegate<T>() where T : struct, Enum; // Compliant
public delegate T ClassProducerDelegate<T>() where T : class; // Noncompliant
public delegate void ClassConsumerDelegate<T>(T item) where T : class; // Noncompliant
public delegate T EnumProducerDelegate<T>() where T : Enum; // Compliant
public delegate T TransitiveEnumProducerDelegate<T, U>() where T : U where U : Enum; // Compliant
public delegate T TransitiveClassProducerDelegate<T, U>() where T : U where U : class; // Noncompliant
public delegate TClass MixedDelegate<TStruct, TClass>(TStruct key) where TStruct : struct where TClass : class; // Noncompliant
public delegate IEnumerable<T> StructEnumerableDelegate<T>() where T : struct; // Compliant
public delegate T StructComparableDelegate<T>() where T : struct, IComparable; // Compliant
public delegate T RefStructDelegate<T>() where T : allows ref struct; // Noncompliant
public delegate T NotNullDelegate<T>() where T : notnull; // Noncompliant
