
#nullable enable

using System;

class NullForgivingOperator
{
    void Basics(int? i)
    {
        _ = i!.Value;               // Compliant, unknown
        i = SomeMethod();
        _ = i!.Value;               // Compliant

        i = null;
        _ = i!.Value;               // Compliant, user-asserted non-empty via bang
        i = new int?();
        _ = i!.Value;               // Compliant
        i = new Nullable<int>();
        _ = i!.Value;               // Compliant

        i = 42;
        _ = i!.Value;               // Compliant

        i = null;
        _ = i.Value;                // Noncompliant
    }

    void CastToValueType(int? i)
    {
        _ = (int)i!;                // Compliant, unknown
        i = SomeMethod();
        _ = (int)i!;                // Compliant

        i = null;
        _ = (int)i!;                // Compliant, user-asserted non-empty via bang
        i = new int?();
        _ = (int)i!;                // Compliant
        i = new Nullable<int>();
        _ = (int)i!;                // Compliant
    }

    void CastToNullableType(int? i)
    {
        _ = ((int?)i)!.Value;       // Compliant, user-asserted non-empty via bang
        _ = (i as int?)!.Value;     // Compliant

        _ = ((int?)null)!.Value;    // Compliant
        _ = (null as int?)!.Value;  // Compliant
    }

    static int? SomeMethod() => null;
}
