/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Immutable;
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
    public sealed class ProvideDeserializationMethodsForOptionalFields : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3926";
        private const string MessageFormat = "{0}";
        private const string BothDeserializationMethodsMissing = "Add deserialization event handlers.";
        private const string OnDeserializedMethodMissing = "Add the missing 'OnDeserializedAttribute' event handler.";
        private const string OnDeserializingMethodMissing = "Add the missing 'OnDeserializingAttribute' event handler.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> AttributesToFind = new HashSet<KnownType>
        {
            KnownType.System_Runtime_Serialization_OptionalFieldAttribute,
            KnownType.System_Runtime_Serialization_OnDeserializingAttribute,
            KnownType.System_Runtime_Serialization_OnDeserializedAttribute
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null)
                    {
                        return;
                    }

                    var watchedAttributes = classSymbol.GetMembers()
                        .SelectMany(m => m.GetAttributes(AttributesToFind))
                        .ToList();

                    var errorMessage = GetErrorMessage(watchedAttributes);
                    if (errorMessage == null)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation(),
                        errorMessage));
                }, SyntaxKind.ClassDeclaration);
        }

        private string GetErrorMessage(IEnumerable<AttributeData> attributes)
        {
            var hasOptionalFieldAttribute = attributes.Any(attribute =>
                attribute.AttributeClass.Is(KnownType.System_Runtime_Serialization_OptionalFieldAttribute));
            var hasOnDeserializingAttribute = attributes.Any(attribute =>
                attribute.AttributeClass.Is(KnownType.System_Runtime_Serialization_OnDeserializingAttribute));
            var hasOnDeserializedAttribute = attributes.Any(attribute =>
                attribute.AttributeClass.Is(KnownType.System_Runtime_Serialization_OnDeserializedAttribute));

            if (!hasOptionalFieldAttribute ||
                hasOnDeserializingAttribute && hasOnDeserializedAttribute)
            {
                return null;
            }

            if (hasOnDeserializingAttribute)
            {
                return OnDeserializedMethodMissing;
            }

            if (hasOnDeserializedAttribute)
            {
                return OnDeserializingMethodMissing;
            }

            return BothDeserializationMethodsMissing;
        }
    }
}
