using System;
using System.Threading;

class Test
{
    static readonly object staticReadonlyField = null;

    readonly object readonlyField = null;
    object readWriteField = null;

    Test()
    {
        ref object refToReadonlyField = ref readonlyField;
        lock (refToReadonlyField) { }  // Noncompliant, while the reference is to a readonly field, the reference itself is a local variable and as of C# 7.3 can be ref reassigned

        ref object refToReadWriteField = ref readWriteField;
        lock (refToReadWriteField) { } // Noncompliant
    }

    void ReadonlyReferences()
    {
        lock (RefReturnReadonlyField(this)) { }
        lock (RefReturnStaticReadonlyField()) { }
        lock (StaticRefReturnReadonlyField(this)) { }
        lock (StaticRefReturnStaticReadonlyField()) { }

        ref readonly object RefReturnReadonlyField(Test instance) => ref instance.readonlyField;
        ref readonly object RefReturnStaticReadonlyField() => ref Test.staticReadonlyField;
        static ref readonly object StaticRefReturnReadonlyField(Test instance) => ref instance.readonlyField;
        static ref readonly object StaticRefReturnStaticReadonlyField() => ref Test.staticReadonlyField;
    }

    void OnANewInstanceOnStack()
    {
        lock (stackalloc int[] { }) { }              // Error [CS0185]
        lock (stackalloc [] { 1 }) { }               // Error [CS0185]
    }

    void CoalescingAssignment(object oPar)
    {
        lock (oPar ??= readonlyField) { }            // FN, null conditional assignment not supported
    }

    void SwitchExpression(object oPar)
    {
        lock (oPar switch { _ => new object() }) { } // FN, switch expression not supported
    }

    void StringLiterals()
    {
        lock ("""a raw string literal""") // Noncompliant
        { }                    
        lock ($"""an interpolated {"raw string literal"}""") // Noncompliant
        { }
    }

    void TargetTypedObjectCreation()
    {
        lock ((object)new())  // FN
        { }
    }
}

class Records
{
    readonly ARecord readonlyField = new();
    ARecord readWriteField = new();

    static readonly ARecord staticReadonlyField = new();
    static ARecord staticReadWriteField = new();

    void OnAFieldOfTypeRecord()
    {
        lock (readonlyField)
        { }
        lock (readWriteField) // Noncompliant
        { }
        lock (staticReadonlyField)
        { }
        lock (staticReadWriteField) // Noncompliant
        { }
    }

    void OnANewRecordInstance()
    {
        lock (new ARecord()) // Noncompliant
        { }
    }

    record ARecord();
}

class LockObjectType
{
    private readonly Lock _LockReadonly = new();
    private Lock _LockWriteable = new();

    public void LockOnReadonlyLock()
    {
        lock (_LockReadonly)
        { }
    }

    public void LockOnWritableLock()
    {
        lock (_LockWriteable) // Noncompliant
        { }
    }
}
