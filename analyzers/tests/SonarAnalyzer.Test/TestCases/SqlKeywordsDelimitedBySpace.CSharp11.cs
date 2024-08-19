using System;
using System.Data.SqlClient;
using Linq = System.Linq;

namespace Tests.Diagnostics
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
