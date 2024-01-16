using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    public class IfChainWithoutElse
    {
        public IfChainWithoutElse(bool a)
        {
            if (a)
            {
            }

            if (a)
            {
            }
            else
            {
            }

            if (a)
            {
            }
            else if (a)
            {
            }
            else if (a) // Noncompliant {{Add the missing 'else' clause with either the appropriate action or a suitable comment as to why no action is taken.}}
//          ^^^^^^^
            {
            }


            if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) // Noncompliant {{Add the missing 'else' clause with either the appropriate action or a suitable comment as to why no action is taken.}}
//          ^^^^^^^
            {
            }

            if (a) { }
            else if (a) { }
            else if (a) { } // Noncompliant
            else { }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { } // Noncompliant
            else { }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else
            {
                Console.WriteLine();
            }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else
            {
                // Single line comment
            }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else
            {
                /* Multi line comment
                 * Which is actually multi line
                 */
            }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else
                Console.WriteLine();

            if (a) { }
            else if (a) { }
            else if (a) { }
            else
            {
#if DEBUG
            Trace.WriteLine("Something to log only in debug", a.ToString());
#endif
            }

            if (a) { }
            else if (a) { }
            else if (a) { } // Noncompliant
            else
            {
#if DEBUG
#endif
            }

            if (a)
            {
            }
            else
            {
                if (a)
                {
                    if (a)
                    {
                    }
                    else if (a) // Noncompliant
                    {
                    }
                }
            }
        }
    }
}
