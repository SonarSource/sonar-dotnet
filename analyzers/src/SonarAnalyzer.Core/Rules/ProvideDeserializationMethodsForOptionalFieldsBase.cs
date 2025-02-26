﻿/*
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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class ProvideDeserializationMethodsForOptionalFieldsBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3926";
        private const string MessageFormat = "{0}";
        private const string BothDeserializationMethodsMissing = "Add deserialization event handlers.";
        private const string OnDeserializedMethodMissing = "Add the missing 'OnDeserializedAttribute' event handler.";
        private const string OnDeserializingMethodMissing = "Add the missing 'OnDeserializingAttribute' event handler.";

        private static readonly ImmutableArray<KnownType> AttributesToFind = ImmutableArray.Create(KnownType.System_Runtime_Serialization_OptionalFieldAttribute,
                                                                                                   KnownType.System_Runtime_Serialization_OnDeserializingAttribute,
                                                                                                   KnownType.System_Runtime_Serialization_OnDeserializedAttribute);

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ProvideDeserializationMethodsForOptionalFieldsBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var symbol = (INamedTypeSymbol)c.Symbol;
                    if (!symbol.IsClassOrStruct())
                    {
                        return;
                    }

                    var watchedAttributes = symbol.GetMembers()
                                                  .SelectMany(m => m.GetAttributes(AttributesToFind))
                                                  .ToList();

                    var errorMessage = GetErrorMessage(watchedAttributes);
                    var declaringSyntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

                    if (errorMessage == null || declaringSyntax == null)
                    {
                        return;
                    }

                    c.ReportIssue(Language.GeneratedCodeRecognizer, SupportedDiagnostics[0], GetNamedTypeIdentifierLocation(declaringSyntax), errorMessage);
                },
                SymbolKind.NamedType);

        protected abstract Location GetNamedTypeIdentifierLocation(SyntaxNode node);

        private static string GetErrorMessage(IReadOnlyCollection<AttributeData> attributes)
        {
            var hasOptionalFieldAttribute = attributes.Any(attribute =>
                attribute.AttributeClass.Is(KnownType.System_Runtime_Serialization_OptionalFieldAttribute));
            var hasOnDeserializingAttribute = attributes.Any(attribute =>
                attribute.AttributeClass.Is(KnownType.System_Runtime_Serialization_OnDeserializingAttribute));
            var hasOnDeserializedAttribute = attributes.Any(attribute =>
                attribute.AttributeClass.Is(KnownType.System_Runtime_Serialization_OnDeserializedAttribute));

            if (!hasOptionalFieldAttribute || (hasOnDeserializingAttribute && hasOnDeserializedAttribute))
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
