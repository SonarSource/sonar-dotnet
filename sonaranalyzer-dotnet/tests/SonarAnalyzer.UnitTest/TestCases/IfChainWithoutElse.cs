using System.Collections.Generic;

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
            else if (a) // Noncompliant {{Add the missing 'else' clause.}}
//          ^^^^^^^
            {
            }


            if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) // Noncompliant {{Add the missing 'else' clause.}}
//          ^^^^^^^
            {
            }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else { }

            if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else if (a) { }
            else { }

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
