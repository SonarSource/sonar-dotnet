using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class Sample
{
    public void SymetricAlgorithmCreateEncryptor()
    {
        var initializationVectorConstant = new byte[16];

        using (SymmetricAlgorithm sa = SymmetricAlgorithm.Create("AES"))
        {
            sa.GenerateKey();
            var generateIVNotCalled = sa.CreateEncryptor(sa.Key, sa.IV);
            var constantVector2 = sa.CreateEncryptor(sa.Key, "1234567890123456"u8.ToArray()); // FN
        }
    }
}
