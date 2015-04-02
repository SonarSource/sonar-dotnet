namespace Tests.Diagnostics
{
    public class TooManyLabelsInSwitch
    {
        public TooManyLabelsInSwitch(int n)
        {
            switch (n)
            {
                case 0:
                    break;
                default:
                    break;
            }

            switch (n) // Noncompliant
            {
                case 0:
                case 1:
                    break;
                default:
                    break;
            }
        }
    }
}
