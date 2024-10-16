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
            var b = "Server = localhost; Database = Test; User = SA; Password = secret";            // Noncompliant
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
            const string part2 = "123";
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
            const string part2 = """123""";
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
            var DBConnectionString1 = "Server=localhost; Database=Test; User=SA; Password=Secret123"u8; // Noncompliant
            var DBConnectionString2 = """Server=localhost; Database=Test; User=SA; Password=Secret123"""u8; // Noncompliant
            var DBConnectionString3 = """
        Server=localhost; Database=Test; User=SA; Password=Secret123
        """u8; // Noncompliant@-2
            var DBConnectionString4 = "Server=localhost; Database=Test; User=SA; Password=Secret123"u8.ToArray(); // Noncompliant
            var DBConnectionString5 = "Server=localhost; Database=Test; User=SA; Password=Secret123"u8.Slice(0);  // Compliant. Only "ToArray" is supported
            var DBConnectionString6 = "Server=localhost; Database=Test; User=SA; \u0050assword=Secret123"u8; // Noncompliant \u0050 is letter 'P'

        }

        void NewlinesInStringInterpolation(string someInput)
        {
            const string test1 = "test1";
            const string test2 = "test2";
            string noncompliant = $"Server = localhost; Database = Test; User = SA; Password ={test1
                + test2}"; // Noncompliant@-1
            string noncompliantRawString = $$"""Server = localhost; Database = Test; User = SA; Password ={{test1
                + test2}}"""; // Noncompliant@-1
        }
    }
}

namespace CSharp12
{
    class PrimaryConstructor(string ctorParam = "Password=123") // Noncompliant
    {
        public void Test(string methodParam = "Password=123") // Noncompliant
        {
            var lambda = (string lambdaParam = "Password=123") => lambdaParam; // FN
        }
    }
}

namespace CSharp13
{
    class EscapeSequence
    {
        void Examples() 
        {
            var DBConnectionString1 = "Server=localhost; Database=Test; User=SA; Password=Secret123"; // Noncompliant
            var DBConnectionString2 = "Server=localhost; Database=Test; User=SA;\ePassword=Secret";   // Noncompliant
            var DBConnectionString3 = "\ePassword=123";       // Noncompliant
            var DBConnectionString4 = "\u001bPassword=123";   // Noncompliant
            var DBConnectionString6 = "Password\e=123";       // Compliant
            var DBConnectionString7 = "Password\u001b=123";   // Compliant
            var DBConnectionString8 = "Password=\e123";       // Noncompliant
            var DBConnectionString9 = "Password=\u001b123";   // Noncompliant
            var DBConnectionString10 = "Password=123\e";      // Noncompliant
            var DBConnectionString11 = "Password=123\u001b";  // Noncompliant
            var DBConnectionString12 = "Passwor\e=123";       // Compliant
            var DBConnectionString13 = "Passwor\u001b=123";   // Compliant
        }   
    }
}
