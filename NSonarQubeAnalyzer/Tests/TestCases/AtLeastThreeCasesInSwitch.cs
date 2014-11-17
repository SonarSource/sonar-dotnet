namespace Tests.Diagnostics
{
    public class AtLeastThreeCasesInSwitch
    {
        public AtLeastThreeCasesInSwitch(int n)
        {
            switch (a) // Noncompliant
            {
                case 0:
                    break;
                default:
                    break;
            }

            switch (n) // Noncompliant
            {
            }

            switch (a)
            { 
                case 0:
                    break;
                case 1:
                default:
                    break;
            }
        }
    }
}
