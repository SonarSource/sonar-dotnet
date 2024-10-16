using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

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
    void Method()
    {
        var data = new byte[42];
        using var stream = new System.IO.MemoryStream(data);
        SHA1.HashData(stream);          // FN
        SHA1.HashData(data);            // FN
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

class CSHarp13
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
        CryptographicOperations.HashData(HashAlgorithmName.MD5, data);                                                   // FN
        CryptographicOperations.HashData(HashAlgorithmName.MD5, readOnlyData, hashSpan);                                 // FN
        CryptographicOperations.HashData(HashAlgorithmName.MD5, readOnlyData);                                           // FN
        CryptographicOperations.HashData(HashAlgorithmName.MD5, stream, hashSpan);                                       // FN
        CryptographicOperations.HashData(HashAlgorithmName.MD5, stream);                                                 // FN
        CryptographicOperations.HashDataAsync(HashAlgorithmName.MD5, stream, cancellationToken);                         // FN
        CryptographicOperations.HashDataAsync(HashAlgorithmName.MD5, stream, memoryData, cancellationToken);             // FN
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, data, data);                                             // FN
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, data, stream);                                           // FN
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, readOnlyData, hashSpan);                   // FN
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, readOnlyData);                             // FN
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, stream, hashSpan);                         // FN
        CryptographicOperations.HmacData(HashAlgorithmName.MD5, readOnlyData, stream);                                   // FN
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, data, stream, cancellationToken);                   // FN
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, memoryData, stream, cancellationToken);             // FN
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.MD5, memoryData, stream, memoryData, cancellationToken); // FN
        CryptographicOperations.TryHashData(HashAlgorithmName.MD5, readOnlyData, hashSpan, out _);                       // FN
        CryptographicOperations.TryHmacData(HashAlgorithmName.MD5, readOnlyData, readOnlyData, hashSpan, out _);         // FN

        CryptographicOperations.HashData(HashAlgorithmName.SHA1, data);                                                  // FN
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA1, data, stream);                                     // FN
        CryptographicOperations.HmacDataAsync(HashAlgorithmName.SHA1, memoryData, stream, cancellationToken);            // FN
        CryptographicOperations.TryHashData(HashAlgorithmName.SHA1, readOnlyData, hashSpan, out _);                      // FN

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
