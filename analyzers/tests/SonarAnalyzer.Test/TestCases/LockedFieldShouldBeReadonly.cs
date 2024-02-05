using System;
using System.Collections.Generic;
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
        lock (staticReadWriteField) { }                  // Noncompliant {{Do not lock on writable field 'staticReadWriteField', use a readonly field instead.}}
        //    ^^^^^^^^^^^^^^^^^^^^
        lock (Test.staticReadonlyField) { }
        lock (Test.staticReadWriteField) { }             // Noncompliant {{Do not lock on writable field 'staticReadWriteField', use a readonly field instead.}}
        //    ^^^^^^^^^^^^^^^^^^^^^^^^^
        lock (AnotherClass.staticReadonlyField) { }
        lock (AnotherClass.staticReadWriteField) { }     // Noncompliant {{Do not lock on writable field 'staticReadWriteField', use a readonly field instead.}}
        //    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    }

    void OnAFieldOfSameInstance()
    {
        lock (readonlyField) { }
        lock (readonlyStringField) { }                   // Noncompliant {{Do not lock on strings as they can be interned, use a readonly field instead.}}
        lock (readWriteField) { }                        // Noncompliant {{Do not lock on writable field 'readWriteField', use a readonly field instead.}}
        lock (this.readonlyField) { }
        lock (this.readWriteField) { }                   // Noncompliant {{Do not lock on writable field 'readWriteField', use a readonly field instead.}}
    }

    void OnParenthesizedExpressions()
    {
        lock ((readonlyField)) { }
        lock ((readWriteField)) { }                      // Noncompliant {{Do not lock on writable field 'readWriteField', use a readonly field instead.}}
        lock ((this.readWriteField)) { }                 // Noncompliant {{Do not lock on writable field 'readWriteField', use a readonly field instead.}}
    }

    void OnAFieldOfDifferentInstance()
    {
        var anotherInstance = new Test();
        lock (anotherInstance.readonlyField) { }
        lock (anotherInstance.readWriteField) { }        // Noncompliant {{Do not lock on writable field 'readWriteField', use a readonly field instead.}}
        lock (anotherInstance.readonlyField) { }
        lock (anotherInstance?.readWriteField) { }       // FN: ?. not supported
    }

    void OnALocalVariable()
    {
        object localVarNull = null;
        lock (localVarNull) { }                          // Noncompliant {{Do not lock on local variable 'localVarNull', use a readonly field instead.}}
        object localVarReadonlyField = readonlyField;
        lock (localVarReadonlyField) { }                 // Noncompliant, while the local variable references a readonly field, the local variable itself can mutate
        object localVarReadWriteField = readWriteField;
        lock (localVarReadWriteField) { }                // Noncompliant
    }

    void OnALocalOutVar(Dictionary<int, object> lockObjs)
    {
        if (lockObjs.TryGetValue(42, out var lockObj))
        {
            lock (lockObj) { }                           // Noncompliant, FP: the lock object is a local variable retrieved from a collection of locks
        }
    }

    void OnANewInstance()
    {
        lock (new object()) { }                          // Noncompliant {{Do not lock on a new instance because is a no-op, use a readonly field instead.}}
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
        lock ("a string") { }                            // Noncompliant {{Do not lock on strings as they can be interned, use a readonly field instead.}}
        lock ($"an interpolated {"string"}") { }         // Noncompliant
        lock ("a" + "string") { }                        // Noncompliant
        lock (MethodReturningString()) { }               // Noncompliant

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
        lock () { }   // Error [CS1525]
        lock (()) { } // Error [CS1525]
    }

    delegate object ADelegate(object oPar);
}

class TestExplicitCast
{
    private readonly object readonlyField = null;

    void Test()
    {
        lock ((AnotherClass)readonlyField) { } // Compliant, the cast operator can run arbitrary code
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

class FieldAccessibily
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

    void FieldAccessibilityDoesntMatter()
    {
        lock (privateField) { }
        lock (protectedField) { }
        lock (protectedInternalField) { }
        lock (internalField) { }
        lock (publicField) { }
    }

    void RuleDoesntRaiseOnProperties()
    {
        lock (PrivateProperty) { }
        lock (ProtectedProperty) { }
        lock (ProtectedInternalProperty) { }
        lock (InternalProperty) { }
        lock (PublicProperty) { }
    }
}

namespace ANamespace
{
    class AClass { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7058
class Repro_7058
{
    void LockInsideCapture()
    {
        var local = new object();
        Action action = () =>
        {
            lock (local) { } // Noncompliant, FP: local is in the captured scope
        };
    }

    void LockOutsideCapture()
    {
        var local = new object();
        Action action = () =>
        {
            var localCaptured = local;
        };
        lock (local) { }     // Noncompliant, locking captured variable in its declaration scope
    }
}
