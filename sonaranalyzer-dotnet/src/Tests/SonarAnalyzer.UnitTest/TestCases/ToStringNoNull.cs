using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ToStringNoNull
    {
        public string AnyOther()
        {
            return null;
        }

        public string ToString()
        {
            if (this.collection.Count == 0)
            {
                return null; // Noncompliant
//              ^^^^^^^^^^^^
            }
            else
            {
                // ...
            }

            return null; // Noncompliant {{Return empty string instead.}}
        }
    }

    public class ToStringNoNull2
    {
        public override string ToString()
        {
            if (this.collection.Count == 0)
            {
                return "";
            }
            else
            {
                // ...
            }
            return "";
        }
    }
}
