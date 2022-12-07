using System;

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
