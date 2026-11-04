using System.Numerics;

class NumericsTypes
{
    void Declarations(int a, int b)
    {
        decimal dec = a / b;                    // Noncompliant
        Complex complex = a / b;                // FN NET-4944
        BigInteger big = a / b;                 // Compliant, integral
        complex = (Complex)a / b;               // Compliant
    }

    void Arguments(int a, int b)
    {
        TakeComplex(a / b);                     // FN NET-4944
    }

    void TakeComplex(Complex value) { }
}
