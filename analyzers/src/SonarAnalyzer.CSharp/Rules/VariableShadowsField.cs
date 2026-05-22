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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VariableShadowsField : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1117";
    private const string MessageFormat = "Rename '{0}' which hides the {1} with the same name.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Process, CSharpFacade.Instance.SyntaxKind.LocalDeclarationKinds);

    private static void Process(SonarSyntaxNodeReportingContext context)
    {
        // VariableDeclarator is shared between locals and fields/events; only locals can shadow.
        if (context.Node is VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax })
        {
            return;
        }
        if (context.Node.GetIdentifier() is { } identifier)
        {
            ReportOnVariableMatchingField(context, ContextSymbols(context), identifier);
        }
    }

    private static List<ISymbol> ContextSymbols(SonarSyntaxNodeReportingContext context)
    {
        var members = context.ContainingSymbol.ContainingType.GetMembers();
        var primaryConstructorParameters = members.OfType<IMethodSymbol>().FirstOrDefault(x => x.IsPrimaryConstructor)?.Parameters;
        var fieldsAndProperties = members.Where(x => x is IPropertySymbol or IFieldSymbol).ToList();
        return primaryConstructorParameters is null ? fieldsAndProperties : fieldsAndProperties.Concat(primaryConstructorParameters).ToList();
    }

    private static void ReportOnVariableMatchingField(SonarSyntaxNodeReportingContext context, IEnumerable<ISymbol> members, SyntaxToken identifier)
    {
        if (members.FirstOrDefault(x => x.Name == identifier.ValueText
                                        && (x.IsStatic || !identifier.Parent.EnclosingScope().GetModifiers().Any(x => x.Kind() == SyntaxKind.StaticKeyword))) is { } matchingMember)
        {
            context.ReportIssue(Rule, identifier, identifier.Text, SymbolName(matchingMember));
        }
    }

    private static string SymbolName(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol => "field",
            IPropertySymbol => "property",
            IParameterSymbol => "primary constructor parameter",
            _ => string.Empty
        };
}
