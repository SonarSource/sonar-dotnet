using System;
using System.Collections.Generic;

class UseNullInsteadOfDefault
{
    void Method()
    {
        _ = default;                                                // Error [CS8716]
        var discarded = default;                                    // Error [CS8716]
        int integer = default;
        int x = default, y = 0, z = default(int);
        var integer2 = default(Int32);
        object objectReference = default;                           // Noncompliant {{Use 'null' instead of 'default' for reference types.}}
        //                       ^^^^^^^
        var objectReference2 = default(object);                     // Noncompliant {{Use 'null' instead of 'default' for reference types.}}
        //                     ^^^^^^^^^^^^^^^
        object obj1 = default, obj2 = null, obj3 = default(object);
        //            ^^^^^^^
        //                                         ^^^^^^^^^^^^^^^@-1
        IEnumerable<string> collection = null;
        IEnumerable<string> collection2 = default;                  // Noncompliant
        IEnumerable<string> collection3 = true ? null : default;    // Noncompliant
        collection = null;
        collection2 = default;                                      // Noncompliant
        collection2 = true ? null : default;                        // Noncompliant

        _ = default(object) is null;                                // Noncompliant FP - Do we care?
        _ = collection is default(IEnumerable<string>);             // Noncompliant


        int? nullableInt = default;                                 // Noncompliant

        _ = collection == default;                                  // Noncompliant

        Use(null);
        Use(default);                                               // Noncompliant

        switch (collection)
        {
            case default(IEnumerable<string>):                      // Noncompliant
                break;
            default:
                break;
        }

        switch (integer)
        {
            case default(int):
                break;
            default:
                break;
        }
    }

    void Use(string s) { }
    void OptionalWithNull(string name = null) { }
    void OptionalWithDefault(string name = default) { }             // Noncompliant
    void OptionalWithNull(int? name = null) { }
    void OptionalWithDefault(int name = default) { }
    void OptionalWithDefault(int? name = default) { }               // Noncompliant

    void GenericMethod<T>()
    {
        T t = default;
        var t2 = default(T);
        T? t3 = default;
        T? t4 = default(T);

        t = default;
        t = default(T);
        t3 = default;
        t3 = default(T);

        _ = t == default;           // Error [CS8761]
        _ = t3 == default;          // Error [CS8761]
        _ = t is default(T);        // Error [CS0150]
        _ = t3 is default(T);       // Error [CS0150]

        switch (t)
        {
            case default(T):        // Error [CS0150]
                break;
            default:
                break;
        }

        switch (t3)
        {
            case default(T):        // Error [CS0150]
                break;
            default:
                break;
        }
    }

    void GenericMethodClassConstraint<T>()
        where T : class
    {
        T t = default;              // Noncompliant
        var t2 = default(T);        // Noncompliant
        T? t3 = default;            // Noncompliant
        T? t4 = default(T);         // Noncompliant

        t = default;                // Noncompliant
        t = default(T);             // Noncompliant
        t3 = default;               // Noncompliant
        t3 = default(T);            // Noncompliant

        _ = t == default;           // Noncompliant
        _ = t3 == default;          // Noncompliant
        _ = t3 != default;          // Noncompliant
        _ = t is default(T);        // Noncompliant
        _ = t3 is default(T);       // Noncompliant
        _ = t is not default(T);    // Noncompliant
        _ = t3 is not default(T);   // Noncompliant

        switch (t)
        {
            case default(T):        // Noncompliant
                break;
            default:
                break;
        }

        switch (t3)
        {
            case default(T):        // Noncompliant
                break;
            default:
                break;
        }
    }

    void GenericMethodStructConstraint<T>()
        where T : struct
    {
        T t = default;
        var t2 = default(T);

        T? t3 = default;            // Noncompliant
    }

    void GenericMethodEnumConstraint<T>()
        where T : Enum
    {
        T t = default;
        var t2 = default(T);

        T? t3 = default;            // FN
    }

    void GenericMethodStructEnumConstraint<T>()
        where T : struct, Enum
    {
        T t = default;
        var t2 = default(T);
        T? t3 = default;            // Noncompliant
        T? t4 = default(T);         // FN

        t = default;
        t = default(T);
        t3 = default;               // Noncompliant
        t3 = default(T);

        _ = t == default;           // Error [CS8761]
        _ = t3 == default;          // Error [CS0019]
        _ = t is default(T);        // Error [CS0150]
        _ = t3 is default(T);       // Error [CS0150]

        switch (t)
        {
            case default(T):        // Error [CS0150]
                break;
            default:
                break;
        }

        switch (t3)
        {
            case default(T):        // Error [CS0150]
                break;
            default:
                break;
        }
    }
}
