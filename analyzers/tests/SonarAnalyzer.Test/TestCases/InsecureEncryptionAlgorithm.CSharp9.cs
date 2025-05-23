﻿using System.Security.Cryptography;

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
