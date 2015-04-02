namespace Tests.Diagnostics
{
    public class IfConditionalAlwaysTrueOrFalse
    {
        public IfConditionalAlwaysTrueOrFalse(int a, int b)
        {
            if (a == b)
            {
            }

            if (true == b)
            {
            }

            if (a)
            {
            }

            if (true) // Noncompliant
            {
            }

            if (false) // Noncompliant
            {
            }
        }
    }
}
