﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MemberInitializedToDefault : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3052";
        private const string MessageFormat = "Remove this initialization to '{0}', the compiler will do that for you.";

        private static readonly CSharpExpressionNumericConverter ExpressionNumericConverter = new CSharpExpressionNumericConverter();

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckField, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckEvent, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckAutoProperty, SyntaxKind.PropertyDeclaration);
        }

        private static void CheckAutoProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            if (propertyDeclaration.Initializer == null
                || !IsAutoProperty(propertyDeclaration))
            {
                return;
            }

            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

            if (propertySymbol != null
                && IsDefaultValueInitializer(propertyDeclaration.Initializer, propertySymbol.Type))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, propertyDeclaration.Initializer.GetLocation(), propertySymbol.Name));
            }
        }

        internal static bool IsAutoProperty(PropertyDeclarationSyntax propertyDeclaration) =>
            propertyDeclaration.AccessorList != null
            && propertyDeclaration.AccessorList.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody() == null);

        private static void CheckEvent(SyntaxNodeAnalysisContext context)
        {
            var field = (EventFieldDeclarationSyntax)context.Node;

            foreach (var eventDeclaration in field.Declaration.Variables.Where(v => v.Initializer != null))
            {
                if (!(context.SemanticModel.GetDeclaredSymbol(eventDeclaration) is IEventSymbol eventSymbol))
                {
                    continue;
                }

                if (IsDefaultValueInitializer(eventDeclaration.Initializer, eventSymbol.Type))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, eventDeclaration.Initializer.GetLocation(), eventSymbol.Name));
                    return;
                }
            }
        }

        private static void CheckField(SyntaxNodeAnalysisContext context)
        {
            var field = (FieldDeclarationSyntax)context.Node;

            foreach (var variableDeclarator in field.Declaration.Variables.Where(v => v.Initializer != null))
            {
                if (context.SemanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol {IsConst: false} fieldSymbol
                    && IsDefaultValueInitializer(variableDeclarator.Initializer, fieldSymbol.Type))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, variableDeclarator.Initializer.GetLocation(), fieldSymbol.Name));
                }
            }
        }

        internal static bool IsDefaultValueInitializer(EqualsValueClauseSyntax initializer, ITypeSymbol type) =>
            IsDefaultExpressionInitializer(initializer)
            || IsReferenceTypeNullInitializer(initializer, type)
            || IsValueTypeDefaultValueInitializer(initializer, type);

        private static bool IsDefaultExpressionInitializer(EqualsValueClauseSyntax initializer) =>
            initializer.Value is DefaultExpressionSyntax;

        private static bool IsReferenceTypeNullInitializer(EqualsValueClauseSyntax initializer, ITypeSymbol type) =>
            type.IsReferenceType
            && CSharpEquivalenceChecker.AreEquivalent(CSharpSyntaxHelper.NullLiteralExpression, initializer.Value);

        private static bool IsValueTypeDefaultValueInitializer(EqualsValueClauseSyntax initializer, ITypeSymbol type)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return CSharpEquivalenceChecker.AreEquivalent(initializer.Value, CSharpSyntaxHelper.FalseLiteralExpression);
                case SpecialType.System_Decimal:
                case SpecialType.System_Double:
                case SpecialType.System_Single:
                    {
                        return ExpressionNumericConverter.TryGetConstantDoubleValue(initializer.Value, out var constantValue) &&
                            Math.Abs(constantValue - default(double)) < double.Epsilon;
                    }
                case SpecialType.System_Char:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_SByte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                    {
                        return ExpressionNumericConverter.TryGetConstantIntValue(initializer.Value, out var constantValue)
                               && constantValue == default;
                    }
                default:
                    return false;
            }
        }
    }
}
