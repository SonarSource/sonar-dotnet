using System.Security.Cryptography;


var oid = CryptoConfig.MapNameToOID("""DES"""); // Compliant
SymmetricAlgorithm test1 = SymmetricAlgorithm.Create("""DES"""); // Noncompliant
