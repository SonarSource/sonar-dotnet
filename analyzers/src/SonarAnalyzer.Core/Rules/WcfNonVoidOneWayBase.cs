/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class WcfNonVoidOneWayBase<TMethodSyntax, TLanguageKind> : SonarDiagnosticAnalyzer
        where TMethodSyntax : SyntaxNode
        where TLanguageKind : struct
    {
        internal const string DiagnosticId = "S3598";
        protected const string MessageFormat = "This method can't return any values because it is marked as one-way operation.";

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var methodDeclaration = (TMethodSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
                    if (methodSymbol == null ||
                        methodSymbol.ReturnsVoid)
                    {
                        return;
                    }

                    var operationContractAttribute = methodSymbol
                        .GetAttributes(KnownType.System_ServiceModel_OperationContractAttribute)
                        .FirstOrDefault();
                    if (operationContractAttribute == null)
                    {
                        return;
                    }

                    var asyncPattern = operationContractAttribute.NamedArguments
                        .FirstOrDefault(na => "AsyncPattern".Equals(na.Key, StringComparison.OrdinalIgnoreCase)) // insensitive for VB.NET
                        .Value.Value as bool?;
                    if (asyncPattern.HasValue &&
                        asyncPattern.Value)
                    {
                        return;
                    }

                    var isOneWay = operationContractAttribute.NamedArguments
                        .FirstOrDefault(na => "IsOneWay".Equals(na.Key, StringComparison.OrdinalIgnoreCase)) // insensitive for VB.NET
                        .Value.Value as bool?;
                    if (isOneWay.HasValue &&
                        isOneWay.Value)
                    {
                        c.ReportIssue(SupportedDiagnostics[0], GetReturnTypeLocation(methodDeclaration));
                    }
                },
                MethodDeclarationKind);
        }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TLanguageKind MethodDeclarationKind { get; }
        protected abstract Location GetReturnTypeLocation(TMethodSyntax method);
    }
}
