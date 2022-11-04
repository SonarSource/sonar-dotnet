using System;

class ToReplace
{
    void Alias()
    {
        const string str = "str";
        const bool b = false;
        const char ch = 'c';
        const byte bits = 8;
        const sbyte sbits = 8;
        const short int16 = 16;
        const ushort uint16 = 16;
        const int int32 = 32;
        const uint uint32 = 32;
        const long int64 = 64;
        const ulong ulongVal = 64;
        const float single = 32;
        const double floating = 64;
        const decimal dec = 128;
        const ConsoleColor enumeration = ConsoleColor.Red;
    }

    void Multiple()
    {
        int int32_0 = 0,           // Fixed
            int32_1 = 1;           // Fixed

        int mixed_0 = DateTime.Now.Day, // Compliant
            mixed_1 = 1,                // Fixed
            mixed_2;                    // Compliant
    }

    void Var()
    {
        const string refType = "str";
        const int valueType = 42;
        const ConsoleColor enumeration = ConsoleColor.Red;
        const AttributeTargets attributeTarget = System.AttributeTargets.All;
    }

    void Full()
    {
        const System.String str = "str";
    }
}
