using System;

namespace Tests.Diagnostics
{
    public class StringOffsetMethods
    {
        public StringOffsetMethods()
        {
            "Test".Substring(1).IndexOf('t'); // Noncompliant {{Replace 'IndexOf' with the overload that accepts a startIndex parameter.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        public int GetIndex =>
            "Test".Substring(1).IndexOf('t'); // Noncompliant

        public void StringOffsetMethodsCases(string x)
        {
            "Test".Substring(1);

            "Test".IndexOf('t');
            "Test".IndexOf('t', 1);
            "Test".IndexOf("es");
            "Test".IndexOf("es", 1);
            "Test".IndexOf("es", 1, StringComparison.InvariantCulture);
            "Test".Substring(1).IndexOf('t'); // Noncompliant
            "Test".Substring(1).IndexOf("t"); // Noncompliant
            "Test".Substring(1).IndexOf("t", 1); // Noncompliant
            "Test".Substring(1).IndexOf("t", StringComparison.InvariantCulture); // Noncompliant
            "Test".Substring(1).IndexOf('t', 1, 3); // Noncompliant
            "Test".Substring(1).IndexOf("t", 1, 3); // Noncompliant
            "Test".Substring(1).IndexOf("t", 1, StringComparison.CurrentCulture); // Noncompliant
            "Test".Substring(1).IndexOf("t", 1, 3, StringComparison.CurrentCulture); // Noncompliant

            "Test".IndexOfAny(new[] { 't' });
            "Test".IndexOfAny(new[] { 't' }, 2);
            "Test".IndexOfAny(new[] { 't' }, 1, 2);
            "Test".Substring(1).IndexOfAny(new[] { 't' }); // Noncompliant {{Replace 'IndexOfAny' with the overload that accepts a startIndex parameter.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            "Test".LastIndexOf('t');
            "Test".LastIndexOf('t', 1);
            "Test".LastIndexOf("t");
            "Test".LastIndexOf("t", 1);
            "Test".LastIndexOf("t", 1, 3);
            "Test".Substring(1).LastIndexOf('t'); // Noncompliant {{Replace 'LastIndexOf' with the overload that accepts a startIndex parameter.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            "Test".Substring(1).LastIndexOf("t"); // Noncompliant


            "Test".LastIndexOfAny(new[] { 't' });
            "Test".LastIndexOfAny(new[] { 't' }, 1);
            "Test".LastIndexOfAny(new[] { 't' }, 1, 3);
            "Test".Substring(1).LastIndexOfAny(new[] { 't' }); // Noncompliant {{Replace 'LastIndexOfAny' with the overload that accepts a startIndex parameter.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            x.Substring(1).IndexOf('t'); // Noncompliant

            x.Substring(1, 3);
            x.Substring(1).Remove(1).IndexOf('t');
            x.Remove(1).IndexOf('t');

            Func<char, int> getIndex = c => {
                return "Test".Substring(1).IndexOf(c); // Noncompliant
            };
        }
    }
}
