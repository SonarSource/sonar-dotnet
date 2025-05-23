﻿using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

var x = new RSACryptoServiceProvider(); // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
RSACryptoServiceProvider y = new(); // Noncompliant

record TestRecord
{
    private const int validKeySizeConst = 2048;
    private const int invalidKeySizeConst = 1024;

    private static readonly int validKeySize = 2048;
    private static readonly int invalidKeySize = 1024;

    public void ConstArgumentResolution()
    {
        const int localValidSize = 2048;
        new RSACryptoServiceProvider();                    // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
        new RSACryptoServiceProvider(new CspParameters()); // Noncompliant - has default key size of 1024
        new RSACryptoServiceProvider(new ());              // Error [CS0121]
        new RSACryptoServiceProvider(2048);
        new RSACryptoServiceProvider(localValidSize);
        new RSACryptoServiceProvider(validKeySizeConst);
        new RSACryptoServiceProvider(validKeySize);
        new RSACryptoServiceProvider(invalidKeySize);      // Noncompliant

        const int localInvalidSize = 1024;
        new RSACryptoServiceProvider(1024);        // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        new RSACryptoServiceProvider(1024, new()); // Noncompliant
        new RSACryptoServiceProvider(invalidKeySizeConst); // Noncompliant
        new RSACryptoServiceProvider(localInvalidSize);    // Noncompliant

        RSACryptoServiceProvider provider;
        provider = new ();                                 // Noncompliant {{Use a key length of at least 2048 bits for RSA cipher algorithm.}}
        provider = new (new CspParameters());              // Noncompliant - has default key size of 1024
//                 ^^^^^^^^^^^^^^^^^^^^^^^^^
        provider = new (new ());                           // Error [CS0121]
        provider = new (2048);
        provider = new (localValidSize);
        provider = new (validKeySizeConst);
        provider = new (validKeySize);
        provider = new (invalidKeySize);                   // Noncompliant

        var malformed = new UnknownCryptoServiceProvider();// Error [CS0246]
    }

    public void KeySize()
    {
        ECDiffieHellmanCng ec1 = new();
        ec1.KeySize = 512;
        ec1.KeySize = 128; // OK - because this is not a valid key size for this object

        DSACng dsa1 = new();
        dsa1.KeySize = 512; // Noncompliant {{Use a key length of at least 2048 bits for DSA cipher algorithm.}}
    }

    public void GenerateKey()
    {
        ECDiffieHellmanCng ec1 = new();
        ec1.GenerateKey(ECCurve.NamedCurves.brainpoolP160r1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}

        ECDsaCng ec2 = new();
        ec2.GenerateKey(ECCurve.NamedCurves.brainpoolP160t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}

        ECDsaOpenSsl ec3 = new();
        ec3.GenerateKey(ECCurve.NamedCurves.brainpoolP192t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}

        ECDsaOpenSsl ec4 = new();
        ec4?.GenerateKey(ECCurve.NamedCurves.brainpoolP192t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
        ec4!.GenerateKey(ECCurve.NamedCurves.brainpoolP192t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
    }
}
