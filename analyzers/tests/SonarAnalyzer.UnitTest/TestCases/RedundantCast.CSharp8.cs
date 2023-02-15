using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/3273
    public class CastOnNullable
    {
        public void Simple(string nonNullable, string? nullable)
        {
            _ = (string)"Test";             // Noncompliant
            _ = (string?)"Test";            // Compliant
            _ = (string)null;               // Compliant
            _ = (string?)null;              // Compliant
            _ = (string)nullable;           // Compliant
            _ = (string?)nullable;          // Noncompliant
            _ = (string)nonNullable;        // Noncompliant
            _ = (string?)nonNullable;       // Compliant
            if (nullable != null)
            {
                _ = (string)nullable;       // Noncompliant
                _ = (string?)nullable;      // Compliant
            }
            if (nonNullable == null)
            {
                _ = (string)nonNullable;    // Compliant
                _ = (string?)nonNullable;   // Noncompliant
            }
        }

#nullable disable
        public void NullableDisabled(string nonNullable, string? nullable)
        {
            _ = (string)"Test";             // Noncompliant
            _ = (string?)"Test";            // Noncompliant
            _ = (string)null;               // Compliant
            _ = (string?)null;              // Compliant
            _ = (string)nullable;           // Noncompliant
            _ = (string?)nullable;          // Noncompliant
            _ = (string)nonNullable;        // Noncompliant
            _ = (string?)nonNullable;       // Noncompliant
            if (nullable != null)
            {
                _ = (string)nullable;       // Noncompliant
                _ = (string?)nullable;      // Noncompliant
            }
            if (nonNullable == null)
            {
                _ = (string)nonNullable;    // Noncompliant
                _ = (string?)nonNullable;   // Noncompliant
            }
        }
#nullable enable

        public static IEnumerable<string> Array()
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
