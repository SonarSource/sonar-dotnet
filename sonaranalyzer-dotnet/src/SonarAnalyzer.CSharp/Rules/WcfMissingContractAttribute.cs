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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class WcfMissingContractAttribute : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3597";
        private const string MessageFormat = "Add the '{0}' attribute to {1}.";
        internal const string MessageOperation = "the methods of this {0}";
        internal const string MessageService = " this {0}";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (namedType.Is(TypeKind.Struct))
                    {
                        return;
                    }

                    var hasServiceContract = namedType.GetAttributes(KnownType.System_ServiceModel_ServiceContractAttribute).Any();
                    var hasAnyMethodWithOperationContract = HasAnyMethodWithoperationContract(namedType);

                    if (!(hasServiceContract ^ hasAnyMethodWithOperationContract))
                    {
                        return;
                    }

                    var declarationSyntax = GetTypeDeclaration(namedType, c.Compilation, c.Options);
                    if (declarationSyntax == null)
                    {
                        return;
                    }

                    string message;
                    string attributeToAdd;

                    if (hasServiceContract)
                    {
                        message = MessageOperation;
                        attributeToAdd = "OperationContract";
                    }
                    else
                    {
                        message = MessageService;
                        attributeToAdd = "ServiceContract";
                    }

                    var classOrInterface = namedType.IsClass() ? "class" : "interface";
                    message = string.Format(message, classOrInterface);

                    c.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule,
                        declarationSyntax.Identifier.GetLocation(), attributeToAdd, message));
                },
                SymbolKind.NamedType);
        }

        private static bool HasAnyMethodWithoperationContract(INamedTypeSymbol namedType)
        {
            return namedType.GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m => m.GetAttributes(KnownType.System_ServiceModel_OperationContractAttribute).Any());
        }

        private static TypeDeclarationSyntax GetTypeDeclaration(INamedTypeSymbol namedType, Compilation compilation,
            AnalyzerOptions options)
        {
            return namedType.DeclaringSyntaxReferences
                .Where(sr => sr.SyntaxTree.ShouldAnalyze(options, compilation))
                .Select(sr => sr.GetSyntax() as TypeDeclarationSyntax)
                .FirstOrDefault(s => s != null);
        }
    }
}
