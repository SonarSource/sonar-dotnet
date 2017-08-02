/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public sealed class ExceptionConstructorShouldNotThrow : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3693";
        private const string MessageFormat = "Avoid throwing exceptions in this constructor.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;

                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null ||
                        !classSymbol.DerivesFrom(KnownType.System_Exception))
                    {
                        return;
                    }

                    var throwStatementsPerCtor = classDeclaration.Members
                        .OfType<ConstructorDeclarationSyntax>()
                        .Select(ctor => ctor.DescendantNodes().OfType<ThrowStatementSyntax>().ToList())
                        .Where(@throw => @throw.Count > 0)
                        .ToList();

                    foreach (var throwStatement in throwStatementsPerCtor)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, throwStatement.First().GetLocation(),
                            throwStatement.Skip(1).Select(@throw => @throw.GetLocation())));
                    }
                },
                SyntaxKind.ClassDeclaration);
        }
    }
}
