/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.XXE
{
    internal class XmlReaderSettingsValidator
    {
        private readonly SemanticModel semanticModel;

        public XmlReaderSettingsValidator(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        public bool IsUnsafe(InvocationExpressionSyntax invocation, ISymbol settings, NetFrameworkVersion version)
        {
            var unsafeDtdProcessing = false;
            var unsafeResolver = version == NetFrameworkVersion.Probably35 || version == NetFrameworkVersion.Between4And451;

            var objectCreation = GetObjectCreation(settings, invocation, semanticModel);
            var objectCreationAssignments = objectCreation.GetInitializerExpressions().OfType<AssignmentExpressionSyntax>();

            var assignments = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(assignment => assignment.Left is MemberAccessExpressionSyntax memberAccess &&
                                     IsXmlReaderSettings(memberAccess.Expression, semanticModel) &&
                                     settings.Equals(semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol));

            foreach (var assignment in objectCreationAssignments.Union(assignments))
            {
                var name = assignment.Left.GetName();

                if (name =="ProhibitDtd" || name == "DtdProcessing")
                {
                    unsafeDtdProcessing = IsXmlResolverDtdProcessingUnsafe(assignment, semanticModel);
                }
                else if (assignment.Left.NameIs("XmlResolver"))
                {
                    unsafeResolver = IsXmlResolverAssignmentUnsafe(assignment, semanticModel);
                }
            }

            return unsafeDtdProcessing && unsafeResolver;
        }

        private static bool IsXmlReaderSettings(ExpressionSyntax expressionSyntax, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expressionSyntax).Type.Is(KnownType.System_Xml_XmlReaderSettings);

        private static ObjectCreationExpressionSyntax GetObjectCreation(ISymbol symbol, InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            symbol.Locations
                .SelectMany(location => GetDescendantNodes(location, invocation).OfType<ObjectCreationExpressionSyntax>())
                .FirstOrDefault(objectCreation => objectCreation.Initializer != null && IsXmlReaderSettings(objectCreation, semanticModel));

        private static IEnumerable<SyntaxNode> GetDescendantNodes(Location location, InvocationExpressionSyntax invocation) =>
            location.SourceTree?.GetRoot()?.FindNode(location.SourceSpan).DescendantNodes() ??
            invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.DescendantNodes() ??
            Enumerable.Empty<SyntaxNode>();

        private static bool IsXmlResolverDtdProcessingUnsafe(AssignmentExpressionSyntax assignment, SemanticModel semanticModel) =>
            semanticModel.GetConstantValue(assignment.Right).Value switch
            {
                false => true,
                (int)DtdProcessing.Parse => true,
                _ => false
            };

        private static bool IsXmlResolverAssignmentUnsafe(AssignmentExpressionSyntax assignment, SemanticModel semanticModel)
        {
            if (assignment.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(assignment.Right).Symbol;
            var type = symbol.GetSymbolType() ?? symbol.ContainingType;

            return type.IsAny(KnownType.System_Xml_XmlUrlResolver, KnownType.System_Xml_Resolvers_XmlPreloadedResolver);
        }
    }
}
