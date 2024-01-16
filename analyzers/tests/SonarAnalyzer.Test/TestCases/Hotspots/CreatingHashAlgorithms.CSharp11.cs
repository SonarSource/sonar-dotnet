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
}
