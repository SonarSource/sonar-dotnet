using System.Security.Cryptography;


var oid = CryptoConfig.MapNameToOID("""DES"""); // Compliant
SymmetricAlgorithm test1 = SymmetricAlgorithm.Create("""DES"""); // Noncompliant

using (MyTripleDESCryptoServiceProvider tripleDES = new())  // Noncompliant {{Use a strong cipher algorithm.}}
{
}

using (DESCryptoServiceProvider des = new())    // Noncompliant
{
}

using (RC2CryptoServiceProvider rc21 = new())   // Noncompliant
{
}

using (AesCryptoServiceProvider aes = new())    // Compliant
{
}

public class MyTripleDESCryptoServiceProvider : TripleDES
{
    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) =>
        throw new System.NotImplementedException();

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) =>
        throw new System.NotImplementedException();

    public override void GenerateIV() =>
        throw new System.NotImplementedException();

    public override void GenerateKey() =>
        throw new System.NotImplementedException();
}

public class InsecureEncryptionAlgorithm
{
    // Rule will raise an issue for both S2278 and S5547 as they are activated by default in unit tests
    public InsecureEncryptionAlgorithm()
    {
        using (MyTripleDESCryptoServiceProvider tripleDES = new())  // Noncompliant {{Use a strong cipher algorithm.}}
        {
        }

        using (DESCryptoServiceProvider des = new())    // Noncompliant
        {
        }

        using (RC2CryptoServiceProvider rc21 = new())   // Noncompliant
        {
        }

        using (AesCryptoServiceProvider aes = new())    // Compliant
        {
        }
    }
}

public struct S
{
    public void Assignment()
    {
        SymmetricAlgorithm a = null;
        (a, var b) = (a, new DESCryptoServiceProvider()); // Noncompliant
    }
}

public partial class PartialProperty
{
    public partial SymmetricAlgorithm Symmetric { get; }
    private partial SymmetricAlgorithm this[int index] { get; } 
}
public partial class PartialProperty
{
    public partial SymmetricAlgorithm Symmetric { get { return new DESCryptoServiceProvider(); } } // Noncompliant
    private partial SymmetricAlgorithm this[int index] { get { return new DESCryptoServiceProvider(); } } // Noncompliant
}

