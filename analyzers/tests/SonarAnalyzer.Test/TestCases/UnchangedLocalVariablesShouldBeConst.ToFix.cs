using System;

class ToReplace
{
    void Alias()
    {
        string str = "str";   // Noncompliant
        bool b = false;       // Noncompliant
        char ch = 'c';        // Noncompliant
        byte bits = 8;        // Noncompliant
        sbyte sbits = 8;      // Noncompliant
        short int16 = 16;     // Noncompliant
        ushort uint16 = 16;   // Noncompliant
        int int32 = 32;       // Noncompliant
        uint uint32 = 32;     // Noncompliant
        long int64 = 64;      // Noncompliant
        ulong ulongVal = 64;  // Noncompliant
        float single = 32;    // Noncompliant
        double floating = 64; // Noncompliant
        decimal dec = 128;    // Noncompliant
        ConsoleColor enumeration = ConsoleColor.Red; // Noncompliant
    }

    void Multiple()
    {
        int int32_0 = 0,           // Noncompliant
            int32_1 = 1;           // Noncompliant

        int mixed_0 = DateTime.Now.Day, // Compliant
            mixed_1 = 1,                // Noncompliant
            mixed_2;                    // Compliant
    }

    void Var()
    {
        var refType = "str"; // Noncompliant
        var valueType = 42;  // Noncompliant
        var enumeration = ConsoleColor.Red; // Noncompliant;
        var attributeTarget = System.AttributeTargets.All; // Noncompliant;
    }

    void Full()
    {
        System.String str = "str"; // Noncompliant
    }
}
