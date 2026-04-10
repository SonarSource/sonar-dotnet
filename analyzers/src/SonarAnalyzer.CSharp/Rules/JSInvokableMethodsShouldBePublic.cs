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
public sealed class JSInvokableMethodsShouldBePublic : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6798";
    private const string MessageFormat = "Methods marked as 'JSInvokable' should be 'public'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
        {
            if (c.Compilation.GetTypeByMetadataName(KnownType.Microsoft_JSInterop_JSInvokable) is not null)
            {
                c.RegisterNodeAction(CheckMethod, SyntaxKind.MethodDeclaration);
            }
        });

    private static void CheckMethod(SonarSyntaxNodeReportingContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
        if (!method.Modifiers.AnyOfKind(SyntaxKind.PublicKeyword)
            && method.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.IsKnownType(KnownType.Microsoft_JSInterop_JSInvokable, context.Model)))
        {
            context.ReportIssue(Rule, method.Identifier);
        }
    }
}
