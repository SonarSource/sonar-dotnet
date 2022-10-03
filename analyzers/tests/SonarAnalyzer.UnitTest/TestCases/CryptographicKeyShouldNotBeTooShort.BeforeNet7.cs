using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void ConstArgumentResolution()
        {
            var ec3 = new ECDsaOpenSsl();
            ec3.GenerateKey(ECCurve.NamedCurves.brainpoolP192t1); // Noncompliant {{Use a key length of at least 224 bits for EC cipher algorithm.}}
        }
    }
}
