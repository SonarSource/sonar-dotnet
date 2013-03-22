using System;

class Program
{
    static void Main(string[] args)
    {
        const decimal grossSalary = 80000;        // Compliant
        const decimal Rate = 0.85m;               // Compliant

        decimal rate2 = 0.85m;                    // Compliant

        decimal netSalary1;
        decimal netSalary2;

        netSalary1 = Rate * grossSalary;          // Compliant
        netSalary2 = 0.85m * grossSalary;         // Non-Compliant

        netSalary1 = 0;                           // Compliant, exception
        netSalary1 = -1;                          // Compliant, exception
        netSalary1 = +1;                          // Compliant, exception
        netSalary1 = 0x0;                         // Compliant, exception
        netSalary1 = 0x00;                        // Compliant, exception
    }
}
