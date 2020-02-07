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
using Microsoft.CodeAnalysis.CSharp;
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
        internal const string DiagnosticId = "S2755";
        private const string MessageFormat = "Disable access to external entities in XML parsing.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            (new XmlDocumentShouldBeSafe()).InternalInitialize(context);
            (new XmlTextReaderShouldBeSafe()).InternalInitialize(context);

            //context.registersyntaxnodeactioninnongenerated(c =>
            //    {
            //        var node = c.node;
            //        if (true)
            //        {
            //            c.reportdiagnosticwhenactive(diagnostic.create(rule, node.getlocation()));
            //        }
            //    },
            //    syntaxkind.invocationexpression);
        }

        [SuppressMessage("AnalyzerCorrectness", "RS1001:Missing diagnostic analyzer attribute.", Justification = "This is not a different diagnostic.")]
        private class XmlDocumentShouldBeSafe : ObjectShouldBeInitializedCorrectlyBase
        {
            private static ImmutableArray<KnownType> UnsafeXmlResolvers { get; } = ImmutableArray.Create(
                KnownType.System_Xml_XmlUrlResolver,
                KnownType.System_Xml_Resolvers_XmlPreloadedResolver
            );

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

            protected override string TrackedPropertyName => "XmlResolver";

            internal override ImmutableArray<KnownType> TrackedTypes { get; } = ImmutableArray.Create(
                KnownType.System_Xml_XmlDocument,
                KnownType.System_Xml_XmlDataDocument,
                KnownType.System_Configuration_ConfigXmlDocument,
                KnownType.Microsoft_Web_XmlTransform_XmlFileInfoDocument,
                KnownType.Microsoft_Web_XmlTransform_XmlTransformableDocument
            );

            // FIXME create constructor that gets the target framework and IsAllowedValue should consider that
            protected override bool CtorInitializesTrackedPropertyWithAllowedValue(ArgumentListSyntax argumentList, SemanticModel semanticModel) => true;

            // FIXME create constructor that gets the target framework and IsAllowedValue should consider that
            protected override bool IsAllowedValue(object constantValue) => constantValue == null;

            protected override bool IsAllowedValue(ISymbol symbol) =>
                !IsUnsafeXmlResolverConstructor(symbol) &&
                !symbol.GetSymbolType().IsAny(UnsafeXmlResolvers);

            private bool IsUnsafeXmlResolverConstructor(ISymbol symbol) =>
                symbol.Kind == SymbolKind.Method &&
                symbol.ContainingType.GetSymbolType().IsAny(UnsafeXmlResolvers);
        }

        [SuppressMessage("AnalyzerCorrectness", "RS1001:Missing diagnostic analyzer attribute.", Justification = "This is not a different diagnostic.")]
        private class XmlTextReaderShouldBeSafe : ObjectShouldBeInitializedCorrectlyBase
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

            protected override string TrackedPropertyName => "DtdProcessing";

            internal override ImmutableArray<KnownType> TrackedTypes { get; } = ImmutableArray.Create(KnownType.System_Xml_XmlTextReader);

            // FIXME create constructor that gets the target framework and IsAllowedValue should consider that
            protected override bool IsAllowedValue(object constantValue)
            {
                throw new System.NotImplementedException();
            }

            protected override bool IsAllowedValue(ISymbol symbol)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}

