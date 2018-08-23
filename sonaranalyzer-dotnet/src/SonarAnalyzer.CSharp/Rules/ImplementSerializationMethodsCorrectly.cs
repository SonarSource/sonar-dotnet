/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ImplementSerializationMethodsCorrectly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3927";
        private const string MessageFormat = "Make this method {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private const string problemMakePrivateText = "'private'";
        private const string problemReturnVoidText = "return 'void'";
        private const string problemParameterText = "have a single parameter of type 'StreamingContext'";
        private const string problemGenericParameterText = "have no type parameters";

        private static readonly ISet<KnownType> serializationAttributes = new HashSet<KnownType>
        {
            KnownType.System_Runtime_Serialization_OnSerializingAttribute,
            KnownType.System_Runtime_Serialization_OnSerializedAttribute,
            KnownType.System_Runtime_Serialization_OnDeserializingAttribute,
            KnownType.System_Runtime_Serialization_OnDeserializedAttribute
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                if (methodDeclaration.Identifier.IsMissing ||
                    !methodSymbol.GetAttributes(serializationAttributes).Any())
                {
                    return;
                }

                var issues = FindIssues(methodSymbol).ToList();
                if (issues.Count > 0)
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                        methodDeclaration.Identifier.GetLocation(),
                        BuildErrorMessage(issues)));
                }
            },
            SyntaxKind.MethodDeclaration);
        }

        private static IEnumerable<string> FindIssues(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.DeclaredAccessibility != Accessibility.Private)
            {
                yield return problemMakePrivateText;
            }

            if (!methodSymbol.ReturnsVoid)
            {
                yield return problemReturnVoidText;
            }

            if (!methodSymbol.TypeParameters.IsEmpty)
            {
                yield return problemGenericParameterText;
            }

            if (methodSymbol.Parameters.Length != 1 ||
                !methodSymbol.Parameters.First().IsType(KnownType.System_Runtime_Serialization_StreamingContext))
            {
                yield return problemParameterText;
            }
        }

        private static string BuildErrorMessage(List<string> issues)
        {
            if (issues.Count == 1)
            {
                return issues.First();
            }

            const string separator = ", ";
            return string.Format("{0} and {1}",
                string.Join(separator, issues.Take(issues.Count - 1)),
                issues.Last());
        }
    }
}
