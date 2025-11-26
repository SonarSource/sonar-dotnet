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

    public void NullConditionalAssignment(Wrapper wrapper)
    {
        wrapper?.Result = Check.That(1); // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2671
        wrapper.Result = Check.That(1);  // Compliant
    }

    public class Wrapper
    {
        public ICheck<int> Result;
    }
}
