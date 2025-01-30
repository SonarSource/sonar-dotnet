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
using SonarAnalyzer.CSharp.Core.Trackers;
using SonarAnalyzer.Rules.XXE;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class XmlExternalEntityShouldNotBeParsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2755";
        private const string MessageFormat = "Disable access to external entities in XML parsing.";
        private const string SecondaryMessage = "This value enables external entities in XML parsing.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        // For the XXE rule we actually need to know about .NET 4.5.2,
        // but it is good enough given the other .NET 4.x do not have support anymore
        private readonly NetFrameworkVersionProvider versionProvider;

        public XmlExternalEntityShouldNotBeParsed() : this(new NetFrameworkVersionProvider()) { }

        internal /*for testing*/ XmlExternalEntityShouldNotBeParsed(NetFrameworkVersionProvider netFrameworkVersionProvider) =>
            versionProvider = netFrameworkVersionProvider;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    ccc.RegisterNodeAction(
                        c =>
                        {
                            var objectCreation = ObjectCreationFactory.Create(c.Node);
                            var netFrameworkVersion = versionProvider.Version(c.Compilation);
                            var constructorIsSafe = ConstructorIsSafe(netFrameworkVersion);
                            var trackers = TrackerFactory.Create();
                            if (trackers.XmlDocumentTracker.ShouldBeReported(objectCreation, c.Model, constructorIsSafe)
                               || trackers.XmlTextReaderTracker.ShouldBeReported(objectCreation, c.Model, constructorIsSafe))
                            {
                                c.ReportIssue(Rule, objectCreation.Expression);
                            }

                            VerifyXPathDocumentConstructor(c, objectCreation);
                        },
                        SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

                    ccc.RegisterNodeAction(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;

                            var trackers = TrackerFactory.Create();
                            if (trackers.XmlDocumentTracker.ShouldBeReported(assignment, c.Model)
                               || trackers.XmlTextReaderTracker.ShouldBeReported(assignment, c.Model))
                            {
                                c.ReportIssue(Rule, assignment);
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression);

                    ccc.RegisterNodeAction(VerifyXmlReaderInvocations, SyntaxKind.InvocationExpression);
                });

        private void VerifyXmlReaderInvocations(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!invocation.IsMemberAccessOnKnownType("Create", KnownType.System_Xml_XmlReader, context.Model))
            {
                return;
            }

            var settings = invocation.GetArgumentSymbolsOfKnownType(KnownType.System_Xml_XmlReaderSettings, context.Model).FirstOrDefault();
            if (settings == null)
            {
                return; // safe by default
            }

            var xmlReaderSettingsValidator = new XmlReaderSettingsValidator(context.Model, versionProvider.Version(context.Compilation));
            if (xmlReaderSettingsValidator.GetUnsafeAssignmentLocations(invocation, settings, SecondaryMessage) is { } secondaryLocations && secondaryLocations.Any())
            {
                context.ReportIssue(Rule, invocation, secondaryLocations);
            }
        }

        private void VerifyXPathDocumentConstructor(SonarSyntaxNodeReportingContext context, IObjectCreation objectCreation)
        {
            if (!context.Model.GetTypeInfo(objectCreation.Expression).Type.Is(KnownType.System_Xml_XPath_XPathDocument)
                // If a XmlReader is provided in the constructor, XPathDocument will be as safe as the received reader.
                // In this case we don't raise a warning since the XmlReader has it's own checks.
                || objectCreation.ArgumentList.Arguments.GetArgumentsOfKnownType(KnownType.System_Xml_XmlReader, context.Model).Any())
            {
                return;
            }

            if (!IsXPathDocumentSecureByDefault(versionProvider.Version(context.Compilation)))
            {
                context.ReportIssue(Rule, objectCreation.Expression);
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
