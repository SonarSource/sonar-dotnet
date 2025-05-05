using System;
using System.Runtime.CompilerServices;

abstract class MyClass
{
    public int this[int a,
                int b,      // Noncompliant
    //          ^^^^^
                   int c]   // Noncompliant
    {
        get
        {
            return a;
        }
        set { }
    }

    public int this[int a, int b, string c]
    {
        get
        {
            return a;
        }
        set { }
    }

    public int this[int a,
                    float b,
                    string c]
    {
        get
        {
            return a;
        }
        set { }
    }

    delegate string AnotherDelegate(string param1, int param2, float param3);
    delegate string AlignedDelegate(string param1,
                                    int param2,
                                    float param3);

    delegate string Delegate(string param1,
                        int param2);  // Noncompliant

    MyClass(int a,
              int b,            // Noncompliant
                  int c) { }    // Noncompliant

    void SameLine(int a, int b, int c)
    {
        Delegate _ = (string param1,
                 int param2) => ""; // Noncompliant
    }

    protected abstract void Aligned(int a,
                                    int b,
                                    int c);

    protected abstract void AlignedNewLine(
        int a,
        int b,
        int c);

    protected abstract void MiddleNotAligned(int a,
                                    int b,  // Noncompliant
                                             int c);

    void NotAligned(int a,
              int b,        // Noncompliant
                  int c)    // Noncompliant
    {
        void LocalMethod(int a,
              int b,            // Noncompliant
                  int c) { }    // Noncompliant
    }

    protected abstract void RefParameter(ref int a,
                                         ref int b,
                                         ref int c);
    protected abstract void RefParameter(ref long a,
                            ref              long b,    // Noncompliant
    //                      ^^^^^^^^^^^^^^^^^^^^^^^
                                    ref long c);        // Noncompliant

    protected abstract void OutParameter(out int a,
                                         out int b,
                                         out int c);
    protected abstract void OutParameter(out long a,
                                      out    long b,    // Noncompliant
                                    out long c);        // Noncompliant

    protected abstract void MixedParameter(int a,
                                           ref int b,
                                           out int c);

    protected abstract void MixedParameter2(int a,
                                        ref int b,   // Noncompliant
                                        out int c);  // Noncompliant

    protected abstract void AttributeParameter([CallerMemberName] string a = "",
                                               [CallerFilePath] string b = "",
                                               [CallerLineNumber] int c = 0);

    protected abstract void AttributeParameter2([CallerMemberName] string a = "",
                                                  [CallerFilePath] string b = "",                 // Noncompliant
    //                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                                                        [CallerLineNumber] int c = 0); // Noncompliant
    protected abstract void ValidTypeParameter<T1,
                                               T2,
                                               T3>();
    protected abstract void ValidTypeParameter<T1, T2, T3>(T1 a, T2 b, T3 c);
    protected abstract void AlignedTypeParameter<T1, T2, T3>(T1 a,
                                                             T2 b,
                                                             T3 c);

    protected abstract void TypeParameter<T1,
        T2,     // Noncompliant
    //  ^^
        T3>();  // Noncompliant

    protected abstract void TypeParameter<T1, T2, T3>(T1 a,
        T2 b,       // Noncompliant
            T3 c);  // Noncompliant
}

unsafe class FunctionPointer
{
    public delegate*<int, int, int> Compliant;
    public delegate*<int,
            int,            //Noncompliant
        int> Noncompliant;  //Noncompliant
}
