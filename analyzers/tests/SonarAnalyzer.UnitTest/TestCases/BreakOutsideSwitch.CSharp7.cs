namespace Tests.Diagnostics
{
    public class BreakOutsideSwitch
    {
        public BreakOutsideSwitch(int n)
        {
            switch (true)
            {
                case true:
                    foreach (var (a, b) in new (int, int)[0])
                    {
                        break; // Noncompliant
                    }
                    break;
            }
        }
    }
}
