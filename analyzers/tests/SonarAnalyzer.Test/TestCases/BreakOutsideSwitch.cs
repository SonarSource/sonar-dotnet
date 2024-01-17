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
                    break; // Noncompliant {{Refactor the code in order to remove this break statement.}}
//                  ^^^^^^
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
                case 2:
                    foreach (var (a, b) in new (int, int)[0])
                    {
                        break; // Noncompliant
                    }

                    break;
            }

            foreach (var e in new List<int>())
            {
                break; // Noncompliant
            }
        }
    }
}
