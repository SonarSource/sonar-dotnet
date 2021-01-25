using System;
using System.Linq;
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
            strings.Add(color);
            strings.Add(name);
            string[] stringArray = strings.ToArray();
            char[] chars = { 'i', 'l' };

            if (stringIList.IndexOf(color) > 0) // Noncompliant
//                                         ^^^
            {
                // ...
            }

            if (stringIList.IndexOf(color) > -0) // Noncompliant
//                                         ^^^^
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

            if (0 > 0)
            {

            }

            if (strings.Count() > 0)
            {

            }

            if (name.IndexOfAny(chars) > 0) // Noncompliant
            {

            }

            if (0 < name.IndexOfAny(chars)) // Noncompliant
            {

            }

            if (name.LastIndexOf('a') > 0) // Noncompliant
            {

            }

            if (0 < name.LastIndexOf('a')) // Noncompliant
            {

            }

            if (name.LastIndexOfAny(chars) > 0) // Noncompliant
            {

            }

            if (0 < name.LastIndexOfAny(chars)) // Noncompliant
            {

            }
        }
    }
}
