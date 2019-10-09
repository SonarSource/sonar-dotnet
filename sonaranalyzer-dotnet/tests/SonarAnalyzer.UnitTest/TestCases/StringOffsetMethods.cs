using System;

namespace Tests.Diagnostics
{
    public class StringOffsetMethods
    {
        public void StringOffsetMethodsCases()
        {
            "Test".Substring(1);

            "Test".IndexOf('t');
            "Test".IndexOf('t', 1);
            "Test".IndexOf("es");
            "Test".IndexOf("es", 1);
            "Test".Substring(1).IndexOf('t'); // Noncompliant {{A new string is going to be created by 'Substring'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            "Test".Substring(1).IndexOf("t"); // Noncompliant {{A new string is going to be created by 'Substring'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            "Test".IndexOfAny(new[] { 't' });
            "Test".IndexOfAny(new[] { 't' }, 2);
            "Test".IndexOfAny(new[] { 't' }, 1, 2);
            "Test".Substring(1).IndexOfAny(new[] { 't' }); // Noncompliant {{A new string is going to be created by 'Substring'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            "Test".LastIndexOf('t');
            "Test".LastIndexOf('t', 1);
            "Test".LastIndexOf("t");
            "Test".LastIndexOf("t", 1);
            "Test".LastIndexOf("t", 1, 3);
            "Test".Substring(1).LastIndexOf('t'); // Noncompliant {{A new string is going to be created by 'Substring'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            "Test".Substring(1).LastIndexOf("t"); // Noncompliant {{A new string is going to be created by 'Substring'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


            "Test".LastIndexOfAny(new[] { 't' });
            "Test".LastIndexOfAny(new[] { 't' }, 1);
            "Test".LastIndexOfAny(new[] { 't' }, 1, 3);
            "Test".Substring(1).LastIndexOfAny(new[] { 't' }); // Noncompliant {{A new string is going to be created by 'Substring'.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
    }
}
