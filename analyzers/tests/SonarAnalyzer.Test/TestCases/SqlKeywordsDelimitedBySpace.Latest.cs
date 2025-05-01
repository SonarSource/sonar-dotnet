using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Linq = System.Linq;

namespace CSharp10
{
    class Examples
    {
        public void VariousSqlKeywords(string unknownValue)
        {
            const string s1 = "TRUNCATE";
            const string s2 = "TABLE HumanResources.JobCandidate;";
            const string noncompliant1 = $"{s1}{s2}"; // Noncompliant {{Add a space before 'TABLE'.}}
//                                             ^^^^

            const string noncompliant2 = $"{s1}TABLE HumanResources.JobCandidate;"; // Noncompliant
//                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            const string noncompliant3 = $"TRUNCATE{s2}"; // Noncompliant
//                                                 ^^^^

            const string s3 = "SELECT e.*, f";
            const string s4 = "ORDER BY LastName";
            const string noncompliant4 = $"{s3}FROM DimEmployee AS e{s4}"; // Noncompliant
//Noncompliant@-1

            const string s5 = "TRUNCATE ";
            const string s6 = "TABLE HumanResources.JobCandidate;";
            const string compliant1 = $"{s5}{s6}";

            const string s7 = "TRUNCATE";
            const string s8 = " TABLE HumanResources.JobCandidate;";
            const string compliant2 = $"{s7}{s8}";

            const string compliant3 = $"{s1} {s2}";
            string compliant4 = $"{s1}{42}";

            string compliant5 = $"{s1}{unknownValue}{s2}";

            string a = string.Empty;
            string b = "TABLE HumanResources.JobCandidate;";
            a = "TRUNCATE";

            string noncompliant5 = $"{a}{b}"; // Noncompliant

            const string complexCase = $"{s1}{$"{s1}{s2}"}"; // Noncompliant
// Noncompliant@-1

            const string complexCase2 = $"{s1}{noncompliant1}"; // Noncompliant {{Add a space before 'TRUNCATETABLE'.}}

            int x = 42;

            (x, var y) = (x, "TRUNCATE" + "TABLE HumanResources.JobCandidate;"); // Noncompliant

            var compliant6 = "UPDATE [some_table] SET [some_column] = @" + $"{nameof(complexCase2)}";
            var compliant7 = "UPDATE [some_table] SET [some_column] = " + $"@{nameof(complexCase2)}";
            var compliant8 = "UPDATE [some_table] SET [some_column] = @" + "VarName";
            var compliant9 = "UPDATE [some_table] SET [some_column] = " + "@VarName";
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6126
    public class Repro_6249
    {
        string SomeColumn { get; set; }

        public void Method()
        {
            const string sql = "UPDATE [some_table]" +
                $"SET [some_column] = @{nameof(SomeColumn)}," + // Noncompliant
                $" [other_column] = @{nameof(SomeColumn)}";
        }
    }

    public class Interpolation
    {
        public const string select = "select";                  // Compliant
        public const string truncate = $"{nameof(truncate)}";   // Compliant
        public const string @from = "from";                     // Compliant

        public const string s1 = $"{select}";                   // Compliant
        public const string s2 = $"{select}Name";               // Noncompliant
        public const string s3 = $"select Name {"from"}";       // Compliant
        public const string s4 = $"select Name {"from"}";       // Compliant
        public const string s5 = $"{select} col1{@from}";       // Noncompliant {{Add a space before 'from'.}}
        //                                      ^^^^^^^
        public const string s6 = $"{select} col1{{{@from}}}";
        //                                               ^^ {{Add a space before '}}'.}}
        //                                        ^^^^^^^@-1 {{Add a space before 'from'.}}
        public const string s7 = $"{select} col1{{{@from}}}";
        //                                        ^^^^^^^ {{Add a space before 'from'.}}
        //                                               ^^@-1 {{Add a space before '}}'.}}

        public const string s8 = truncate + $"col1{@from}"; // here we have an FN on col1 + @from, because of the mixed binary/interpolation scenario
        //                                  ^^^^^^^^^^^^^^ {{Add a space before 'col1{@from}'.}}

        public const string s9 = truncate + $"table";           // Noncompliant
        //                                  ^^^^^^^^
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6355
    public class Repro_6355
    {
        void Example(string parameter)
        {
            const string constOne = "One";
            string nonConstOne = "One";
            string empty = string.Empty;

            const string s0 = $"{constOne}";                // Compliant
            const string s1 = $"{constOne}Two";             // Compliant
            const string s2 = $"{nameof(constOne)}Two";     // Compliant
            const string s3 = $"{parameter}";               // Error [CS0133]
            const string s4 = $"{parameter}Two";            // Error [CS0133]
            const string s5 = $"{nameof(parameter)}Two";    // Compliant
            const string s6 = $"{nonConstOne}";             // Error [CS0133]
            const string s7 = $"{nonConstOne}Two";          // Error [CS0133]
            const string s8 = $"{nameof(nonConstOne)}Two";  // Compliant
            const string s9 = $"{empty}";                   // Error [CS0133]
            const string s10 = $"{empty}Two";               // Error [CS0133]
            const string s11 = $"{nameof(empty)}Two";       // Compliant

            string s12 = $"{constOne}";                     // Compliant
            string s13 = $"{constOne}Two";                  // Compliant
            string s14 = $"{nameof(constOne)}Two";          // Compliant
            string s15 = $"{parameter}";                    // Compliant
            string s16 = $"{parameter}Two";                 // Compliant
            string s17 = $"{nameof(parameter)}Two";         // Compliant
            string s18 = $"{nonConstOne}";                  // Compliant
            string s19 = $"{nonConstOne}Two";               // Compliant
            string s20 = $"{nameof(nonConstOne)}Two";       // Compliant
            string s21 = $"{empty}";                        // Compliant
            string s22 = $"{empty}Two";                     // Compliant
            string s23 = $"{nameof(empty)}Two";             // Compliant

            string s24 = $"{{{nonConstOne}}}";              // Compliant
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-1322
    public class Repro_NET1322
    {
        const string tableName = "MyTableName";
        const string strQuery = $"SELECT * FROM [{tableName}] "; // Noncompliant {{Add a space before 'MyTableName'.}} FP
                                                                 // Noncompliant@-1 {{Add a space before ']'.}} FP
    }
}

namespace CSharp11
{
    class Examples
    {
        void RawStringLiterals(string unknownValue)
        {
            const string s1 = """TRUNCATE"""; // Compliant
            const string s2 = """TABLE HumanResources.JobCandidate;"""; // Compliant

            const string noncompliant1 = $"""{s1}{s2}"""; // Noncompliant {{Add a space before 'TABLE'.}}
//                                               ^^^^
            const string noncompliant2 = $"""{s1}TABLE HumanResources.JobCandidate;"""; // Noncompliant
//                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            const string complexCase = $""""{s1}{$"""{s1}{s2}"""} """"; // Noncompliant
                                                                        // Noncompliant@-1
        }

        void NewlinesInStringInterpolation()
        {
            const string s1 = "truncate";
            const string s2 = "";
            string noncompliant = $"{s1 +
                s2}TABLE HumanResources.JobCandidate;"; // Noncompliant
            string noncompliantRawString = $$"""{{s1 +
                s2}}TABLE HumanResources.JobCandidate;"""; // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9177
    public class Repro_9177
    {
        record ArtifactDto(string Id, string TagIdentifier);

        string sqlQuery1 = $@"
	        SELECT
		        [Artifact].[Id] AS [{nameof(ArtifactDto.Id)}],
		        [Artifact].[TagIdentifier] AS [{nameof(ArtifactDto.TagIdentifier)}]
	        FROM
		        [Artifacts] AS [Artifact]"; // Noncompliant@-3 [Id, bracket_Id] FPs
                                            // Noncompliant@-3 [TagIdentifier, bracket_TagIdentifier] FPs

        string sqlQuery2 = $"""
            SELECT
                [Artifact].[Id] AS [{nameof(ArtifactDto.Id)}],
                [Artifact].[TagIdentifier] AS [{nameof(ArtifactDto.TagIdentifier)}]
            FROM
                [Artifacts] AS [Artifact]
            """; // Noncompliant@-4 [Id2, bracket_Id2] FPs
                 // Noncompliant@-4 [TagIdentifier2, bracket_TagIdentifier2] FPs
    }
}


namespace CSharp12
{
    class PrimaryConstructors
    {
        class C1(string sql = "SELECT x" + "FROM y");          // Noncompliant
        struct S1(string sql = "SELECT x" + "FROM y");         // Noncompliant
        record R1(string sql = "SELECT x" + "FROM y");         // Noncompliant
        record struct RS1(string sql = "SELECT x" + "FROM y"); // Noncompliant
    }

    class DefaultLambdaParameters
    {
        void Test()
        {
            var f1 = (string s = "SELECT x" + "FROM y") => s;                   // Noncompliant
            var f2 = (string s1 = "SELECT x", string s2 = "FROM y") => s1 + s2; // Compliant, different strings
        }
    }

    class CollectionExpressions
    {
        void MonoDimensional()
        {
            IList<string> a;
            a = ["SELECT x" + "FROM y"];            // Noncompliant
            a = ["SELECT x" + """FROM y"""];        // Noncompliant
            a = ["SELECT x", "FROM y"];             // Compliant, different strings
            a = [$"SELECT x{"FROM y"}"];            // Noncompliant
            a = [$$$""""SELECT x{{"FROM y"}}""""];  // Compliant
            a = [$$""""SELECT x{{"FROM y"}}""""];   // Noncompliant
        }

        void MultiDimensional()
        {
            IList<IList<string>> a;
            a = [
                    ["SELECT x" + "FROM y",  // Noncompliant
                "SELECT x"],
                ["FROM y",
                "SELECT x" + "FROM y"]   // Noncompliant
                ];
        }
    }
}

namespace CSharp13
{
    public class MyClass
    {
        // https://sonarsource.atlassian.net/browse/NET-444
        void NewEscapeSequence()
        {
            const string keyword1 = "SELECT";
            const string keyword2 = "FROM";
            const char escape = '\e';

            _ = "\e";
            _ = "\' \" \\ \0 \a \b \e \f \n \r \t \v";
            _ = '\e';
            _ = $"\e{"\e"}";
            _ = $"{'\e'}";
            _ = $@"{'\e'}";
            _ = $"""{'\e'}""";
            _ = $"""
            {'\e'}
            """;
            _ = "SELECT x\eFROM y";            // Compliant: works as expected in SQL server
            _ = "SELECT x" + '\e' + "FROM y";  // Compliant: works as expected in SQL server
            _ = "SELECT x" + "FROM y" + '\e';  // Compliant: works as expected in SQL server
            _ = keyword1 + escape + keyword2 + " table_name";

            _ = " SELECT" + "FROM y";          // Noncompliant
            _ = '\e' + " SELECT" + "FROM y";   // FN
            _ = '\e' + " SELECT x" + "FROM y"; // FN
            _ = "\e" + " SELECT x" + "FROM y"; // FN
            _ = "SELECT" + "FROM y" + '\e';    // FN
            _ = "SELECT x" + "FROM y" + '\e';  // FN
            _ = "SELECT x" + "FROM y" + "\e";  // Noncompliant
        }
    }
}
