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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WcfMissingContractAttribute : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3597";
        private const string MessageFormat = "Add the '{0}' attribute to {1}.";
        private const string MessageOperation = "the methods of this {0}";
        private const string MessageService = " this {0}";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (namedType.Is(TypeKind.Struct))
                    {
                        return;
                    }

                    var hasServiceContract = namedType.HasAttribute(KnownType.System_ServiceModel_ServiceContractAttribute);
                    var hasAnyMethodWithOperationContract = HasAnyMethodWithOperationContract(namedType);

                    if (!(hasServiceContract ^ hasAnyMethodWithOperationContract))
                    {
                        return;
                    }

                    var declarationSyntax = GetTypeDeclaration(c, namedType);
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

                    c.ReportIssue(CreateDiagnostic(Rule, declarationSyntax.Identifier.GetLocation(), attributeToAdd, message));
                },
                SymbolKind.NamedType);

        private static bool HasAnyMethodWithOperationContract(INamespaceOrTypeSymbol namedType) =>
            namedType.GetMembers()
                     .OfType<IMethodSymbol>()
                     .Any(m => m.HasAttribute(KnownType.System_ServiceModel_OperationContractAttribute));

        private static TypeDeclarationSyntax GetTypeDeclaration(SonarSymbolReportingContext context, ISymbol namedType) =>
            namedType.DeclaringSyntaxReferences
                     .Where(x => context.ShouldAnalyzeTree(x.SyntaxTree, CSharpGeneratedCodeRecognizer.Instance))
                     .Select(x => x.GetSyntax() as TypeDeclarationSyntax)
                     .FirstOrDefault(x => x is not null);
    }
}
