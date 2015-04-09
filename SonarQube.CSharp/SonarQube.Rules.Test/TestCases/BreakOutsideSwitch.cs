using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class BreakOutsideSwitch
    {
        public BreakOutsideSwitch(int n)
        {
            while (true)
            {
                if (n == 10)
                {
                    break; // Noncompliant
                }

                n++;
                break; // Noncompliant
            }

            while (n != 10)
            {
                n++;
            }

            switch (n)
            {
                case 0:
                    break;
                case 1:
                    do
                    {
                        break; // Noncompliant
                    }
                    while (true);

                    break;
            }

            foreach (var e in new List<int>())
            {
                break; // Noncompliant
            }
        }
    }
}
