
const string part1 = """Password""";
const string part2 = """123""";
const string randomString = """
        Random
        Value
        Url
    """;
const string noncompliant = $"""{part1}={part2}"""; // Noncompliant
const string compliant = $"""{randomString}={part2}""";
