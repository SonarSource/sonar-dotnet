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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class WcfNonVoidOneWayBase<TMethodSyntax, TLanguageKind> : SonarDiagnosticAnalyzer
        where TMethodSyntax : SyntaxNode
        where TLanguageKind : struct
    {
        internal const string DiagnosticId = "S3598";
        protected const string MessageFormat = "This method can't return any values because it is marked as one-way operation.";

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], GetReturnTypeLocation(methodDeclaration)));
                    }
                },
                MethodDeclarationKind);
        }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TLanguageKind MethodDeclarationKind { get; }
        protected abstract Location GetReturnTypeLocation(TMethodSyntax method);
    }
}
