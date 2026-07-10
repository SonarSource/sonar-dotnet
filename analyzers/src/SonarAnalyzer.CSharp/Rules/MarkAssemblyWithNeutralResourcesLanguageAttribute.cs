/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Runtime.CompilerServices;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkAssemblyWithNeutralResourcesLanguageAttribute : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4026";
    private const string MessageFormat = "Provide a 'System.Resources.NeutralResourcesLanguageAttribute' attribute for assembly '{0}'.";
    private const string StronglyTypedResourceBuilder = "System.Resources.Tools.StronglyTypedResourceBuilder";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isCompilationEnd: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
            {
                // Strongbox: Make explicit that the captured bool is heap allocated object and any reference to it points to the same value.
                // This avoids problems with value type copy semantic e.g. when passing the bool as a parameter
                var hasResx = new StrongBox<bool>(false);
                c.RegisterNodeActionInAllFiles(cc =>
                    {
                        // Volatile.Read and Volatile.Write ensure that the Jit and the CPU do not hoist hasResX into a register, but read from memory so any writes by another thread are observed.
                        // Don't use short circuit || here. It can cause a lost true write when two threads both read false but one thread returns true and the other overrides it with false.
                        // Locking isn't needed here, we just need to make sure that any true result is not lost and that it is observable by other threads shortly after the write.
                        if (!Volatile.Read(ref hasResx.Value) && IsResxGeneratedFile(cc.Model, (ClassDeclarationSyntax)cc.Node))
                        {
                            Volatile.Write(ref hasResx.Value, true);
                        }
                    },
                    SyntaxKind.ClassDeclaration);

                c.RegisterCompilationEndAction(cc =>
                    {
                        if (hasResx.Value && !HasNeutralResourcesLanguageAttribute(cc.Compilation.Assembly))
                        {
                            cc.ReportIssue(Rule, (Location)null, cc.Compilation.AssemblyName);
                        }
                    });
            });

    private static bool IsDesignerFile(SyntaxTree tree) =>
        tree.FilePath?.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) == true;

    private static bool HasGeneratedCodeAttributeWithStronglyTypedResourceBuilderValue(SemanticModel model, ClassDeclarationSyntax classSyntax) =>
        classSyntax.AttributeLists
            .GetAttributes(KnownType.System_CodeDom_Compiler_GeneratedCodeAttribute, model)
            .Where(x => x.ArgumentList.Arguments.Count > 0)
            .Select(x => model.GetConstantValue(x.ArgumentList.Arguments[0].Expression))
            .Any(x => string.Equals(x.Value as string, StronglyTypedResourceBuilder, StringComparison.OrdinalIgnoreCase));

    private static bool IsResxGeneratedFile(SemanticModel model, ClassDeclarationSyntax classSyntax) =>
        IsDesignerFile(model.SyntaxTree) && HasGeneratedCodeAttributeWithStronglyTypedResourceBuilderValue(model, classSyntax);

    private static bool HasNeutralResourcesLanguageAttribute(IAssemblySymbol assemblySymbol) =>
        assemblySymbol.GetAttributes(KnownType.System_Resources_NeutralResourcesLanguageAttribute)
            .Any(x => x.ConstructorArguments.Any(arg => arg.Type.Is(KnownType.System_String) && !string.IsNullOrWhiteSpace((string)arg.Value)));
}
