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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MemberInitializedToDefault : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3052";
        private const string MessageFormat = "Remove this initialization to '{0}', the compiler will do that for you.";
        private const string Zero = "Zero";

        private static readonly CSharpExpressionNumericConverter ExpressionNumericConverter = new CSharpExpressionNumericConverter();

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckField, SyntaxKind.FieldDeclaration);
            context.RegisterNodeAction(CheckEvent, SyntaxKind.EventFieldDeclaration);
            context.RegisterNodeAction(CheckAutoProperty, SyntaxKind.PropertyDeclaration);
        }

        private static void CheckAutoProperty(SonarSyntaxNodeReportingContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            if (propertyDeclaration.Initializer == null
                || !propertyDeclaration.IsAutoProperty())
            {
                return;
            }

            var propertySymbol = context.Model.GetDeclaredSymbol(propertyDeclaration);

            if (propertySymbol != null
                && IsDefaultValueInitializer(propertyDeclaration.Initializer, propertySymbol.Type))
            {
                context.ReportIssue(Rule, propertyDeclaration.Initializer, propertySymbol.Name);
            }
        }

        private static void CheckEvent(SonarSyntaxNodeReportingContext context)
        {
            var field = (EventFieldDeclarationSyntax)context.Node;

            foreach (var eventDeclaration in field.Declaration.Variables.Where(v => v.Initializer != null))
            {
                if (!(context.Model.GetDeclaredSymbol(eventDeclaration) is IEventSymbol eventSymbol))
                {
                    continue;
                }

                if (IsDefaultValueInitializer(eventDeclaration.Initializer, eventSymbol.Type))
                {
                    context.ReportIssue(Rule, eventDeclaration.Initializer, eventSymbol.Name);
                    return;
                }
            }
        }

        private static void CheckField(SonarSyntaxNodeReportingContext context)
        {
            var field = (FieldDeclarationSyntax)context.Node;

            foreach (var variableDeclarator in field.Declaration.Variables.Where(v => v.Initializer != null))
            {
                if (context.Model.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol {IsConst: false} fieldSymbol
                    && IsDefaultValueInitializer(variableDeclarator.Initializer, fieldSymbol.Type))
                {
                    context.ReportIssue(Rule, variableDeclarator.Initializer, fieldSymbol.Name);
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
            && CSharpEquivalenceChecker.AreEquivalent(SyntaxConstants.NullLiteralExpression, initializer.Value);

        private static bool IsValueTypeDefaultValueInitializer(EqualsValueClauseSyntax initializer, ITypeSymbol type)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return CSharpEquivalenceChecker.AreEquivalent(initializer.Value, SyntaxConstants.FalseLiteralExpression);
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
                        if (initializer.Value is MemberAccessExpressionSyntax memberAccess
                            && memberAccess.Name.Identifier.Text == Zero)
                        {
                            return true;
                        }
                        else if (ObjectCreationFactory.TryCreate(initializer.Value) is { } objectCreation)
                        {
                            var argCount = objectCreation.ArgumentList?.Arguments.Count;
                            if (argCount == null || argCount == 0)
                            {
                                return true;
                            }

                            return ExpressionNumericConverter.TryGetConstantIntValue(objectCreation.ArgumentList.Arguments.First().Expression, out var ctorParameter)
                                   && ctorParameter == default;
                        }
                        else
                        {
                            return ExpressionNumericConverter.TryGetConstantIntValue(initializer.Value, out var constantValue)
                                   && constantValue == default;
                        }
                    }
                default:
                    return false;
            }
        }
    }
}
