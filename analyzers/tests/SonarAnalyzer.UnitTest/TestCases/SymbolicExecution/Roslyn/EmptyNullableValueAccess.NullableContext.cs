#nullable enable

using System;

class NullForgivingOperator
{
    void Basics(int? i)
    {
        _ = i!.Value;               // Compliant, unknown
        i = SomeMethod();
        _ = i!.Value;               // Compliant, unknown

        i = null;
        _ = i!.Value;               // Noncompliant, empty
        i = new int?();
        _ = i!.Value;               // Noncompliant, empty
        i = new Nullable<int>();
        _ = i!.Value;               // Noncompliant, empty

        i = 42;
        _ = i!.Value;               // Compliant, non-empty
    }

    void CastToValueType(int? i)
    {
        _ = (int)i!;                // Compliant, unknown
        i = SomeMethod();
        _ = (int)i!;                // Compliant, unknown

        i = null;
        _ = (int)i!;                // Noncompliant, empty
        i = new int?();
        _ = (int)i!;                // Noncompliant, empty
        i = new Nullable<int>();
        _ = (int)i!;                // Noncompliant, empty
    }

    void CastToNullableType(int? i)
    {
        _ = ((int?)i)!.Value;       // Compliant, unknown
        _ = (i as int?)!.Value;     // Compliant, unknown

        _ = ((int?)null)!.Value;    // Noncompliant
        _ = (null as int?)!.Value;  // Noncompliant
    }

    static int? SomeMethod() => null;
}
