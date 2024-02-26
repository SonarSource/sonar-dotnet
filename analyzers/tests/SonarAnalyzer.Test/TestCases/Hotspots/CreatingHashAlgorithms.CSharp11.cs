using System.Security.Cryptography;

namespace Tests.Diagnostics
{
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
}
