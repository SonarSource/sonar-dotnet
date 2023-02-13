using System;
using System.Collections.Generic;
using System.Linq;
namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/3273
    public class CastOnNullable
    {
#nullable enable
        public void Simple()
        {
            var nullable = (string?)"Test";         // Compliant
            var nonNullable = (string)nullable!;    // Compliant
            if (nullable != null)
            {
                var s1 = (string)nullable;          // Compliant
                var s2 = (string?)nullable;         // Compliant
            }
            if (nonNullable != null)
            {
                var s1 = (string)nonNullable;       // Compliant
                var s2 = (string?)nonNullable;      // Compliant
            }
        }

        public void NonNullable()
        {
#nullable disable
            var s1 = (string?)"Test";   // Compliant
            var s2 = (string)s1!;       // Compliant
#nullable enable
        }

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
            var anon = new { X = (string?)"foo" };  // Compliant
        }

        public void Array()
        {
            var anonArray = new[] { new { X = (string?)"foo" }, new { X = (string?)null } };    // Compliant
            var oneElementAnonArray = new[] { new { X = (string?)"foo" } };                     // Compliant
            var notSoAnonArray = new[] { new HoldsObject(new { X = (string?)"foo" }) };         // Compliant
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
