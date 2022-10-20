using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    public class InsecureHashAlgorithm
    {
        public void Hash(byte[] temp)
        {
            string part1 = """System.Security.Cryptography""";
            string part2 = """SHA1""";
            using var SHA1HashAlgorithmWithNamespaceRawStringLiteral = HashAlgorithm.Create("""System.Security.Cryptography.SHA1"""); // Noncompliant
            using var SHA1HashAlgorithmWithNamespaceInterpolatedRawStringLiteral = HashAlgorithm.Create($$"""{{part1}}.{{part2}}""");
        }
    }
}
