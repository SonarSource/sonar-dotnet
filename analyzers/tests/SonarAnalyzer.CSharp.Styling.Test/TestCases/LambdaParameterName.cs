using System;
using System.Linq;

public class Sample
{
    public void SimpleLambdas()
    {
        Method(x => 42);
        Method(_ => 42);
        Method(x => { });
        Method(_ => { });

        Method(item => 42);     // Noncompliant {{Use 'x' for the lambda parameter name.}}
        Method(item => { });    // Noncompliant
        //     ^^^^

        Method(a => 42);        // Noncompliant
        Method(b => 42);        // Noncompliant
        Method(y => 42);        // Noncompliant
        Method(z => 42);        // Noncompliant
        Method(node => 42);     // Noncompliant
        Method(dataGridViewCellContextMenuStripNeededEventArgs => 42);     // Noncompliant


        Method(x => Enumerable.Range(x, 10).Where(item => item == 42)); // Compliant, nested
        Method(item => Enumerable.Range(item, 10).Where(x => x == 42)); // Noncompliant, the outer one should be x
        //     ^^^^
    }

    public void ParenthesizedLambdas()
    {
        Method(() => 42);
        Method(() => { });
        Method((a, b) => 42);
        Method((a, b) => { });
    }

    private void Method(Func<int> f) { }
    private void Method(Func<int, int> f) { }
    private void Method(Func<int, int, int> f) { }

    private void Method(Action a) { }
    private void Method(Action<int> a) { }
    private void Method(Action<int, int> a) { }
}
