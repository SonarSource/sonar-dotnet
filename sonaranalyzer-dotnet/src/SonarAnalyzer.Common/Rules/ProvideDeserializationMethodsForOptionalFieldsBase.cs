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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class ProvideDeserializationMethodsForOptionalFieldsBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3926";
        protected const string MessageFormat = "{0}";
        protected const string BothDeserializationMethodsMissing = "Add deserialization event handlers.";
        protected const string OnDeserializedMethodMissing = "Add the missing 'OnDeserializedAttribute' event handler.";
        protected const string OnDeserializingMethodMissing = "Add the missing 'OnDeserializingAttribute' event handler.";

        internal static readonly ImmutableArray<KnownType> AttributesToFind =
            ImmutableArray.Create(
                KnownType.System_Runtime_Serialization_OptionalFieldAttribute,
                KnownType.System_Runtime_Serialization_OnDeserializingAttribute,
                KnownType.System_Runtime_Serialization_OnDeserializedAttribute
            );

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var namedTypeSymbol = (INamedTypeSymbol)c.Symbol;
                    if (namedTypeSymbol.TypeKind != TypeKind.Class &&
                        namedTypeSymbol.TypeKind != TypeKind.Struct)
                    {
                        return;
                    }

                    var watchedAttributes = namedTypeSymbol.GetMembers()
                        .SelectMany(m => m.GetAttributes(AttributesToFind))
                        .ToList();

                    var errorMessage = GetErrorMessage(watchedAttributes);
                    var declaringSyntax = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

                    if (errorMessage == null ||
                        declaringSyntax == null)
                    {
                        return;
                    }

                    c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0],
                        GetNamedTypeIdentifierLocation(declaringSyntax),
                        errorMessage));
                },
                SymbolKind.NamedType);
        }

        protected abstract Location GetNamedTypeIdentifierLocation(SyntaxNode node);

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
