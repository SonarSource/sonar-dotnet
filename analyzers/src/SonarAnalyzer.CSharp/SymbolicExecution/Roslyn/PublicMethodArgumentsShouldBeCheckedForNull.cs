namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class PublicMethodArgumentsShouldBeCheckedForNull : PublicMethodArgumentsShouldBeCheckedForNullBase
{
    internal static readonly DiagnosticDescriptor S3900 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3900;

    public override bool ShouldExecute()
    {
        // Should return true if:
        // ✔ method is public, protected or protected internal
        // ✔ method body is not empty
        // - method body is not a single "throw NotImplementedException();"
        // - has parameters with reference type (or nullable value type?)
        // - and at least one of those parameters are dereferenced
        // - parameters don't have attributes which guard against null
        // - nullability is not enabled
        var method = (MethodDeclarationSyntax)Node;

        return
            MethodIsAccesibleFromOtherAssemblies(method)
         && MethodHasBody(method);

        static bool MethodIsAccesibleFromOtherAssemblies(MethodDeclarationSyntax method) =>
            method.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword))
            || (method.Modifiers.Any(x => x.IsKind(SyntaxKind.ProtectedKeyword)) && !method.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword)));

        static bool MethodHasBody(MethodDeclarationSyntax method) =>
            method.Body != null || method.ExpressionBody != null;
    }
}
