/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OptionalRefOutParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3447";
        private const string MessageFormat = "Remove the 'Optional' attribute, it cannot be used with '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var parameter = (ParameterSyntax)c.Node;
                    if (!parameter.AttributeLists.Any()
                        || !parameter.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword)))
                    {
                        return;
                    }

                    var optionalAttribute = AttributeSyntaxSymbolMapping.GetAttributesForParameter(parameter, c.SemanticModel)
                        .FirstOrDefault(a => a.Symbol.IsInType(KnownType.System_Runtime_InteropServices_OptionalAttribute));

                    if (optionalAttribute != null)
                    {
                        var refKind = parameter.Modifiers.Any(SyntaxKind.OutKeyword) ? "out" : "ref";
                        c.ReportIssue(CreateDiagnostic(Rule, optionalAttribute.SyntaxNode.GetLocation(), refKind));
                    }
                },
                SyntaxKind.Parameter);
    }
}
