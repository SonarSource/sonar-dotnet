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

using System.Collections;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PropertiesShouldBePreferred : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4049";
        private const string MessageFormat = "Consider making method '{0}' a property.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.IsRedundantPositionalRecordContext()
                        || c.SemanticModel.GetDeclaredSymbol(c.Node) is not INamedTypeSymbol typeSymbol)
                    {
                        return;
                    }

                    var propertyCandidates = typeSymbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(x => HasCandidateName(x) && HasCandidateReturnType(x) && HasCandidateSignature(x) && UsageAttributesAllowProperties(x));

                    foreach (var candidate in propertyCandidates)
                    {
                        c.ReportIssue(CreateDiagnostic(
                            Rule,
                            candidate.Locations.FirstOrDefault(),
                            messageArgs: candidate.Name));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

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
            if (method.Name is nameof(IEnumerable.GetEnumerator) or nameof(Task.GetAwaiter))
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
