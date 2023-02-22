using System;
using System.Linq;

class Test
{
    private static readonly object staticReadonlyField = null;
    private static object staticReadWriteField = null;

    private readonly object readonlyField = null;
    private readonly string readonlyStringField = "a string";
    private object readWriteField = null;

    private static object StaticReadonlyProperty => null;
    private object ReadonlyProperty => null;

    private static object StaticReadWriteProperty { get; set; }
    private object ReadWriteProperty { get; set; }

    void OnAStaticField()
    {
        lock (staticReadonlyField) { }
        lock (staticReadWriteField) { }                  // Noncompliant {{'staticReadWriteField' is not 'private readonly', and should not be used for locking.}}
        //    ^^^^^^^^^^^^^^^^^^^^
        lock (Test.staticReadonlyField) { }
        lock (Test.staticReadWriteField) { }             // Noncompliant {{'staticReadWriteField' is not 'private readonly', and should not be used for locking.}}
        //    ^^^^^^^^^^^^^^^^^^^^^^^^^
        lock (AnotherClass.staticReadonlyField) { }      // Noncompliant {{Use fields from 'Test' for locking.}}
        lock (AnotherClass.staticReadWriteField) { }     // Noncompliant {{Use fields from 'Test' for locking.}}
        //    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    }

    void OnAFieldOfSameInstance()
    {
        lock (readonlyField) { }
        lock ((readonlyField)) { }
        lock (readonlyStringField) { }                   // Noncompliant {{Strings can be interned, and should not be used for locking.}}
        lock (readWriteField) { }                        // Noncompliant {{'readWriteField' is not 'private readonly', and should not be used for locking.}}
        lock ((readWriteField)) { }                      // Noncompliant {{'readWriteField' is not 'private readonly', and should not be used for locking.}}
        lock (this.readonlyField) { }
        lock (this.readWriteField) { }                   // Noncompliant {{'readWriteField' is not 'private readonly', and should not be used for locking.}}
        lock ((this.readWriteField)) { }                 // Noncompliant {{'readWriteField' is not 'private readonly', and should not be used for locking.}}
    }

    void OnAFieldOfDifferentInstance()
    {
        var anotherInstance = new Test();
        lock (anotherInstance.readonlyField) { }
        lock (anotherInstance.readWriteField) { }        // Noncompliant {{'readWriteField' is not 'private readonly', and should not be used for locking.}}
        lock (anotherInstance.readonlyField) { }
        lock (anotherInstance?.readWriteField) { }       // FN: ?. not supported
    }

    void OnALocalVariable()
    {
        object localVarNull = null;
        lock (localVarNull) { }                          // Noncompliant {{'localVarNull' is a local variable, and should not be used for locking.}}
        object localVarReadonlyField = readonlyField;
        lock (localVarReadonlyField) { }                 // Noncompliant, while the local variable references a readonly field, the local variable itself can mutate
        object localVarReadWriteField = readWriteField;
        lock (localVarReadWriteField) { }                // Noncompliant
    }

    void OnANewInstance()
    {
        lock (new object()) { }                          // Noncompliant {{Locking on a new instance is a no-op.}}
        lock (new ANamespace.AClass()) { }               // Noncompliant
        lock (new Test[] { }) { }                        // Noncompliant
        lock (new[] { readonlyField }) { }               // Noncompliant
        lock (new Tuple<object>(readonlyField)) { }      // Noncompliant
        lock (new { }) { }                               // Noncompliant

        lock (1) { }                                     // Error [CS0185]
        lock ((a: readonlyField, b: readonlyField)) { }  // Error [CS0185]

        lock (new ADelegate(x => x)) { }                 // Noncompliant
        lock (new Func<int, int>(x => x)) { }            // Noncompliant
        lock (x => x) { }                                // Error [CS0185]
        lock ((int?)1) { }                               // Error [CS0185]

        lock (from x in new object[2] select x) { }      // Noncompliant
    }

    void OnAStringInstance()
    {
        lock ("a string") { }                            // Noncompliant {{Strings can be interned, and should not be used for locking.}}
        lock ($"an interpolated {"string"}") { }         // Noncompliant {{Strings can be interned, and should not be used for locking.}}
        lock ("a" + "string") { }                        // Noncompliant {{Strings can be interned, and should not be used for locking.}}
        lock (MethodReturningString()) { }               // Noncompliant {{Strings can be interned, and should not be used for locking.}}

        string MethodReturningString() => "a string";
    }

    void OnAssignment()
    {
        object x;
        lock (x = readonlyField) { }
        lock (x = readWriteField) { }                    // FN, assignment not supported
    }

    void OtherCases(object oPar, bool bPar, object[] arrayPar)
    {
        lock (null) { }

        lock (oPar) { }

        lock (this) { }

        lock (SomeMethod()) { }
        lock (oPar.GetType()) { }
        lock (typeof(Test)) { }
        lock (default(Test)) { }

        object SomeMethod() => null;

        lock (StaticReadonlyProperty) { }
        lock (ReadonlyProperty) { }
        lock (StaticReadWriteProperty) { }
        lock (ReadWriteProperty) { }

        lock (bPar ? readWriteField : readonlyField) { }

        lock (oPar ?? readonlyField) { }
        lock (oPar = readonlyField) { }

        lock (arrayPar[0]) { }
    }

    void ReadWriteReferences()
    {
        lock (RefReturnReadWriteField(this)) { }         // FN, the method returns a readwrite ref to a member
        lock (RefReturnStaticReadonlyField(this)) { }    // FN, the method returns a readwrite ref to a member

        ref object RefReturnReadWriteField(Test instance) => ref instance.readWriteField;
        ref object RefReturnStaticReadonlyField(Test instance) => ref Test.staticReadWriteField;
    }

    void NoIdentifier()
    {
        lock () { }   // Error
        lock (()) { } // Error
    }

    delegate object ADelegate(object oPar);
}

class TestExplicitCast
{
    private readonly object readonlyField = null;

    void Test()
    {
        lock ((AnotherClass)readonlyField) { } // Compliant, the cast operator can build
    }
}

class AnotherClass
{
    public static readonly object staticReadonlyField = null;
    public static object staticReadWriteField = null;

    public readonly object readonlyField = null;
    public object readWriteField = null;

    public static explicit operator AnotherClass(Test o) => new AnotherClass();
}

class NonPrivateAccessibily
{
    private readonly object privateField = null;
    protected readonly object protectedField = null;
    protected internal readonly object protectedInternalField = null;
    internal readonly object internalField = null;
    public readonly object publicField = null;

    private object PrivateProperty => null;
    protected object ProtectedProperty => null;
    protected internal object ProtectedInternalProperty => null;
    internal object InternalProperty => null;
    public object PublicProperty => null;

    void Test()
    {
        lock (privateField) { }              // Compliant
        lock (protectedField) { }            // Noncompliant
        lock (protectedInternalField) { }    // Noncompliant
        lock (internalField) { }             // Noncompliant
        lock (publicField) { }               // Noncompliant

        lock (PrivateProperty) { }           // Compliant, not a field
        lock (ProtectedProperty) { }         // Compliant, not a field
        lock (ProtectedInternalProperty) { } // Compliant, not a field
        lock (InternalProperty) { }          // Compliant, not a field
        lock (PublicProperty) { }            // Compliant, not a field
    }
}

namespace ANamespace
{
    class AClass { }
}
