using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ElseIfWithoutElse
    {
        public ElseIfWithoutElse(bool a)
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
            else if (a) // Noncompliant
            {
            }


            if (a)
            {
            }
            else if (a)
            {
            }
            else if (a)
            {
            }
            else
            {
            }

            if (a)
            {
            }
            else
            {
                if (a)
                {
                }
            }
        }
    }
}
