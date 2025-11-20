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
