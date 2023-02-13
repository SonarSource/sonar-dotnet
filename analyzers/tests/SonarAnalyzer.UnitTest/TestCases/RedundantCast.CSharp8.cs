using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/3273
    public class CastOnNullable
    {
        public static IEnumerable<string> UsefulCast()
        {
            var nullableStrings = new string?[] { "one", "two", null, "three" };
            return nullableStrings.OfType<string>(); // Compliant - filters out the null
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6438
    public class ArrayOfAnonTypes
    {
        public void UsefulCast()
        {
            var anonArray = new[] { new { X = (string?)"foo" }, new { X = (string?)null } }; // Compliant
            var anon = true switch
            {
                true => new { X = (string?)"foo" }, // Compliant
                false => new { X = (string?)null }  // Compliant
            };
        }
    }
}
