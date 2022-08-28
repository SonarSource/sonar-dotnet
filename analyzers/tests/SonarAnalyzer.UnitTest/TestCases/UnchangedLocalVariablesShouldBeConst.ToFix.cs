enum Colors { Red, Blue, Green };

class ToReplace
{
    void Alias()
    {
        string str = "str";              // Noncompliant
        char ch = 'c';                   // Noncompliant
        byte bits = 8;                   // Noncompliant
        sbyte sbits = 8;                 // Noncompliant
        short int16 = 16;                // Noncompliant
        ushort uint16 = 16;              // Noncompliant
        int int32 = 32;                  // Noncompliant
        uint uint32 = 32;                // Noncompliant
        long int64 = 64;                 // Noncompliant
        long longVal = 1;                // Noncompliant
        ulong ulongVal = 1;              // Noncompliant
        float single = 32;               // Noncompliant
        double floating = 64;            // Noncompliant
        decimal dec = 128;               // Noncompliant
        Colors enumeration = Colors.Red; // Noncompliant
    }

    void Var()
    {
        var refType = "str";          // Noncompliant
        var valueType = 42;           // Noncompliant
        var enumeration = Colors.Red; // Noncompliant;
    }

    void Full()
    {
        System.String str = "str"; // Noncompliant
    }
}
