var calc = new Calc(x => x);
calc.BeginInvoke(1, null, null); // Noncompliant

public delegate int Calc(int x);

public class Paired
{
    public int Method()
    {
        var calc = new Calc(x => x);
        return calc.EndInvoke(calc.BeginInvoke(1, null, null)); // Compliant
    }
}
