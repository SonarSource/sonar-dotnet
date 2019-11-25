/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    public sealed class RequireAttributeUsageAttribute : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3993";
        private const string MessageFormat = "Specify AttributeUsage on '{0}'{1}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclaration = (ClassDeclarationSyntax)c.Node;
                var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                if (classSymbol == null ||
                    !classSymbol.DerivesFrom(KnownType.System_Attribute) ||
                    classSymbol.GetAttributes(KnownType.System_AttributeUsageAttribute).Any())
                {
                    return;
                }

                var additionalText = InheritsAttributeUsage(classSymbol)
                    ? " to improve readability, even though it inherits it from its base type"
                    : string.Empty;

                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(),
                    classSymbol.Name, additionalText));
            },
            SyntaxKind.ClassDeclaration);
        }

        private static bool InheritsAttributeUsage(INamedTypeSymbol classSymbol) =>
            classSymbol.GetSelfAndBaseTypes()
                // System.Attribute already has AttributeUsage, we don't want to report it
                .TakeWhile(t => !t.Is(KnownType.System_Attribute))
                .Any(t => t.GetAttributes(KnownType.System_AttributeUsageAttribute).Any());
    }
}
