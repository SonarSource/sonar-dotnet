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

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class XmlExternalEntityShouldNotBeParsed : SonarDiagnosticAnalyzer
    {
        // For the XXE rule we actually need to know about .NET 4.5.2,
        // but it is good enough given the other .NET 4.x do not have support anymore

        internal const string DiagnosticId = "S2755";
        private const string MessageFormat = "Disable access to external entities in XML parsing.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static ImmutableArray<KnownType> UnsafeXmlResolvers { get; } = ImmutableArray.Create(
                KnownType.System_Xml_XmlUrlResolver,
                KnownType.System_Xml_Resolvers_XmlPreloadedResolver
        );

        // The value of System.Xml.DtdProcessing.Parse
        private const int DtdProcessingParse = 2;

        private INetFrameworkVersionProvider VersionProvider;

        public XmlExternalEntityShouldNotBeParsed()
            : this(NetFrameworkVersionProvider.Instance)
        {
        }

        internal /*for testing*/ XmlExternalEntityShouldNotBeParsed(INetFrameworkVersionProvider netFrameworkVersionProvider)
        {
            VersionProvider = netFrameworkVersionProvider;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            (new XmlDocumentShouldBeSafe(VersionProvider)).InternalInitialize(context);
            (new XmlTextReaderShouldBeSafe(VersionProvider)).InternalInitialize(context);

            // FIXME Implement for XmlReader and XPathNavigator
         }

        [SuppressMessage("AnalyzerCorrectness", "RS1001:Missing diagnostic analyzer attribute.", Justification = "This is not a different diagnostic.")]
        private class XmlDocumentShouldBeSafe : ObjectShouldBeInitializedCorrectlyBase
        {
            private NetFrameworkVersion NetFrameworkVersion;

            private INetFrameworkVersionProvider VersionProvider;

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

            protected override bool IsTrackedPropertyName(string propertyName) => "XmlResolver".Equals(propertyName);

            internal override ImmutableArray<KnownType> TrackedTypes { get; } = ImmutableArray.Create(
                KnownType.System_Xml_XmlDocument,
                KnownType.System_Xml_XmlDataDocument,
                KnownType.System_Configuration_ConfigXmlDocument,
                KnownType.Microsoft_Web_XmlTransform_XmlFileInfoDocument,
                KnownType.Microsoft_Web_XmlTransform_XmlTransformableDocument
            );

            protected override bool CtorInitializesTrackedPropertyWithAllowedValue(ArgumentListSyntax argumentList, SemanticModel semanticModel) =>
                NetFrameworkVersion switch
                {
                    NetFrameworkVersion.After45 => true,
                    NetFrameworkVersion.Between4And45 => false,
                    NetFrameworkVersion.Below4 => false,
                    _ => true
                };

            // we do not expect any constant values for XmlResolver
            protected override bool IsAllowedValue(object constantValue) => true;

            protected override bool IsAllowedValue(ISymbol symbol) =>
                !IsUnsafeXmlResolverConstructor(symbol) &&
                !symbol.GetSymbolType().IsAny(UnsafeXmlResolvers);

            protected override void CompilationAction(Compilation compilation)
            {
                NetFrameworkVersion = VersionProvider.GetDotNetFrameworkVersion(compilation);
            }

            internal XmlDocumentShouldBeSafe(INetFrameworkVersionProvider netFrameworkVersionProvider)
            {
                VersionProvider = netFrameworkVersionProvider;
            }

            private bool IsUnsafeXmlResolverConstructor(ISymbol symbol) =>
                symbol.Kind == SymbolKind.Method &&
                symbol.ContainingType.GetSymbolType().IsAny(UnsafeXmlResolvers);
        }

        [SuppressMessage("AnalyzerCorrectness", "RS1001:Missing diagnostic analyzer attribute.", Justification = "This is not a different diagnostic.")]
        private class XmlTextReaderShouldBeSafe : ObjectShouldBeInitializedCorrectlyBase
        {
            private NetFrameworkVersion NetFrameworkVersion;

            private INetFrameworkVersionProvider VersionProvider;

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

            protected override bool IsTrackedPropertyName(string propertyName) => "XmlResolver".Equals(propertyName) || "DtdProcessing".Equals(propertyName);

            internal override ImmutableArray<KnownType> TrackedTypes { get; } = ImmutableArray.Create(KnownType.System_Xml_XmlTextReader);

            // FIXME create constructor that gets the target framework and IsAllowedValue should consider that
            protected override bool CtorInitializesTrackedPropertyWithAllowedValue(ArgumentListSyntax argumentList, SemanticModel semanticModel) =>
                NetFrameworkVersion switch
                {
                    NetFrameworkVersion.After45 => true,
                    NetFrameworkVersion.Between4And45 => false,
                    NetFrameworkVersion.Below4 => false,
                    _ => true
                };

            // FIXME create constructor that gets the target framework and IsAllowedValue should consider that
            protected override bool IsAllowedValue(ISymbol symbol) =>
                !IsUnsafeXmlResolverConstructor(symbol) &&
                !symbol.GetSymbolType().IsAny(UnsafeXmlResolvers);

            protected override bool IsAllowedValue(object constantValue)
            {
                if (constantValue is int integerValue)
                {
                    return integerValue != DtdProcessingParse;
                }
                return constantValue == null;
            }

            protected override void CompilationAction(Compilation compilation)
            {
                NetFrameworkVersion = VersionProvider.GetDotNetFrameworkVersion(compilation);
            }

            internal XmlTextReaderShouldBeSafe(INetFrameworkVersionProvider netFrameworkVersionProvider)
            {
                VersionProvider = netFrameworkVersionProvider;
            }

            private bool IsUnsafeXmlResolverConstructor(ISymbol symbol) =>
                symbol.Kind == SymbolKind.Method &&
                symbol.ContainingType.GetSymbolType().IsAny(UnsafeXmlResolvers);
        }
    }
}

