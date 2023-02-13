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
    public class AnonTypes
    {
        public void Simple()
        {
            var anon = new { X = (string?)"foo" };  // Noncompliant
        }

        public void Array()
        {
            var anonArray = new[] { new { X = (string?)"foo" }, new { X = (string?)null } };    // Compliant
            var notSoAnonArray = new[] { new HoldsObject(new { X = (string?)"foo" }) };         // Noncompliant
        }

        public void SwitchExpression()
        {
            var anonSwitch = true switch
            {
                true => new { X = (string?)"foo" }, // Compliant
                false => new { X = (string?)null }  // Compliant
            };
        }
    }

    internal class HoldsObject
    {
        object O { get; }
        public HoldsObject(object o)
        {
            O = o;
        }
    }
}
