namespace Tests.Diagnostics
{
    public class SwitchCasesMinimumThree
    {
        public SwitchCasesMinimumThree(int n)
        {
            switch (n) // Noncompliant
//          ^^^^^^
            {
                case 0:
                    break;
                default:
                    break;
            }

            switch (n) // Noncompliant {{Replace this 'switch' statement with 'if' statements to increase readability.}}
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
