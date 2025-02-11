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

using System.Xml;

namespace SonarAnalyzer.CSharp.Rules.XXE
{
    /// <summary>
    /// This class is responsible to check if a XmlReaderSettings node is vulnerable to XXE attacks.
    ///
    /// By default the XmlReaderSettings is safe:
    ///  - before .Net 4.5.2 it has ProhibitDtd set to true (even if the internal XmlResolver is not secure)
    ///  - starting with .Net 4.5.2 XmlResolver is set to null, ProhibitDtd set to true and DtdProcessing set to ignore
    ///
    /// If the properties are modified, in order to be secure, we have to check that either ProhibitDtd is set to true, DtdProcessing set to Ignore or
    /// the internal XmlResolver is secure.
    /// </summary>
    internal class XmlReaderSettingsValidator
    {
        private readonly SemanticModel semanticModel;
        private readonly bool isXmlResolverSafeByDefault;

        public XmlReaderSettingsValidator(SemanticModel semanticModel, NetFrameworkVersion version)
        {
            this.semanticModel = semanticModel;
            isXmlResolverSafeByDefault = IsXmlResolverPropertySafeByDefault(version);
        }

        /// <summary>
        /// Gets the assignment locations of XmlReaderSettings properties which are unsafe and are used in invocations afterwards (e.g. XmlReader.Create).
        /// </summary>
        /// <param name="invocation">A method invocation syntax node (e.g. XmlReader.Create).</param>
        /// <param name="settings">The symbol of the XmlReaderSettings node received as parameter. This is used to check
        /// if certain properties (ProhibitDtd, DtdProcessing or XmlUrlResolver) were modified for the given symbol.</param>
        /// <returns>The list of unsafe assignment locations.</returns>
        public IList<SecondaryLocation> GetUnsafeAssignmentLocations(InvocationExpressionSyntax invocation, ISymbol settings, string message)
        {
            var unsafeAssignmentLocations = new List<SecondaryLocation>();
            // By default ProhibitDtd is 'true' and DtdProcessing is 'ignore'
            var unsafeDtdProcessing = false;
            var unsafeResolver = isXmlResolverSafeByDefault;

            var objectCreation = GetObjectCreation(settings, invocation, semanticModel);
            var objectCreationAssignments = objectCreation?.InitializerExpressions.OfType<AssignmentExpressionSyntax>()
                ?? Enumerable.Empty<AssignmentExpressionSyntax>();

            var propertyAssignments = GetAssignments(invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>())
                .Where(assignment => IsMemberAccessOnSymbol(assignment.Left, settings, semanticModel));

            foreach (var assignment in objectCreationAssignments.Union(propertyAssignments))
            {
                var name = assignment.Left.GetName();

                if (name == "ProhibitDtd" || name == "DtdProcessing")
                {
                    unsafeDtdProcessing = IsXmlResolverDtdProcessingUnsafe(assignment, semanticModel);
                    if (unsafeDtdProcessing)
                    {
                        unsafeAssignmentLocations.Add(assignment.ToSecondaryLocation(message));
                    }
                }
                else if (name == "XmlResolver")
                {
                    unsafeResolver = IsXmlResolverAssignmentUnsafe(assignment, semanticModel);
                    if (unsafeResolver)
                    {
                        unsafeAssignmentLocations.Add(assignment.ToSecondaryLocation(message));
                    }
                }
            }

            return unsafeDtdProcessing && unsafeResolver ? unsafeAssignmentLocations : [];
        }

        private static bool IsMemberAccessOnSymbol(ExpressionSyntax expression, ISymbol symbol, SemanticModel semanticModel) =>
            expression is MemberAccessExpressionSyntax memberAccess
            && semanticModel.GetTypeInfo(memberAccess.Expression).Type.Is(KnownType.System_Xml_XmlReaderSettings)
            && symbol.Equals(semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol);

        private static IEnumerable<AssignmentExpressionSyntax> GetAssignments(SyntaxNode node) =>
            node == null
                ? Enumerable.Empty<AssignmentExpressionSyntax>()
                : node.DescendantNodes().OfType<AssignmentExpressionSyntax>();

        private static bool IsXmlResolverPropertySafeByDefault(NetFrameworkVersion version) =>
            version == NetFrameworkVersion.Probably35 || version == NetFrameworkVersion.Between4And451;

        private static IObjectCreation GetObjectCreation(ISymbol symbol, InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            // First we search for object creations at the syntax level to see if the object is created inline
            // and if not we look for the identifier declaration.
            invocation.DescendantNodes()
                      .Union(symbol.GetLocationNodes(invocation))
                      .Where(x => x?.Kind() is SyntaxKind.ObjectCreationExpression or SyntaxKindEx.ImplicitObjectCreationExpression)
                      .Select(ObjectCreationFactory.Create)
                      .FirstOrDefault(objectCreation => IsXmlReaderSettingsCreationWithInitializer(objectCreation, semanticModel));

        private static bool IsXmlReaderSettingsCreationWithInitializer(IObjectCreation objectCreation, SemanticModel semanticModel) =>
            objectCreation.Initializer != null && objectCreation.TypeSymbol(semanticModel).Is(KnownType.System_Xml_XmlReaderSettings);

        private static bool IsXmlResolverDtdProcessingUnsafe(AssignmentExpressionSyntax assignment, SemanticModel semanticModel) =>
            semanticModel.GetConstantValue(assignment.Right).Value switch
            {
                false => true, // If ProhibitDtd is set to false the settings will be unsafe (parsing is allowed)
                (int)DtdProcessing.Parse => true,
                _ => false
            };

        private static bool IsXmlResolverAssignmentUnsafe(AssignmentExpressionSyntax assignment, SemanticModel semanticModel)
        {
            if (assignment.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return false;
            }

            var type = semanticModel.GetTypeInfo(assignment.Right).Type;
            return type.IsAny(KnownType.System_Xml_XmlUrlResolver, KnownType.System_Xml_Resolvers_XmlPreloadedResolver);
        }
    }
}
