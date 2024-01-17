using System;
using System.Threading.Tasks;
using NFluent;
using static NFluent.Check;

public class Program
{
    public void CheckThat()
    {
        That(0);     // Noncompliant {{Complete the assertion}}
    //  ^^^^
        That<int>(); // Noncompliant

        That(1).IsEqualTo(1);
    }
}
