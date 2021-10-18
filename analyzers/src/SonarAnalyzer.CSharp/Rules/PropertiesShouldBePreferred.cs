/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class PropertiesShouldBePreferred : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4049";
        private const string MessageFormat = "Consider making method '{0}' a property.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (c.ContainingSymbol.Kind != SymbolKind.NamedType
                        || !(c.SemanticModel.GetDeclaredSymbol(c.Node) is INamedTypeSymbol typeSymbol))
                    {
                        return;
                    }

                    var propertyCandidates = typeSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(HasCandidateName)
                        .Where(HasCandidateReturnType)
                        .Where(HasCandidateSignature)
                        .Where(UsageAttributesAllowProperties);

                    foreach (var candidate in propertyCandidates)
                    {
                        c.ReportIssue(Diagnostic.Create(
                            Rule,
                            candidate.Locations.FirstOrDefault(),
                            messageArgs: candidate.Name));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private static bool HasCandidateSignature(IMethodSymbol method) =>
            method.IsPubliclyAccessible()
            && method.Parameters.Length == 0
            && !method.IsConstructor()
            && !method.IsOverride
            && method.MethodKind != MethodKind.PropertyGet
            && method.GetInterfaceMember() is null;

        private static bool HasCandidateReturnType(IMethodSymbol method) =>
            !method.ReturnsVoid
            && !method.IsAsync
            && !method.ReturnType.Is(TypeKind.Array)
            && !method.ReturnType.OriginalDefinition.IsAny(KnownType.SystemTasks);

        private static bool HasCandidateName(IMethodSymbol method)
        {
            if (method.Name == "GetEnumerator" || method.Name == "GetAwaiter")
            {
                return false;
            }
            var nameParts = method.Name.SplitCamelCaseToWords().ToList();
            return nameParts.Count > 1 && nameParts[0] == "GET";
        }

        private static bool UsageAttributesAllowProperties(IMethodSymbol method) =>
            method.GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .SelectMany(cls => cls.GetAttributes(KnownType.System_AttributeUsageAttribute))
                .Select(attr => attr.ConstructorArguments[0].Value)
                .Cast<AttributeTargets>()
                .All(targets => targets.HasFlag(AttributeTargets.Property));
    }
}
