namespace Tests.Diagnostics
{
    public class BreakOutsideSwitch
    {
        public BreakOutsideSwitch(int n)
        {
            foreach (var (a, b) in new (int, int)[0])
            {
                break; // Noncompliant
            }
        }
    }
}
