/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberShouldNotHaveConflictingTransparencyAttributes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4211";
        private const string MessageFormat = "Change or remove this attribute to be consistent with its container.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
        protected override bool EnableConcurrentExecution => false;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                csac =>
                {
                    var nodesWithSecuritySafeCritical = new Dictionary<SyntaxNode, AttributeSyntax>();
                    var nodesWithSecurityCritical = new Dictionary<SyntaxNode, AttributeSyntax>();

                    csac.RegisterNodeAction(snac => CollectSecurityAttributes(snac, nodesWithSecuritySafeCritical, nodesWithSecurityCritical), SyntaxKind.Attribute);

                    csac.RegisterCompilationEndAction(cac => ReportOnConflictingTransparencyAttributes(cac, nodesWithSecuritySafeCritical, nodesWithSecurityCritical));
                });

        private static void CollectSecurityAttributes(SonarSyntaxNodeReportingContext syntaxNodeAnalysisContext,
                                                      Dictionary<SyntaxNode, AttributeSyntax> nodesWithSecuritySafeCritical,
                                                      Dictionary<SyntaxNode, AttributeSyntax> nodesWithSecurityCritical)
        {
            var attribute = (AttributeSyntax)syntaxNodeAnalysisContext.Node;
            if (!(syntaxNodeAnalysisContext.Model.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructor))
            {
                return;
            }

            if (attributeConstructor.ContainingType.Is(KnownType.System_Security_SecuritySafeCriticalAttribute))
            {
                nodesWithSecuritySafeCritical.Add(attribute.Parent.Parent, attribute);
            }
            else if (attributeConstructor.ContainingType.Is(KnownType.System_Security_SecurityCriticalAttribute))
            {
                nodesWithSecurityCritical.Add(attribute.Parent.Parent, attribute);
            }
            else
            {
                // nothing
            }
        }

        private static void ReportOnConflictingTransparencyAttributes(SonarCompilationReportingContext compilationContext,
                                                                      Dictionary<SyntaxNode, AttributeSyntax> nodesWithSecuritySafeCritical,
                                                                      Dictionary<SyntaxNode, AttributeSyntax> nodesWithSecurityCritical)
        {
            var assemblySecurityCriticalAttribute = compilationContext.Compilation.Assembly
                                                                      .GetAttributes(KnownType.System_Security_SecurityCriticalAttribute)
                                                                      .FirstOrDefault();

            if (assemblySecurityCriticalAttribute is not null)
            {
                var assemblySecurityLocation = assemblySecurityCriticalAttribute.ApplicationSyntaxReference.GetSyntax().ToSecondaryLocation();

                // All parts declaring the 'SecuritySafeCriticalAttribute' are incorrect since the assembly
                // itself is marked as 'SecurityCritical'.
                foreach (var item in nodesWithSecuritySafeCritical)
                {
                    compilationContext.ReportIssue(Rule, item.Value.GetLocation(), [assemblySecurityLocation]);
                }
            }
            else
            {
                foreach (var item in nodesWithSecuritySafeCritical)
                {
                    var current = item.Key.Parent;
                    while (current is not null)
                    {
                        if (nodesWithSecurityCritical.ContainsKey(current))
                        {
                            compilationContext.ReportIssue(Rule, item.Value.GetLocation(), [nodesWithSecurityCritical[current].ToSecondaryLocation()]);
                            break;
                        }

                        current = current.Parent;
                    }
                }
            }
        }
    }
}
