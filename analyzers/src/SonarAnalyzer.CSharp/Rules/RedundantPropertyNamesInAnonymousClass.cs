/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public sealed class RedundantPropertyNamesInAnonymousClass : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3441";
        private const string MessageFormat = "Remove the redundant '{0} ='.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var anonymousObjectCreation = (AnonymousObjectCreationExpressionSyntax)c.Node;

                    foreach (var initializer in GetRedundantInitializers(anonymousObjectCreation.Initializers))
                    {
                        c.ReportIssue(rule, initializer.NameEquals, initializer.NameEquals.Name.Identifier.ValueText);
                    }
                },
                SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static IEnumerable<AnonymousObjectMemberDeclaratorSyntax> GetRedundantInitializers(
            IEnumerable<AnonymousObjectMemberDeclaratorSyntax> initializers)
        {
            var initializersToReportOn = new List<AnonymousObjectMemberDeclaratorSyntax>();

            foreach (var initializer in initializers.Where(initializer => initializer.NameEquals != null))
            {
                if (initializer.Expression is IdentifierNameSyntax identifier &&
                    identifier.Identifier.ValueText == initializer.NameEquals.Name.Identifier.ValueText)
                {
                    initializersToReportOn.Add(initializer);
                }
            }

            return initializersToReportOn;
        }
    }
}
