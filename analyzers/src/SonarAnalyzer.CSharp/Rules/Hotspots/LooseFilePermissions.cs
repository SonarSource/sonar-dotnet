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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Wrappers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.Hotspots
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class LooseFilePermissions : LooseFilePermissionsBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public LooseFilePermissions() : this(AnalyzerConfiguration.Hotspot) { }

        internal LooseFilePermissions(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override void VisitAssignments(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            if (IsFileAccessPermissions(node, context.SemanticModel) && !node.IsPartOfBinaryNegationOrCondition())
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, node.GetLocation()));
            }
        }

        protected override void VisitInvocations(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if ((IsSetAccessRule(invocation, context.SemanticModel) || IsAddAccessRule(invocation, context.SemanticModel))
                && (GetObjectCreation(invocation, context.SemanticModel) is { } objectCreation))
            {
                var invocationLocation = invocation.GetLocation();
                var secondaryLocation = objectCreation.Expression.GetLocation();

                var diagnostic = invocationLocation.GetLineSpan().StartLinePosition.Line == secondaryLocation.GetLineSpan().StartLinePosition.Line
                    ? Diagnostic.Create(Rule, invocationLocation)
                    : Diagnostic.Create(Rule, invocationLocation, additionalLocations: new[] {secondaryLocation});

                context.ReportDiagnosticWhenActive(diagnostic);
            }
        }

        private static bool IsSetAccessRule(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.IsMemberAccessOnKnownType("SetAccessRule", KnownType.System_Security_AccessControl_FileSystemSecurity, semanticModel);

        private static bool IsAddAccessRule(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.IsMemberAccessOnKnownType("AddAccessRule", KnownType.System_Security_AccessControl_FileSystemSecurity, semanticModel);

        private static IObjectCreation GetObjectCreation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (GetVulnerableFileSystemAccessRule(invocation.DescendantNodes()) is { }  accessRuleSyntaxNode)
            {
                return accessRuleSyntaxNode;
            }
            else if (invocation.GetArgumentSymbolsOfKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, semanticModel).FirstOrDefault() is { } accessRuleSymbol
                && !(accessRuleSymbol is IMethodSymbol))
            {
                return GetVulnerableFileSystemAccessRule(accessRuleSymbol.GetLocationNodes(invocation));
            }
            else
            {
                return null;
            }

            IObjectCreation GetVulnerableFileSystemAccessRule(IEnumerable<SyntaxNode> nodes) =>
                FilterObjectCreations(nodes).FirstOrDefault(x => IsFileSystemAccessRuleForEveryoneWithAllow(x, semanticModel));
        }

        private static bool IsFileSystemAccessRuleForEveryoneWithAllow(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.IsKnownType(KnownType.System_Security_AccessControl_FileSystemAccessRule, semanticModel)
            && objectCreation.ArgumentList is { } argumentList
            && IsEveryone(argumentList.Arguments.First().Expression, semanticModel)
            && semanticModel.GetConstantValue(argumentList.Arguments.Last().Expression) is {HasValue: true, Value: 0};

        private static bool IsEveryone(SyntaxNode syntaxNode, SemanticModel semanticModel) =>
            semanticModel.GetConstantValue(syntaxNode) is {HasValue: true, Value: Everyone}
            || FilterObjectCreations(syntaxNode.DescendantNodesAndSelf()).Any(x => IsNTAccountWithEveryone(x, semanticModel) || IsSecurityIdentifierWithEveryone(x, semanticModel));

        private static bool IsNTAccountWithEveryone(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.IsKnownType(KnownType.System_Security_Principal_NTAccount, semanticModel)
            && objectCreation.ArgumentList is { } argumentList
            && semanticModel.GetConstantValue(argumentList.Arguments.Last().Expression) is { HasValue: true, Value: Everyone};

        private static bool IsSecurityIdentifierWithEveryone(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.IsKnownType(KnownType.System_Security_Principal_SecurityIdentifier, semanticModel)
            && objectCreation.ArgumentList is { } argumentList
            && semanticModel.GetConstantValue(argumentList.Arguments.First().Expression) is { HasValue: true, Value: 1};

        private static IEnumerable<IObjectCreation> FilterObjectCreations(IEnumerable<SyntaxNode> nodes) =>
            nodes.Where(x => x.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression))
                 .Select(x => ObjectCreationFactory.Create(x));
    }
}
