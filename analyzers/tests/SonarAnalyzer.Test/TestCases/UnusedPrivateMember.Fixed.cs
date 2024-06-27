using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

public class MyAttribute : Attribute { }

class UnusedPrivateMember
{
    public static void Main() { }

    private class MyOtherClass
    { }

    private class MyClass
    {
        internal MyClass(int i)
        {
            var x = (MyOtherClass)null;
            x = x as MyOtherClass;
            Console.WriteLine();
        }
    }

    private class Gen<T> : MyClass
    {
        public Gen() : base(1)
        {
            Console.WriteLine();
        }
    }

    public UnusedPrivateMember()
    {
        MyProperty = 5;
        MyEvent += UnusedPrivateMember_MyEvent;
        MyUsedEvent += UnusedPrivateMember_MyUsedEvent;
        new Gen<int>();
    }

    private void UnusedPrivateMember_MyUsedEvent(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void UnusedPrivateMember_MyEvent()
    {
        field3 = 5;
        throw new NotImplementedException();
    }
    private
        int field3; // Fixed
    private delegate void Delegate();
    private event Delegate MyEvent; //Fixed
    private int[][] array = new int[0][];
    private Dictionary<int, int> used = new Dictionary<int, int>();

    [MyAttribute] void PrivateMethodWithAttribute() { } // Compliant: due to the attribute

    public int GetValue(int x, int y) => array[x][y];

    public int GetItem(int i) => used[i];

    private Dictionary<int, int> GetDictionary() => null;

    public int GetDictionaryItem(int i) => GetDictionary()[i];

    private event EventHandler<EventArgs> MyUsedEvent
    {
        add { }
        remove { }
    }

    private int MyProperty
    {
        get;
        set;
    }

    [My]
    private class Class1 { }

    // https://github.com/SonarSource/sonar-dotnet/issues/6699
    public void MethodUsingLocalMethod()
    {
        LocalMethod();

        void LocalMethod()
        {
        }
    }
}

class NewClass1
{
    // See https://github.com/SonarSource/sonar-dotnet/issues/888
    static async Task Main() // Compliant - valid main method since C# 7.1
    {
        Console.WriteLine("Test");
    }
}

class NewClass2
{
    static async Task<int> Main() // Compliant - valid main method since C# 7.1
    {
        Console.WriteLine("Test");

        return 1;
    }
}

class NewClass3
{
    static async Task Main(string[] args) // Compliant - valid main method since C# 7.1
    {
        Console.WriteLine("Test");
    }
}

class NewClass4
{
    static async Task<int> Main(string[] args) // Compliant - valid main method since C# 7.1
    {
        Console.WriteLine("Test");

        return 1;
    }
}

class NewClass5
{
}

public static class MyExtension
{
    private static void MyMethod<T>(this T self) { "".MyMethod<string>(); }
}

public class NonExactMatch
{
    private static void M(int i) { }    // Compliant, might be called
    private static void M(string i) { } // Compliant, might be called

    public static void Call(dynamic d)
    {
        M(d);
    }
}

public class EventHandlerSample
{
    private void MyOnClick(object sender, EventArgs args) { } // Compliant
}

public partial class EventHandlerSample1
{
    private void MyOnClick(object sender, EventArgs args) { } // Compliant, event handlers in partial classes are not reported
}

public class PropertyAccess
{
    private int OnlyRead { get; }                                              // Fixed
    private int OnlySet { get; set; }
    private int OnlySet2 { set { } }                             // Fixed
    public int PrivateGetter { set; }                                  // Fixed
    public int PrivateSetter { get; }                                  // Fixed
    private int ExpressionBodiedProperty5 { set => _ = value; }           // Fixed
    private int ExpressionBodiedProperty6 { get => 1; }           // Fixed
    public int ExpressionBodiedProperty7 { set => _ = value; }    // Fixed
    public int ExpressionBodiedProperty8 { get => 1; }    // Fixed

    private int BothAccessed { get; set; }

    private int OnlyGet { get { return 42; } }

    public void M()
    {
        Console.WriteLine(OnlyRead);
        OnlySet = 42;
        (this.OnlySet2) = 42;

        BothAccessed++;

        int? x = 10;
        x = this?.OnlyGet;

        ExpressionBodiedProperty5 = 0;
        Console.WriteLine(ExpressionBodiedProperty6);
    }
}

public class Indexer1
{
}

public class Indexer2
{
}

public class Indexer3
{
}

public class Indexer4
{
}

public class Indexer5
{
    private int this[int i] { get { return 1; } }    // Fixed

    public void Method()
    {
        Console.WriteLine(this[0]);
    }
}

public class Indexer6
{
    private int this[int i] { set { _ = value; } }    // Fixed

    public void Method()
    {
        this[0] = 42;
    }
}

[Serializable]
public sealed class GoodException : Exception
{
    public GoodException()
    {
    }
    public GoodException(string message)
        : base(message)
    {
    }
    public GoodException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
    private GoodException(SerializationInfo info, StreamingContext context) // Compliant because of the serialization
        : base(info, context)
    {
    }
}

public class FieldAccess
{
    private object field1;
    private object field2; // Fixed
    private object field3;

    public FieldAccess()
    {
        this.field2 = field3 ?? this.field1?.ToString();
    }
}

// As S4487 will raise when a private field is written and not read, S1450 won't raise on these cases
// These tests where finding issues before with S1450 and should find them with S4487 now
public class TestsFormerS1450
{
    private int F1 = 0; // Fixed

    public void M1()
    {
        ((F1)) = 42;
    }

    private int F5 = 0; // Fixed
    private int F6; // Fixed
    public void M2()
    {
        F5 = 42;
        F6 = 42;
    }

    private int F14 = 0; // Fixed
    public void M6(int F14)
    {
        this.F14 = 42;
    }
    private int F28 = 42; // Fixed
    public event EventHandler E1
    {
        add
        {
            F28 = 42;
        }
        remove
        {
        }
    }

    private int F36; // Fixed
    public void M15(int i) => F36 = i + 1;
}

public class OutAndRef
{
    private int F37; // Fixed
    public void M37() => int.TryParse("1", out F37);

    private int F38;
    public void M38() => Modify(ref F38);

    public void Modify(ref int x) => x = 37;
    public void M39()
    {
        int.TryParse("1", out var x);
        int.TryParse("1", out var F39);
    }
}

internal class MyClass
{
    protected MyClass() // Compliant
    {
        var a = 1;
    }
}

internal class MyClass2
{
}

public interface IPublicInterface { }
[Serializable]
public sealed class PublicClass : IPublicInterface
{
    public static readonly PublicClass Instance = new PublicClass();

    private PublicClass()
    {
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8342
class Repro_8342
{
    [Private1] public void APublicMethod() => APrivateMethodCalledByAPublicMethod();
    [Private2] internal void AnInternalMethod() { }
    [Private3] protected void AProtectedMethod() { }
    [Private4] private void APrivateMethodCalledByAPublicMethod() { }

    private class Private1Attribute : Attribute { }
    private class Private2Attribute : Attribute { }
    private class Private3Attribute : Attribute { }
    private class Private4Attribute : Attribute { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8532
[Serializable]
public class Repro_8532
{
    private string serializedField;                             // Compliant
    private string serializedReadWriteProperty { get; set; }    // Compliant
}

// https://github.com/SonarSource/sonar-dotnet/issues/9219
class Repro_9219
{
    [MyAttribute]
    public bool AttributeOnProperty
    {
        get;
        private set; // Compliant
    }

    public bool AttributeOnPropertyGetter
    {
        [MyAttribute]
        private get; // Compliant
        set;
    }

    public bool AttributeOnPropertySetter
    {
        get;
        [MyAttribute]
        private set; // Compliant
    }

    public bool AttributeOnPropertyNoncompliant
    {
        [MyAttribute]
        get;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9432
namespace Repro_9432
{
    partial class OuterClass
    {
        public void MethodUsesNestedStruct()
        {
            NestedClass.NestedStruct x;
        }

        private static class NestedClass
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute
            [StructLayout(LayoutKind.Sequential)]
            internal struct NestedStruct
            {
                public int Field;                       // Compliant: the unused field can be used to control the physical layout of struct in the memory
            }
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6990
namespace Repro_6990
{
    public class ChildEventArgs : EventArgs
    {
        // Some properties
    }

    public class InheritedEvent : EventArgs
    {
        // Some properties
    }

    public class SenderObject
    {
        // Some things
    }

    public class Consumer
    {
        static void SenderObject_WithChildEvent(SenderObject senderObject, ChildEventArgs e) // Compliant
        {
            // Logic
        }

        static void SenderObject_WithChildEvent(SenderObject senderObject, InheritedEvent e) // Compliant
        {
            // Logic
        }

        static void Comsumer_WithChildEvent(object sender, ChildEventArgs e) // Compliant
        {
            // Logic
        }
    }
}
