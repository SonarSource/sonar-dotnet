using System.Security.Cryptography;

class PrimaryConstructor(string ctorParam = "MD5")
{
    void Method(string methodParam = "MD5")
    {
        var md5Ctor = (HashAlgorithm)CryptoConfig.CreateFromName(ctorParam); // FN
        var md5Method = (HashAlgorithm)CryptoConfig.CreateFromName(methodParam); // FN
        var lambda = (string lambdaParam = "MD5") => (HashAlgorithm)CryptoConfig.CreateFromName(lambdaParam); // FN
    }
}
