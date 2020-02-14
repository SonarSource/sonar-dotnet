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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SyntaxTrackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class XmlExternalEntityShouldNotBeParsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2755";
        private const string MessageFormat = "Disable access to external entities in XML parsing.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        // For the XXE rule we actually need to know about .NET 4.5.2,
        // but it is good enough given the other .NET 4.x do not have support anymore
        private readonly INetFrameworkVersionProvider VersionProvider;

        public XmlExternalEntityShouldNotBeParsed()
            : this(new NetFrameworkVersionProvider())
        {
        }

        internal /*for testing*/ XmlExternalEntityShouldNotBeParsed(INetFrameworkVersionProvider netFrameworkVersionProvider)
        {
            VersionProvider = netFrameworkVersionProvider;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var objectCreation = (ObjectCreationExpressionSyntax)c.Node;

                            var trackers = TrackerFactory.Create(c.Compilation, VersionProvider);
                            if (trackers.Item1.IsAnalyzedIncorrectly(objectCreation, c.SemanticModel) ||
                                trackers.Item2.IsAnalyzedIncorrectly(objectCreation, c.SemanticModel))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], objectCreation.GetLocation()));
                            }
                        },
                        SyntaxKind.ObjectCreationExpression);

                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;

                            var trackers = TrackerFactory.Create(c.Compilation, VersionProvider);
                            if (trackers.Item1.IsAnalyzedIncorrectly(assignment, c.SemanticModel) ||
                                trackers.Item2.IsAnalyzedIncorrectly(assignment, c.SemanticModel))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], assignment.GetLocation()));
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression);
                });

         }

        private static class TrackerFactory
        {
            // The value of System.Xml.DtdProcessing.Parse
            private const int DtdProcessingParse = 2;

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

            public static Tuple<CSharpObjectInitializationTracker, CSharpObjectInitializationTracker> Create(Compilation compilation, INetFrameworkVersionProvider versionProvider)
            {
                var netFrameworkVersion = versionProvider.GetDotNetFrameworkVersion(compilation);
                var constructorIsSafe = ConstructorIsSafe(netFrameworkVersion);

                var xmlDocumentTracker = new CSharpObjectInitializationTracker(
                    // we do not expect any constant values for XmlResolver
                    isAllowedConstantValue: constantValue => true,
                    trackedTypes: XmlDocumentTrackedTypes,
                    isTrackedPropertyName: propertyName => "XmlResolver" == propertyName,
                    isAllowedObject: symbol => IsAllowedObject(symbol),
                    constructorIsSafe: constructorIsSafe
                );

                var xmlTextReaderTracker = new CSharpObjectInitializationTracker(
                    isAllowedConstantValue: constantValue => IsAllowedValueForXmlTextReader(constantValue),
                    trackedTypes: ImmutableArray.Create(KnownType.System_Xml_XmlTextReader),
                    isTrackedPropertyName : propertyName => XmlTextReaderTrackedProperties.Contains(propertyName),
                    isAllowedObject: symbol => IsAllowedObject(symbol),
                    constructorIsSafe: constructorIsSafe
                );

                return Tuple.Create(xmlDocumentTracker, xmlTextReaderTracker);
            }

            // The XmlDocument and XmlTextReader constructors were made safe-by-default in .NET 4.5.2
            private static bool ConstructorIsSafe(NetFrameworkVersion version) =>
                version switch
                {
                    NetFrameworkVersion.After45 => true,
                    NetFrameworkVersion.Between4And45 => false,
                    NetFrameworkVersion.Below4 => false,
                    _ => true
                };

            private static bool IsAllowedValueForXmlTextReader(object constantValue)
            {
                if (constantValue == null)
                {
                    return true;
                }
                if (constantValue is int integerValue)
                {
                    // treat the DtdProcessing property
                    return integerValue != DtdProcessingParse;
                }
                // treat the ProhibitDtd property
                return constantValue is bool && (bool)constantValue;
            }

            private static bool IsUnsafeXmlResolverConstructor(ISymbol symbol) =>
                    symbol.Kind == SymbolKind.Method &&
                    symbol.ContainingType.GetSymbolType().IsAny(UnsafeXmlResolvers);

            private static bool IsAllowedObject(ISymbol symbol) =>
                !IsUnsafeXmlResolverConstructor(symbol) &&
                !symbol.GetSymbolType().IsAny(UnsafeXmlResolvers);
        }
    }
}

