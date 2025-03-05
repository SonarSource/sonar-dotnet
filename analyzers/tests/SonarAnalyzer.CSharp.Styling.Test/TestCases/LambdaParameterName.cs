using System;
using System.Collections.Generic;
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
        RegisterSonarSomething(c => { });                // Noncompliant, wrong suffix
        RegisterSomethingAnalysisContext(c => { });
        RegisterSomethingReportingContext(c => { });     // Noncompliant, wrong prefix
        RegisterSonarSomethingContext(c => { });         // Noncompliant, wrong suffix
    }

    protected void RegisterSonarWhateverAnalysisContext(Action<SonarWhateverAnalysisContext> action) { }
    protected void RegisterSonarWhateverReportingContext(Action<SonarWhateverReportingContext> action) { }
    protected void RegisterSonarSomething(Action<SonarSomething> action) { }
    protected void RegisterSomethingAnalysisContext(Action<SomethingAnalysisContext> action) { }
    protected void RegisterSomethingReportingContext(Action<SomethingReportingContext> action) { }
    protected void RegisterSonarSomethingContext(Action<SonarSomethingContext> action) { }

    // Well-known expected classes patterns
    public class SonarWhateverAnalysisContext { }
    public class SonarWhateverReportingContext { }
    public class SomethingAnalysisContext { }
    // Unexpected types
    public class SonarSomething { }
    public class SomethingReportingContext { }
    public class SonarSomethingContext { }
}

public class CustomDelegates
{
    public delegate void ParameterNamedI(int i);
    public delegate void ParameterNamedTest(int test);
    public delegate void ParameterNamedCamelCasing(int camelCasing);

    public void Test()
    {
        ParameterNamedI delegate1 = i => { }; // Compliant "i" matches the parameter name of the delegate
        ParameterNamedI delegate2 = j => { }; // Noncompliant
        ParameterNamedI delegate3 = x => { }; // Compliant

        ParameterNamedTest delegate4 = test => { };     // Compliant
        ParameterNamedTest delegate5 = someTest => { }; // Noncompliant
        ParameterNamedTest delegate6 = testSome => { }; // Noncompliant

        ParameterNamedCamelCasing delegate7 = camelCasing => { }; // Compliant
        ParameterNamedCamelCasing delegate8 = camelcasing => { }; // Noncompliant
        ParameterNamedCamelCasing delegate9 = camel => { };     // Noncompliant
        ParameterNamedCamelCasing delegate10 = casing => { };    // Noncompliant

        Func<int, int> function = arg => 0;  // Noncompliant, the delegate parameter is named "arg" (https://learn.microsoft.com/en-us/dotnet/api/system.func-2) but we do not allow that for Func<T, TResult>
        Action<int> action = obj => { };     // Noncompliant, the delegate parameter is named "obj" (https://learn.microsoft.com/en-us/dotnet/api/system.action-1) but we do not allow that for Action<T>
        new List<int>().Exists(obj => true); // Compliant. List.Exists uses System.Predicate<T> instead of System.Func<T, bool>
    }
}
