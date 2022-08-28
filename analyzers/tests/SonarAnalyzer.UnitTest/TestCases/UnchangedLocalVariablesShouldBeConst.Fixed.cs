enum Colors { Red, Blue, Green };

class ToReplace
{
    void Alias()
    {
        const string str = "str";
        const char ch = 'c';
        const byte bits = 8;
        const sbyte sbits = 8;
        const short int16 = 16;
        const ushort uint16 = 16;
        const int int32 = 32;
        const uint uint32 = 32;
        const long int64 = 64;
        const long longVal = 1;
        const ulong ulongVal = 1;
        const float single = 32;
        const double floating = 64;
        const decimal dec = 128;
        const Colors enumeration = Colors.Red;
    }

    void Var()
    {
        const string refType = "str";
        const int valueType = 42;
        const Colors enumeration = Colors.Red;
    }

    void Full()
    {
        const System.String str = "str";
    }
}
