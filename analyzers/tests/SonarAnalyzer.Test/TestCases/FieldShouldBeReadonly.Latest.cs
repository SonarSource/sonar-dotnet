using System;

record Person
{
    private int birthYear;     // Noncompliant {{Make 'birthYear' 'readonly'.}}
    int birthMonth = 3;        // Noncompliant

    int legSize1 = 3;
    int legSize2 = 3;
    bool usedInInit = false;

    Person(int birthYear)
    {
        this.birthYear = birthYear;
    }

    public int LegSize
    {
        get
        {
            legSize2++;
            return legSize1;
        }
        init
        {
            legSize1 = value;
            usedInInit = true;
        }
    }
}

record PointerTypes
{
    private readonly nint _nint; // Compliant
    private nint _nint2; // Compliant

    private readonly nuint _nuint; // Compliant
    private nuint _nuint2; // Compliant

    private nint _nint3 = 42; // Noncompliant
    private UIntPtr _nuint3 = 42; // Noncompliant

    private IntPtr _nint4; // Compliant, used by a method
    private IntPtr _nint5 = 42; // Compliant, ++ operation invoked

    private nuint _nuint4; // Compliant, used by a property
    private nuint _nuint5 = 42; // Compliant, -- operation invoked

    nint _ref_nint = 42;  // Compliant, it is passed as ref outside the ctor
    UIntPtr _out_nuint = 42;  // Compliant, it is passed as out outside the ctor

    PointerTypes(nint nint1, nuint nuint1)
    {
        _nint = nint1;
        _nuint = nuint1;
    }

    private void AssignValue()
    {
        _nint4 = 42;

        _nint5++;
        _nuint5--;
    }

    private UIntPtr Get_UIntPtr => _nuint4;

    private void M()
    {
        Method_RefArgument(ref _ref_nint);
        Method_OutArgument(out _out_nuint);
    }

    private void Method_RefArgument(ref nint v) { }
    private void Method_OutArgument(out nuint v) { v = 42; }
}

class Person2
{
    int somefield = 42; // Compliant
    private readonly Action<int> setter;

    Person2(int birthYear)
    {
        setter = i => { somefield >>>= i; };
    }
}

public class FieldReadOnly
{
    private int field = 42;  // Noncompliant

    public int Property
    {
        get => @field;
        set => field = value;
    }
}

public class FieldWriteOnly
{
    private int field = 42;

    public int Property
    {
        get => field;
        set => @field = value;
    }
}

public class NullConditionalAssignment
{
    private int value;  // Compliant

    public void Assign() =>
        this?.value = 100;
}

// https://sonarsource.atlassian.net/browse/NET-744
public static class BitExtensions
{
    extension(ref int bits)
    {
        public int this[int bit]
        {
            get => (bits >> bit) & 1;
            set => bits |= value << bit;
        }
    }
}

public class ExtensionIndexerOnRefReceiver
{
    private int flags = 42;         // Compliant, ref receiver of an extension indexer

    public void Set(int bit) =>
        flags[bit] = 1;
}

public class ExtensionIndexerOnRefReceiverVariants
{
    private int compound = 42;      // Compliant
    private int postfix = 42;       // Compliant
    private int prefix = 42;        // Compliant
    private int thisAccess = 42;    // Compliant

    public void Compound(int bit) =>
        compound[bit] |= 1;

    public void Postfix(int bit) =>
        postfix[bit]++;

    public void Prefix(int bit) =>
        --prefix[bit];

    public void ThisAccess(int bit) =>
        this.thisAccess[bit] = 1;
}

public class ExtensionPropertyOnRefReceiver
{
    private int propertyRead = 42;  // Compliant, ref receiver of an extension property, readonly would not compile (CS0192)
    private int propertyWrite = 42; // Compliant

    public bool Read() =>
        propertyRead.Low;

    public void Write() =>
        propertyWrite.Low = true;
}

public static class BitPropertyExtensions
{
    extension(ref int bits)
    {
        public bool Low
        {
            get => (bits & 1) == 1;
            set => bits |= value ? 1 : 0;
        }
    }
}

public class ExtensionBlockReceiverKinds
{
    private int refMethod = 42;         // Compliant, ref receiver of an extension method
    private int byValue = 42;           // Noncompliant
    private int byIn = 42;              // Noncompliant
    private int byRefReadonly = 42;     // Noncompliant

    public void Use()
    {
        refMethod.Clear();
        _ = byValue.Doubled;
        _ = byIn.Tripled;
        _ = byRefReadonly["low"];
    }
}

public static class ReceiverKindExtensions
{
    extension(ref int bits)
    {
        public void Clear() => bits = 0;
    }

    extension(int bits)
    {
        public int Doubled => bits * 2;
    }

    extension(in int bits)
    {
        public int Tripled => bits * 3;
    }

    extension(ref readonly int bits)
    {
        public bool this[string name] => (bits & 1) == 1;
    }
}
