namespace Tests.Diagnostics
{
    public class AtLeastThreeCasesInSwitch
    {
        public AtLeastThreeCasesInSwitch(int n)
        {
            switch (n) // Noncompliant
            {
                case 0:
                    break;
                default:
                    break;
            }

            switch (n) // Noncompliant
            {
            }

            switch (n)
            { 
                case 0:
                    break;
                case 1:
                default:
                    var x=5;
                    break;
            }
        }
    }
}
