/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

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

                    c.ReportIssue(Rule, declarationSyntax.Identifier, attributeToAdd, message);
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
