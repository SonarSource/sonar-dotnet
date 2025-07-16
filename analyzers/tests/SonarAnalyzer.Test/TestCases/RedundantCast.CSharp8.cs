using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Tests.Diagnostics
{
    public class InvocationTests
    {
        public static void Invocations()
        {
            var ints = new int[1];
            var objects= new object[1];
            var moreInts = new int[1][];
            var moreObjects = new object[1][];
            ints?.Cast<int>();              // Noncompliant
            objects?.Cast<int>();           // Compliant
            moreInts[0].Cast<int>();        // Noncompliant
            moreObjects[0].Cast<int>();     // Compliant
            moreInts[0]?.Cast<int>();       // Noncompliant
            moreObjects[0]?.Cast<int>();    // Compliant
            GetInts().Cast<int>();          // Noncompliant
            GetObjects().Cast<int>();       // Compliant
            GetInts()?.Cast<int>();         // Noncompliant
            GetObjects()?.Cast<int>();      // Compliant
            Enumerable.Cast<int>();         // Error [CS7036] - overload resolution failure
        }

        public static int[] GetInts() => null;
        public static object[] GetObjects() => null;
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3273
    public class CastOnNullable
    {
        public static IEnumerable<string> Array()
        {
            var nullableStrings = new string?[] { "one", "two", null, "three" };
            return nullableStrings.OfType<string>(); // Compliant - filters out the null
        }

        public void Tuple()
        {
            _ = (a: (string?)"", b: "");    // Compliant
        }

        public void ValueTypes(int nonNullable, int? nullable)
        {
            _ = (int?)nonNullable;  // Compliant
            _ = (int?)nullable;     // Noncompliant
            _ = (int)nonNullable;   // Noncompliant
            _ = (int)nullable;      // Compliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6438
    public class AnonTypes
    {
        public void Simple(string nonNullable, string? nullable)
        {
            _ = new { X = (string?)nonNullable };   // Compliant
            _ = new { X = (string?)nullable };      // Noncompliant
            _ = new { X = (string)nonNullable };    // Noncompliant
            _ = new { X = (string)nullable };       // Compliant
        }

        public void Array(string nonNullable, string? nullable)
        {
            _ = new[] { new { X = (string?)nonNullable }, new { X = (string?)null } };  // Compliant
            _ = new[] { new { X = (string?)nullable }, new { X = (string?)null } };     // Noncompliant
            _ = new[] { new { X = (string?)nonNullable } };                             // Compliant
            _ = new[] { new { X = (string?)nullable } };                                // Noncompliant
            _ = new[] { new HoldsObject(new { X = (string?)nonNullable }) };            // Compliant
            _ = new[] { new HoldsObject(new { X = (string?)nullable }) };               // Noncompliant
        }

        public void SwitchExpression(string nonNullable, string? nullable)
        {
            _ = true switch
            {
                true => new { X = (string?)nonNullable },   // Compliant
                false => new { X = (string?)null }          // Compliant
            };
            _ = true switch
            {
                true => new { X = (string?)nullable },   // Noncompliant
                false => new { X = (string?)null }          // Compliant
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

    // https://github.com/SonarSource/sonar-dotnet/issues/8413
    // See also https://github.com/SonarSource/sonar-dotnet/pull/7036
    class Repro_8413
    {
        public IEnumerable<string> GetNonNullStringsDirectCast(IEnumerable<string?> strings)
        {
            return (IEnumerable<string>)strings.Where(s => s != null); // Compliant
        }

        public IEnumerable<string> GetNonNullStringsMethodCast(IEnumerable<string?> strings)
        {
            return strings.Where(s => s != null).Cast<string>(); // Compliant
        }

        public IEnumerable<string> GetNonNullStringsAsCast(IEnumerable<string?> strings)
        {
            return strings.Where(s => s != null) as IEnumerable<string>;  // Compliant
        }
    }
}
