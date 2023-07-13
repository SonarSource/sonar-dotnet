namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

public sealed class InitializationVectorShouldBeRandom : InitializationVectorShouldBeRandomBase
{
    public static readonly DiagnosticDescriptor S3329 = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    protected override DiagnosticDescriptor Rule => S3329;

    public override bool ShouldExecute() => false;
}
