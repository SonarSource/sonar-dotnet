using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class InsecureHashAlgorithm
{
    const string part1 = """System.Security.Cryptography""";
    const string part2 = """SHA1""";

    void RawStringLiterals(byte[] temp)
    {
        using var SHA1HashAlgorithmWithNamespaceRawStringLiteral = HashAlgorithm.Create("""System.Security.Cryptography.SHA1"""); // Noncompliant
        using var SHA1HashAlgorithmWithNamespaceInterpolatedRawStringLiteral = HashAlgorithm.Create($$"""{{part1}}.{{part2}}"""); // Noncompliant
    }

    void NewlinesInStringInterpolation()
    {
        using var SHA1HashAlgorithm = HashAlgorithm.Create($"{part1 +
            '.' +
            part2}"); // FN (at the moment we validate only constant string)
        using var SHA1HashAlgorithmRawString = HashAlgorithm.Create($$"""{{part1 +
            '.' +
            part2}}"""); // FN (at the moment we validate only constant string)
    }
}

// All the new .NET5 methods should be taken into consideration
// https://github.com/SonarSource/sonar-dotnet/issues/8758
public class Repro_FN_8758
{
    public void Method()
    {
        var data = new byte[42];
        var hashBuffer = new byte[SHA1.HashSizeInBytes];
        using var stream = new System.IO.MemoryStream(data);
        SHA1.HashData(stream);                                                 // Noncompliant
        SHA1.HashData(data);                                                   // Noncompliant
        if (SHA1.TryHashData(data, hashBuffer, out int bytesWrittenSHA1))      // Noncompliant
        { }

        MD5.HashData(data);                                                    // Noncompliant
        if (MD5.TryHashData(data, hashBuffer, out int bytesWrittenMD5))        // Noncompliant
        { }
    }

    public async Task Method2()
    {
        using var stream = new System.IO.MemoryStream(new byte[42]);
        await SHA1.HashDataAsync(stream);          // Noncompliant
        await MD5.HashDataAsync(stream);           // Noncompliant
    }

    private sealed class MyDSA : DSA
    {
        public override byte[] CreateSignature(byte[] rgbHash) => [];

        public override DSAParameters ExportParameters(bool includePrivateParameters) => new DSAParameters();

        public override void ImportParameters(DSAParameters parameters)
        { }

        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature) => true;

        protected override byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm) => [];

        protected override byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm) => [];

        protected override bool TryHashData(ReadOnlySpan<byte> data, Span<byte> destination, HashAlgorithmName hashAlgorithm, out int bytesWritten)
        {
            bytesWritten = 0;
            return false;
        }

        public void UseHashData()
        {
            var data = new byte[42];
            var hashBuffer = new byte[SHA1.HashSizeInBytes];
            using var stream = new System.IO.MemoryStream(data);

            _ = HashData(data, 0, data.Length, HashAlgorithmName.SHA1);                          // Noncompliant
            _ = HashData(stream, HashAlgorithmName.SHA1);                                        // Noncompliant
            if (TryHashData(data, hashBuffer, HashAlgorithmName.SHA1, out int bytesWrittenSHA1)) // Noncompliant
            { }

            _ = HashData(data, 0, data.Length, HashAlgorithmName.SHA3_256);
            _ = HashData(stream, HashAlgorithmName.SHA3_384);
            if (TryHashData(data, hashBuffer, HashAlgorithmName.SHA3_384, out int bytesWrittenSHA3))
            { }

            _ = base.HashData(data, 0, data.Length, HashAlgorithmName.SHA1);                              // Noncompliant
            _ = base.HashData(stream, HashAlgorithmName.SHA1);                                            // Noncompliant
            if (base.TryHashData(data, hashBuffer, HashAlgorithmName.SHA1, out int bytesWrittenSHA1Base)) // Noncompliant
            { }

            _ = base.HashData(data, 0, data.Length, HashAlgorithmName.SHA3_256);
            _ = base.HashData(stream, HashAlgorithmName.SHA3_384);
            if (base.TryHashData(data, hashBuffer, HashAlgorithmName.SHA3_384, out int bytesWrittenSHA3Base))
            { }
        }
    }
}

class PrimaryConstructor(string ctorParam = "MD5")
{
    void Method(string methodParam = "MD5")
    {
        var md5Ctor = (HashAlgorithm)CryptoConfig.CreateFromName(ctorParam); // FN
        var md5Method = (HashAlgorithm)CryptoConfig.CreateFromName(methodParam); // FN
        var lambda = (string lambdaParam = "MD5") => (HashAlgorithm)CryptoConfig.CreateFromName(lambdaParam); // FN
    }
}

class CSharp13
{
    void KMAK_Hashing()
    {
        using var kmac128 = new Kmac128(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // Compliant
        using var kmac256 = new Kmac256(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // Compliant
        using var kmacXof128 = new KmacXof128(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // Compliant
        using var kmacXof256 = new KmacXof256(new byte[] { 0x01, 0x02, 0x03, 0x04 }); // Compliant

        byte[] data = Encoding.UTF8.GetBytes("KMAK");
        byte[] key = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        byte[] mac128 = Kmac128.HashData(key, data, 200); // Compliant
        byte[] mac256 = Kmac256.HashData(key, data, 200); // Compliant
        byte[] macXof128 = KmacXof128.HashData(key, data, 200); // Compliant
        byte[] macXof256 = KmacXof256.HashData(key, data, 200); // Compliant
    }

    // https://sonarsource.atlassian.net/browse/NET-399
    void CryptoOperations(byte[] data, Span<byte> hashSpan, ReadOnlySpan<byte> readOnlyData, Memory<byte> memoryData, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(data);
        CryptographicOperations.HashData(HashAlgorithmName.MD5, data);                                                   // Noncompliant
        CryptographicOperations.HashData(HashAlgorithmName.MD5, readOnlyData, hashSpan);                                 // Noncompliant
        CryptographicOperations.HashData(HashAlgorithmName.MD5, readOnlyData);                                           // Noncompliant
        CryptographicOperations.HashData(HashAlgorithmName.MD5, stream, hashSpan);                                       // Noncompliant
        CryptographicOperations.HashData(HashAlgorithmName.MD5, stream);                                                 // Noncompliant
        CryptographicOperations.HashDataAsync(HashAlgorithmName.MD5, stream, cancellationToken);                         // Noncompliant
        CryptographicOperations.HashDataAsync(HashAlgorithmName.MD5, stream, memoryData, cancellationToken);             // Noncompliant
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, data, data);                                             // Noncompliant
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, data, stream);                                           // Noncompliant
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, readOnlyData, hashSpan);                   // Noncompliant
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, readOnlyData);                             // Noncompliant
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, stream, hashSpan);                         // Noncompliant
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, stream);                                   // Noncompliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, data, stream, cancellationToken);                   // Noncompliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, memoryData, stream, cancellationToken);             // Noncompliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, memoryData, stream, memoryData, cancellationToken); // Noncompliant
        CryptographicOperations.TryHashData(HashAlgorithmName.MD5, readOnlyData, hashSpan, out _);                       // Noncompliant
        CryptographicOperations.TryHmacData(HashAlgorithmName.MD5, readOnlyData, readOnlyData, hashSpan, out _);         // Noncompliant

        CryptographicOperations.HashData(HashAlgorithmName.SHA1, data);                                                  // Noncompliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA1, data, stream);                                     // Noncompliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA1, memoryData, stream, cancellationToken);            // Noncompliant
        CryptographicOperations.TryHashData(HashAlgorithmName.SHA1, readOnlyData, hashSpan, out _);                      // Noncompliant

        CryptographicOperations.HashData(HashAlgorithmName.SHA256, data);                                         // Compliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA3_256, data, stream);                          // Compliant
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA3_384, memoryData, stream, cancellationToken); // Compliant
        CryptographicOperations.TryHashData(HashAlgorithmName.SHA3_512, readOnlyData, hashSpan, out _);           // Compliant
    }

    partial class Partial
    {
        partial HashAlgorithm MyHashAlgorithm { get; }
    }

    partial class Partial
    {
        partial HashAlgorithm MyHashAlgorithm => HashAlgorithm.Create("""System.Security.Cryptography.SHA1"""); // Noncompliant
    }
}
