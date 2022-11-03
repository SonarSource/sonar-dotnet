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
        const ulong ulongVal = 1;
        const float single = 32;
        const double floating = 64;
        const decimal dec = 128;
        const int i1 = 1,           // Fixed
            i2 = 2;
        const ConsoleColor enumeration = ConsoleColor.Red;
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
