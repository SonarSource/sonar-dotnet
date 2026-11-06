using System;

namespace CSharp8
{
    class Program
    {
        private static string SomeMethod() => null;

        public void Concatenations()
        {
            var secret = SomeMethod();

            // Reassigned
            secret ??= "hardcoded";
            var a = "Server = localhost; Database = Test; User = SA; Password = " + secret;         // Compliant: this is not symbolic execution rule and ConstantValueFinder cannot detect that.
            var b = "Server = localhost; Database = Test; User = SA; Password = hardcoded";            // Noncompliant
        }
    }
}

namespace CSharp10
{
    class Program
    {
        public void Test()
        {
            const string part1 = "Password";
            const string part2 = "hardcoded";
            const string randomString = "RandomValue";
            const string noncompliant = $"{part1}={part2}"; // Noncompliant
            const string compliant = $"{randomString}={part2}";
        }
    }
}

namespace CSharp11
{
    class Tests
    {
        void RawStringLiterals()
        {
            const string part1 = """Password""";
            const string part2 = """hardcoded""";
            const string randomString = """
            Random
            Value
            Url
        """;
            const string noncompliant = $"""{part1}={part2}"""; // Noncompliant
            const string compliant = $"""{randomString}={part2}""";
        }

        void Utf8StringLiterals()
        {
            ReadOnlySpan<byte> DBConnectionString0;  // Don't crash if initializer is not present.
            var DBConnectionString1 = "Server=localhost; Database=Test; User=SA; Password=hardcoded"u8;     // Noncompliant
            var DBConnectionString2 = """Server=localhost; Database=Test; User=SA; Password=hardcoded"""u8; // Noncompliant
            var DBConnectionString3 = """
        Server=localhost; Database=Test; User=SA; Password=hardcoded
        """u8; // Noncompliant@-2
            var DBConnectionString4 = "Server=localhost; Database=Test; User=SA; Password=hardcoded"u8.ToArray(); // Noncompliant
            var DBConnectionString5 = "Server=localhost; Database=Test; User=SA; Password=hardcoded"u8.Slice(0);  // Compliant. Only "ToArray" is supported
            var DBConnectionString6 = "Server=localhost; Database=Test; User=SA; \u0050assword=hardcoded"u8;      // Noncompliant \u0050 is letter 'P'

        }

        void NewlinesInStringInterpolation(string someInput)
        {
            const string test1 = "hard";
            const string test2 = "coded";
            string noncompliant = $"Server = localhost; Database = Test; User = SA; Password ={test1
                + test2}"; // Noncompliant@-1
            string noncompliantRawString = $$"""Server = localhost; Database = Test; User = SA; Password ={{test1
                + test2}}"""; // Noncompliant@-1
        }
    }
}

namespace CSharp12
{
    class PrimaryConstructor(string ctorParam = "Password=hardcoded") // Noncompliant
    {
        public void Test(string methodParam = "Password=hardcoded") // Noncompliant
        {
            var lambda = (string lambdaParam = "Password=hardcoded") => lambdaParam; // FN
        }
    }
}

namespace CSharp13
{
    class EscapeSequence
    {
        void Examples()
        {
            var DBConnectionString1 = "Server=localhost; Database=Test; User=SA; Password=hardcoded";   // Noncompliant
            var DBConnectionString2 = "Server=localhost; Database=Test; User=SA;\ePassword=hardcoded";  // Noncompliant
            var DBConnectionString3 = "\ePassword=hardcoded";       // Noncompliant
            var DBConnectionString4 = "\u001bPassword=hardcoded";   // Noncompliant
            var DBConnectionString6 = "Password\e=hardcoded";       // Compliant
            var DBConnectionString7 = "Password\u001b=hardcoded";   // Compliant
            var DBConnectionString8 = "Password=\ehardcoded";       // Noncompliant
            var DBConnectionString9 = "Password=\u001bhardcoded";   // Noncompliant
            var DBConnectionString10 = "Password=hardcoded\e";      // Noncompliant
            var DBConnectionString11 = "Password=hardcoded\u001b";  // Noncompliant
            var DBConnectionString12 = "Passwor\e=hardcoded";       // Compliant
            var DBConnectionString13 = "Passwor\u001b=hardcoded";   // Compliant
        }
    }
}

public class NullConditionalAssignment
{
    public class Sample
    {
        public string ConnectionString { get; set; }
    }
    public void Test(Sample sample)
    {
        sample?.ConnectionString = "Server=localhost; Database=Test; User=SA; Password=hardcoded";  // Noncompliant
    }
}

namespace CSharp15
{
    using System.DirectoryServices;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public class PasswordApis
    {
        public void Control() =>
            _ = new NetworkCredential("u", "hardcoded");                                                                        // Noncompliant

        public async Task Zip(ZipArchive archive, ZipArchiveEntry entry, Stream source)
        {
            entry.Open("hardcoded");                                                                                            // FN NET-5020
            entry.Open(FileAccess.Read, "hardcoded");                                                                           // FN NET-5020
            await entry.OpenAsync("hardcoded", default);                                                                        // FN NET-5020
            await entry.OpenAsync(FileAccess.Read, "hardcoded", default);                                                       // FN NET-5020
            archive.CreateEntry("entry.txt", "hardcoded", ZipEncryptionMethod.Aes256);                                          // FN NET-5020
            archive.CreateEntry("entry.txt", CompressionLevel.Optimal, "hardcoded", ZipEncryptionMethod.Aes256);                // FN NET-5020
            ZipFileExtensions.CreateEntryFromFile(archive, "f", "e", "hardcoded", ZipEncryptionMethod.Aes256);                  // FN NET-5020
            await archive.CreateEntryFromFileAsync("f", "e", "hardcoded".AsMemory(), ZipEncryptionMethod.Aes256, default);      // FN NET-5020
            ZipFile.ExtractToDirectory(source, "directory", new ZipExtractionOptions { Password = "hardcoded".ToCharArray() }); // FN NET-5020
            ZipFile.CreateFromDirectory("directory", "f.zip", new ZipFileCreationOptions { Password = "hardcoded".ToCharArray() }); // FN NET-5020
        }

        public void Crypto(byte[] raw, byte[] salt)
        {
            _ = new X509Certificate2("f.pfx", "hardcoded");                                                                     // FN NET-5020
            _ = new X509Certificate2(raw, "hardcoded");                                                                         // FN NET-5020
            _ = X509CertificateLoader.LoadPkcs12FromFile("f.pfx", "hardcoded");                                                 // FN NET-5020
            _ = new Rfc2898DeriveBytes("hardcoded", salt, 1000, HashAlgorithmName.SHA256);                                      // FN NET-5020
            _ = Rfc2898DeriveBytes.Pbkdf2("hardcoded", salt, 1000, HashAlgorithmName.SHA256, 32);                               // FN NET-5020
            RSA.Create().ImportFromEncryptedPem("pem", "hardcoded");                                                            // FN NET-5020
        }

        public void X25519(X25519DiffieHellman key, PbeParameters pbe, byte[] source)
        {
            _ = X25519DiffieHellman.ImportFromEncryptedPem("pem", "hardcoded");                                                 // FN NET-5020
            _ = X25519DiffieHellman.ImportEncryptedPkcs8PrivateKey("hardcoded", source);                                        // FN NET-5020
            _ = key.ExportEncryptedPkcs8PrivateKey("hardcoded", pbe);                                                           // FN NET-5020
            _ = key.ExportEncryptedPkcs8PrivateKeyPem("hardcoded", pbe);                                                        // FN NET-5020
        }

        public void Ldap() =>
            _ = new DirectoryEntry("LDAP://example.org", "user", "hardcoded");                                                  // FN NET-5020

        public void NonConstantPassword(ZipArchiveEntry entry, string password)
        {
            entry.Open(password);                                                                                               // Compliant
            _ = new X509Certificate2("f.pfx", password);                                                                        // Compliant
            _ = new DirectoryEntry("LDAP://example.org", "user", password);                                                     // Compliant
        }
    }
}
