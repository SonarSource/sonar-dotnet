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

using System.Xml;
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.Rules.XXE;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class XmlExternalEntityShouldNotBeParsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2755";
        private const string MessageFormat = "Disable access to external entities in XML parsing.";
        private const string SecondaryMessageFormat = "This value enables external entities in XML parsing.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        // For the XXE rule we actually need to know about .NET 4.5.2,
        // but it is good enough given the other .NET 4.x do not have support anymore
        private readonly INetFrameworkVersionProvider versionProvider;

        public XmlExternalEntityShouldNotBeParsed() : this(new NetFrameworkVersionProvider()) { }

        internal /*for testing*/ XmlExternalEntityShouldNotBeParsed(INetFrameworkVersionProvider netFrameworkVersionProvider) =>
            versionProvider = netFrameworkVersionProvider;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    ccc.RegisterNodeAction(
                        c =>
                        {
                            var objectCreation = ObjectCreationFactory.Create(c.Node);
                            var netFrameworkVersion = versionProvider.GetDotNetFrameworkVersion(c.Compilation);
                            var constructorIsSafe = ConstructorIsSafe(netFrameworkVersion);

                            var trackers = TrackerFactory.Create();
                            if (trackers.XmlDocumentTracker.ShouldBeReported(objectCreation, c.SemanticModel, constructorIsSafe)
                               || trackers.XmlTextReaderTracker.ShouldBeReported(objectCreation, c.SemanticModel, constructorIsSafe))
                            {
                                c.ReportIssue(CreateDiagnostic(Rule, objectCreation.Expression.GetLocation()));
                            }

                            VerifyXPathDocumentConstructor(c, objectCreation);
                        },
                        SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

                    ccc.RegisterNodeAction(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;

                            var trackers = TrackerFactory.Create();
                            if (trackers.XmlDocumentTracker.ShouldBeReported(assignment, c.SemanticModel)
                               || trackers.XmlTextReaderTracker.ShouldBeReported(assignment, c.SemanticModel))
                            {
                                c.ReportIssue(CreateDiagnostic(Rule, assignment.GetLocation()));
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression);

                    ccc.RegisterNodeAction(VerifyXmlReaderInvocations, SyntaxKind.InvocationExpression);
                });

        private void VerifyXmlReaderInvocations(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!invocation.IsMemberAccessOnKnownType("Create", KnownType.System_Xml_XmlReader, context.SemanticModel))
            {
                return;
            }

            var settings = invocation.GetArgumentSymbolsOfKnownType(KnownType.System_Xml_XmlReaderSettings, context.SemanticModel).FirstOrDefault();
            if (settings == null)
            {
                return; // safe by default
            }

            var xmlReaderSettingsValidator = new XmlReaderSettingsValidator(context.SemanticModel, versionProvider.GetDotNetFrameworkVersion(context.Compilation));
            if (xmlReaderSettingsValidator.GetUnsafeAssignmentLocations(invocation, settings) is { } secondaryLocations && secondaryLocations.Any())
            {
                context.ReportIssue(CreateDiagnostic(Rule, invocation.GetLocation(), secondaryLocations, secondaryLocations.ToProperties(SecondaryMessageFormat)));
            }
        }

        private void VerifyXPathDocumentConstructor(SonarSyntaxNodeReportingContext context, IObjectCreation objectCreation)
        {
            if (!context.SemanticModel.GetTypeInfo(objectCreation.Expression).Type.Is(KnownType.System_Xml_XPath_XPathDocument)
                // If a XmlReader is provided in the constructor, XPathDocument will be as safe as the received reader.
                // In this case we don't raise a warning since the XmlReader has it's own checks.
                || objectCreation.ArgumentList.Arguments.GetArgumentsOfKnownType(KnownType.System_Xml_XmlReader, context.SemanticModel).Any())
            {
                return;
            }

            if (!IsXPathDocumentSecureByDefault(versionProvider.GetDotNetFrameworkVersion(context.Compilation)))
            {
                context.ReportIssue(CreateDiagnostic(Rule, objectCreation.Expression.GetLocation()));
            }
        }

        private static bool IsXPathDocumentSecureByDefault(NetFrameworkVersion version) =>
            // XPathDocument is secure by default starting with .Net 4.5.2
            version == NetFrameworkVersion.After452 || version == NetFrameworkVersion.Unknown;

        // The XmlDocument and XmlTextReader constructors were made safe-by-default in .NET 4.5.2
        private static bool ConstructorIsSafe(NetFrameworkVersion version) =>
            version switch
            {
                NetFrameworkVersion.After452 => true,
                NetFrameworkVersion.Between4And451 => false,
                NetFrameworkVersion.Probably35 => false,
                _ => true
            };

        private static class TrackerFactory
        {
            private static ImmutableArray<KnownType> UnsafeXmlResolvers { get; } = ImmutableArray.Create(
                KnownType.System_Xml_XmlUrlResolver,
                KnownType.System_Xml_Resolvers_XmlPreloadedResolver
            );

            private static readonly ImmutableArray<KnownType> XmlDocumentTrackedTypes = ImmutableArray.Create(
                KnownType.System_Xml_XmlDocument,
                KnownType.System_Xml_XmlDataDocument,
                KnownType.System_Configuration_ConfigXmlDocument,
                KnownType.Microsoft_Web_XmlTransform_XmlFileInfoDocument,
                KnownType.Microsoft_Web_XmlTransform_XmlTransformableDocument
            );

            private static readonly ISet<string> XmlTextReaderTrackedProperties = ImmutableHashSet.Create(
                "XmlResolver", // should be null
                "DtdProcessing", // should not be Parse
                "ProhibitDtd" // should be true in .NET 3.5
            );

            public static TrackersHolder Create()
            {
                var xmlDocumentTracker = new CSharpObjectInitializationTracker(
                    // we do not expect any constant values for XmlResolver
                    isAllowedConstantValue: constantValue => false,
                    trackedTypes: XmlDocumentTrackedTypes,
                    isTrackedPropertyName: propertyName => propertyName == "XmlResolver",
                    isAllowedObject: (symbol, _, __) => IsAllowedObject(symbol)
                );

                var xmlTextReaderTracker = new CSharpObjectInitializationTracker(
                    isAllowedConstantValue: IsAllowedValueForXmlTextReader,
                    trackedTypes: ImmutableArray.Create(KnownType.System_Xml_XmlTextReader),
                    isTrackedPropertyName: XmlTextReaderTrackedProperties.Contains,
                    isAllowedObject: (symbol, _, __) => IsAllowedObject(symbol)
                );

                return new TrackersHolder(xmlDocumentTracker, xmlTextReaderTracker);
            }

            private static bool IsAllowedValueForXmlTextReader(object constantValue)
            {
                if (constantValue == null)
                {
                    return true;
                }
                if (constantValue is int integerValue)
                {
                    return integerValue != (int)DtdProcessing.Parse;
                }
                // treat the ProhibitDtd property
                return constantValue is bool value && value;
            }

            private static bool IsUnsafeXmlResolverConstructor(ISymbol symbol) =>
                symbol.Kind == SymbolKind.Method
                && symbol.ContainingType.GetSymbolType().IsAny(UnsafeXmlResolvers);

            private static bool IsAllowedObject(ISymbol symbol) =>
                !IsUnsafeXmlResolverConstructor(symbol)
                && !symbol.GetSymbolType().IsAny(UnsafeXmlResolvers)
                && !IsUnsafeXmlResolverReturnType(symbol);

            private static bool IsUnsafeXmlResolverReturnType(ISymbol symbol) =>
                symbol is IMethodSymbol methodSymbol
                && methodSymbol.ReturnType.IsAny(UnsafeXmlResolvers);
        }

        private readonly struct TrackersHolder
        {
            internal readonly CSharpObjectInitializationTracker XmlDocumentTracker;
            internal readonly CSharpObjectInitializationTracker XmlTextReaderTracker;

            internal TrackersHolder(CSharpObjectInitializationTracker xmlDocumentTracker, CSharpObjectInitializationTracker xmlTextReaderTracker)
            {
                XmlDocumentTracker = xmlDocumentTracker;
                XmlTextReaderTracker = xmlTextReaderTracker;
            }
        }
    }
}
