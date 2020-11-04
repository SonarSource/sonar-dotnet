using System.Collections.Generic;
using System.Linq;


List<string> LocalFunction() => null; // Noncompliant {{Return an empty collection instead of null.}}
List<string> LocalFunctionNew() => new(); // Compliant

static IEnumerable<string> StaticLocalFunction() => (null); // Noncompliant

record Record
{
    IEnumerable<string> Property => null; // Noncompliant

    IEnumerable<char> Method(string str)
    {
        if (str == null)
        {
            return null; // Noncompliant
        }

        return str.ToCharArray();
    }

    IEnumerable<int> Compliant() => Enumerable.Empty<int>();
}
