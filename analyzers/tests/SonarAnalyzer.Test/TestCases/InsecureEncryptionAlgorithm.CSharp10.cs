using System.Security.Cryptography;

public struct S
{
    public void Assignment()
    {
        SymmetricAlgorithm a = null;
        (a, var b) = (a, new DESCryptoServiceProvider()); // Noncompliant
    }
}
