using System;

var calc = new Calc(x => x);
var result = calc.BeginInvoke(1, null, null); // Compliant
calc.EndInvoke(result);

public delegate int Calc(int x);

public class Unpaired
{
    public void Method()
    {
        var calc = new Calc(x => x);
        calc.BeginInvoke(1, null, null); // Noncompliant
    }
}
