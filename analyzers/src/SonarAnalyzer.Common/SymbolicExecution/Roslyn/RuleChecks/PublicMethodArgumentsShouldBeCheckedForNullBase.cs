namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class PublicMethodArgumentsShouldBeCheckedForNullBase : SymbolicRuleCheck
{
    internal const string DiagnosticId = "S3900";
    protected const string MessageFormat = "Add a null check for parameter '{0}'.";
}
