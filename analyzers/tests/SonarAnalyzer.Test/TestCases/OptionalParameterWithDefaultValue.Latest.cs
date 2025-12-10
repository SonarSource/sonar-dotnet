using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Tests.Diagnostics
{
    public interface OptionalParameterWithDefaultValue
    {
            static virtual void Virtual([Optional][DefaultValue(4)]int i, int j = 5) // Noncompliant
            {
                Console.WriteLine(i);
            }

            static abstract void Abstract([Optional] [DefaultValue(4)] int i, int j = 5); // Noncompliant

            static abstract void Abstract2([Optional] [DefaultParameterValue(4)] int i, int j = 5); // Compliant

            static virtual void DoStuff3([Optional][DefaultParameterValue(4)]int i, int j = 5)
            {
                Console.WriteLine(i);
            }
    }
}
