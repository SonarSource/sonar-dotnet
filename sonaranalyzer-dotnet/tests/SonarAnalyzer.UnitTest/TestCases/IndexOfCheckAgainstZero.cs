using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class IndexOfCheckAgainstZero
    {
        public IndexOfCheckAgainstZero()
        {
            string color = "blue";
            string name = "ishmael";

            List<string> strings = new List<string>();
            System.Collections.IList stringIList = new List<string>();
            strings.add(color);
            strings.add(name);
            string[] stringArray = strings.ToArray();

            if (stringIList.IndexOf(color) > 0) // Noncompliant
//                                         ^^^
            {
                // ...
            }
            if (strings.IndexOf(color) > 0) // Noncompliant {{0 is a valid index, but this check ignores it.}}
            {
                // ...
            }
            if (0 < name.IndexOf("ish")) // Noncompliant
            {
                // ...
            }
            if (-1 < name.IndexOf("ish"))
            {
                // ...
            }
            if (2 < name.IndexOf("ish"))
            {
                // ...
            }
            if (name.IndexOf("ae") > 0) // Noncompliant
            {
                // ...
            }
            if (Array.IndexOf(stringArray, color) > 0) // Noncompliant
            {
                // ...
            }
            if (Array.IndexOf(stringArray, color) >= 0)
            {
                // ...
            }
        }
    }
}
