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
    var DBConnectionString1 = "Server=localhost; Database=Test; User=SA; Password=Secret123"u8; // FN
    var DBConnectionString2 = """Server=localhost; Database=Test; User=SA; Password=Secret123"""u8; // FN
    var DBConnectionString3 = """
        Server=localhost; Database=Test; User=SA; Password=Secret123
        """u8; // FN
}

void NewlinesInStringInterpolation(string someInput)
{
    const string test1 = "test1";
    const string test2 = "test2";
    string noncompliant = $"Server = localhost; Database = Test; User = SA; Password ={test1 // Noncompliant
        + test2}";
    string noncompliantRawString = $$"""Server = localhost; Database = Test; User = SA; Password ={{test1 // Noncompliant
        + test2}}""";
}
