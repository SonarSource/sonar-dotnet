namespace Tests.Diagnostics
{
    public class SwitchWithoutDefaultTestCase
    {
        public SwitchWithoutDefaultTestCase(int n)
        {
            switch (n) // Noncompliant
            {
            }

            switch (n) // Noncompliant
            {
                case 1:
                case 2:
                    break;
            }

            switch (n)
            {
                default:
                    break;
            }

            switch (n)
            {
                case 1:
                    break;
                default:
                    break;
            }

            switch (n)
            {
                case 1:
                default:
                case 2:
                    break;
                case 3:
                    break;
            }
        }
    }
}
