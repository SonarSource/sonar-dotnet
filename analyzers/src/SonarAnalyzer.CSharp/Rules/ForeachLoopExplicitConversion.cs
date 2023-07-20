/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
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
                    var foreachInfo = c.SemanticModel.GetForEachStatementInfo(foreachStatement);

                    if (foreachInfo.Equals(default(ForEachStatementInfo))
                        || foreachInfo.ElementConversion.IsImplicit
                        || foreachInfo.ElementConversion.IsUserDefined
                        || !foreachInfo.ElementConversion.Exists
                        || foreachInfo.ElementType.Is(KnownType.System_Object))
                    {
                        return;
                    }

                    c.ReportIssue(
                        CreateDiagnostic(Rule,
                            foreachStatement.Type.GetLocation(),
                            foreachStatement.Identifier.ValueText,
                            foreachInfo.ElementType.ToMinimalDisplayString(c.SemanticModel, foreachStatement.Type.SpanStart),
                            foreachStatement.Type.ToString()));
                },
                SyntaxKind.ForEachStatement);
    }
}
