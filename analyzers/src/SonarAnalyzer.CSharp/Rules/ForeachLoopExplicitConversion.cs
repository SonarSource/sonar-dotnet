/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ForeachLoopExplicitConversion : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3217";
        private const string MessageFormat = "Either change the type of '{0}' to '{1}' or iterate on a generic collection of type '{2}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var foreachStatement = (ForEachStatementSyntax)c.Node;
                    var foreachInfo = c.Model.GetForEachStatementInfo(foreachStatement);

                    if (foreachInfo.Equals(default(ForEachStatementInfo))
                        || foreachInfo.ElementConversion.IsImplicit
                        || foreachInfo.ElementConversion.IsUserDefined
                        || !foreachInfo.ElementConversion.Exists
                        || foreachInfo.ElementType.Is(KnownType.System_Object))
                    {
                        return;
                    }

                    c.ReportIssue(
                        Rule,
                        foreachStatement.Type,
                        foreachStatement.Identifier.ValueText,
                        foreachInfo.ElementType.ToMinimalDisplayString(c.Model, foreachStatement.Type.SpanStart),
                        foreachStatement.Type.ToString());
                },
                SyntaxKind.ForEachStatement);
    }
}
