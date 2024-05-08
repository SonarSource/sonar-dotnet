using System;
using System.Linq;

public class Sample
{
    public void SimpleLambdas()
    {
        Method(x => 42);
        Method(x => { });

        Method(item => 42);     // Noncompliant {{Use 'x' for the lambda parameter name.}}
        Method(item => { });    // Noncompliant
        //     ^^^^

        Method(a => 42);        // Noncompliant
        Method(b => 42);        // Noncompliant
        Method(y => 42);        // Noncompliant
        Method(z => 42);        // Noncompliant
        Method(_ => 42);        // Compliant
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
        Method((_, _) => 42);
        Method((a, b) => { });
    }

    public void Errors()
    {
        // Error@+5 [CS1001] Identifier expected
        // Error@+4 [CS1003] Syntax error, '=>' expected
        // Error@+3 [CS1026] ) expected
        // Error@+2 [CS1593] Delegate 'Func<int, int>' does not take 0 arguments
        // Error@+1 [CS1003] Syntax error, ',' expected
        Func<int, int> f = ( => 42);

        // Error@+2 [CS1001] Identifier expected
        // Error@+1 [CS1503] Argument 1: cannot convert from 'int' to 'System.Func<int>'
        Method( => 42);

        // Error@+5 [CS1501] No overload for method 'Method' takes 0 arguments
        // Error@+4 [CS1001] Identifier expected
        // Error@+3 [CS1002] ; expected
        // Error@+2 [CS1026] ) expected
        // Error@+1 [CS1513] Closing curly brace expected
        Method( => { });
    }

    private void Method(Func<int> f) { }
    private void Method(Func<int, int> f) { }
    private void Method(Func<int, int, int> f) { }

    private void Method(Action a) { }
    private void Method(Action<int> a) { }
    private void Method(Action<int, int> a) { }
}

public class RuleRegistration
{
    public void Initialize()
    {
        RegisterSonarWhateverAnalysisContext(c => { });
        RegisterSonarWhateverAnalysisContext(context => { });
        RegisterSonarWhateverAnalysisContext(whateverContext => { });
        RegisterSonarWhateverReportingContext(c => { });
        RegisterSonarSometing(c => { });                // Noncompliant, wrong suffix
        RegisterSometingAnalysisContext(c => { });      // Noncompliant, wrong prefix
        RegisterSometingReportingContext(c => { });     // Noncompliant, wrong prefix
        RegisterSonarSometingContext(c => { });         // Noncompliant, wrong suffix
    }

    protected void RegisterSonarWhateverAnalysisContext(Action<SonarWhateverAnalysisContext> action) { }
    protected void RegisterSonarWhateverReportingContext(Action<SonarWhateverReportingContext> action) { }
    protected void RegisterSonarSometing(Action<SonarSometing> action) { }
    protected void RegisterSometingAnalysisContext(Action<SometingAnalysisContext> action) { }
    protected void RegisterSometingReportingContext(Action<SometingReportingContext> action) { }
    protected void RegisterSonarSometingContext(Action<SonarSometingContext> action) { }

    // Well-known expected classes patterns
    public class SonarWhateverAnalysisContext { }
    public class SonarWhateverReportingContext { }
    // Unexpected types
    public class SonarSometing { }
    public class SometingAnalysisContext { }
    public class SometingReportingContext { }
    public class SonarSometingContext { }
}
