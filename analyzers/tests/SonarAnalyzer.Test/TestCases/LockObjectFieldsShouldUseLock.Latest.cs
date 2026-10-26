using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

partial class Cache
{
    private readonly object objectGate = new();         // Compliant, also returned by a method and property
    private object compatibilityGate = new Lock();      // Compliant, initializer creates a different type
    private readonly Lock lockGate = new();
    private static readonly object staticGate = new();  // Noncompliant [static] {{Use 'System.Threading.Lock' for field 'staticGate'.}}
    private static readonly Lock staticLockGate = new();
    private readonly object unusedGate = new();         // Compliant, never used in a lock
    private readonly List<int> items = new();           // Compliant, holds data
    private readonly Cache otherGate;                   // Compliant, not an object field
    private readonly string name = "cache";

    private object PropertyGate => objectGate;

    void Refresh(Cache other, object localGate)
    {
        lock (objectGate) { }
        lock ((objectGate)) { }
        lock (compatibilityGate) { }
        lock (this.objectGate) { }
        lock (other.objectGate) { }
        lock (Cache.staticGate) { }     // Secondary [static]

        lock (lockGate) { }
        lock (staticLockGate) { }
        lock (PropertyGate) { }
        lock (localGate) { }
        lock (GetGate()) { }
        lock (items) { items.Add(1); }
        lock (otherGate) { }
        lock (name) { }
        lock (other.items) { }
    }

    object GetGate() => objectGate;
}

partial class Cache
{
    private readonly object partialGate = new();        // Noncompliant [partial]
    void RefreshOther() { lock (objectGate) { } }
}

partial class Cache
{
    void RefreshPartial() { lock (partialGate) { } }    // Secondary [partial]
}

class Consumer
{
    void Use(Cache cache, object gate)
    {
        lock (gate) { }
    }
}

class Generic<T>
{
    private readonly object gate = new();               // Noncompliant [generic]

    void M(Generic<int> other) { lock (other.gate) { } }    // Secondary [generic]
}

class GenericMixed<T>
{
    private readonly object gate = new();               // Noncompliant [mixed]

    void M(GenericMixed<string> other)
    {
        lock (this.gate) { }                            // Secondary [mixed]
        lock (other.gate) { }                           // Secondary [mixed]
    }
}

class ConditionalAccess
{
    private readonly object gate = new();               // Noncompliant [conditional]

    void M(ConditionalAccess other)
    {
        lock (other?.gate) { }                          // Secondary [conditional]
        lock ((other?.gate)) { }                        // Secondary [conditional]
    }
}

namespace NestedConditionalAccessTest
{
    class NestedConditionalAccess
    {
        private readonly object gate = new();               // Noncompliant [nestedConditional]

        public static void M(NestedConditionalAccess other)
        {
            lock (other?.Middle?.gate) { _ = other; }       // Secondary [nestedConditional]
            lock ((other?.Middle?.gate)) { _ = other; }     // Secondary [nestedConditional]
            lock (other?.Middle?.Middle?.gate) { _ = other; } // Secondary [nestedConditional]
        }

        NestedConditionalAccess Middle => this;
    }
}

class SafeLockFields
{
    private readonly object gate = new();               // Noncompliant [safe] {{Use 'System.Threading.Lock' for field 'gate'.}}
//                          ^^^^
    private object reassigned = new object();           // Noncompliant [reassigned]
    private readonly object named = (new object());     // Noncompliant [named]
    private object neverLocked = new();                  // Compliant, never locked

    void Use()
    {
        lock (gate) { }                                  // Secondary [safe]
        lock (this.gate) { }                             // Secondary [safe]
        lock (reassigned) { }                            // Secondary [reassigned]
        reassigned = new object();
        lock (named) { }                                 // Secondary [named]
        _ = nameof(named);
        _ = nameof(named.GetType);
        _ = nameof(this.named.ToString);
    }
}

class UnsafeLockFields
{
    public readonly object publicGate = new();          // Compliant, exposed outside the type
    internal readonly object internalGate = new();      // Compliant, exposed outside the type
    private readonly object passedToMethod = new();     // Compliant, also passed to a method
    private readonly object inspected = new();          // Compliant, also used for another purpose
    private readonly object wrongInitializer = new Lock(); // Compliant, initializer is not an object creation
    private readonly object stringInitializer = "";     // Compliant, initializer is not an object creation
    private object wrongAssignment = new();             // Compliant, assigned a different value
    private object first = new(), second = new();        // Compliant, multiple declarators

    void Use()
    {
        lock (publicGate) { }
        lock (internalGate) { }
        lock (passedToMethod) { }
        Consume(passedToMethod);
        lock (inspected) { }
        _ = inspected.GetType();
        lock (wrongInitializer) { }
        lock (stringInitializer) { }
        lock (wrongAssignment) { }
        wrongAssignment = "new value";
        lock (first) { }
        lock (second) { }
    }

    void Consume(object value) { }
}

class YieldingLock
{
    private readonly object gate = new();               // Compliant, a yield inside the lock cannot use Lock.Scope
    private readonly object otherGate = new();          // Noncompliant [yield] {{Use 'System.Threading.Lock' for field 'otherGate'.}}

    IEnumerable<int> Use()
    {
        lock (gate) { yield return 1; }
        lock (otherGate) { }                             // Secondary [yield]
        yield return 2;
    }
}

struct StructLock
{
    private readonly object gate = new();               // Noncompliant [struct] {{Use 'System.Threading.Lock' for field 'gate'.}}

    public StructLock() { }

    void Use() { lock (gate) { } }                        // Secondary [struct]
}

class NestedYield
{
    private readonly object gate = new();               // Noncompliant [nested] {{Use 'System.Threading.Lock' for field 'gate'.}}

    void Use()
    {
        lock (gate)                                      // Secondary [nested]
        {
            IEnumerable<int> Local()
            {
                yield return 1;
            }
            _ = Local();
        }
    }
}

class YieldScoping
{
    private readonly object gate = new();               // Compliant, yield inside a nested block of the lock belongs to the same iterator
    private readonly object lambdaGate = new();         // Noncompliant [lambdaYield]

    IEnumerable<int> Use(bool condition)
    {
        lock (gate)
        {
            if (condition)
            {
                yield return 1;
            }
        }
        lock (lambdaGate)                                // Secondary [lambdaYield]
        {
            Func<int> f = () => 42;
        }
    }
}

class AsyncLocking
{
    private readonly object inAsyncMethod = new();      // Compliant, CS9217: Lock cannot be locked in an async method
    private readonly object inAsyncLambda = new();      // Compliant, CS9217
    private readonly object inAsyncLocalFunction = new(); // Compliant, CS9217
    private readonly object inAnonymousMethod = new();  // Compliant, CS9217
    private readonly object syncInsideAsync = new();    // Noncompliant [syncInsideAsync]
    private readonly object asyncInsideSync = new();    // Compliant, CS9217

    async Task InAsyncMethod()
    {
        lock (inAsyncMethod) { }
        await Task.Yield();
    }

    void InAsyncLambda()
    {
        Func<Task> f = async () =>
        {
            lock (inAsyncLambda) { }
            await Task.Yield();
        };
        Func<Task> g = async delegate
        {
            lock (inAnonymousMethod) { }
            await Task.Yield();
        };
    }

    void InAsyncLocalFunction()
    {
        async Task Local()
        {
            lock (inAsyncLocalFunction) { }
            await Task.Yield();
        }
    }

    async Task SyncInsideAsync()
    {
        void Local()
        {
            lock (syncInsideAsync) { }                   // Secondary [syncInsideAsync]
        }
        Action a = () => { lock (syncInsideAsync) { } }; // Secondary [syncInsideAsync]
        await Task.Yield();
    }

    void AsyncInsideSync()
    {
        lock (asyncInsideSync) { }
        Func<Task> f = async () =>
        {
            lock (asyncInsideSync) { }
            await Task.Yield();
        };
    }
}

class Outer
{
    private readonly object usedByNested = new();       // Compliant, passed to a method from a nested type
    private readonly object lockedByNested = new();     // Noncompliant [nestedType]

    void Use()
    {
        lock (usedByNested) { }
    }

    class Nested
    {
        void Use(Outer outer)
        {
            object o = outer.usedByNested;
            lock (outer.lockedByNested) { }              // Secondary [nestedType]
        }
    }
}
