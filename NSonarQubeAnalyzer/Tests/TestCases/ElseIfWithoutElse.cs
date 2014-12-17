namespace Tests.Diagnostics
{
    using System.Collections.Generic;

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
