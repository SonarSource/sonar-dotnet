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
}
