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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class MethodOverrideChangedDefaultValueCodeFix : SonarCodeFix
    {
        internal const string TitleGeneral = "Synchronize default parameter value";
        internal const string TitleExplicitInterface = "Remove default parameter value from explicit interface implementation";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(MethodOverrideChangedDefaultValue.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);
            var parameter = syntaxNode?.FirstAncestorOrSelf<ParameterSyntax>();
            if (parameter == null)
            {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.Cancel).ConfigureAwait(false);
            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);
            if (!(parameterSymbol?.ContainingSymbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            ParameterSyntax newParameter;
            string title;

            if (methodSymbol.ExplicitInterfaceImplementations.Any())
            {
                newParameter = parameter.WithDefault(null);
                title = TitleExplicitInterface;
            }
            else
            {
                var index = methodSymbol.Parameters.IndexOf(parameterSymbol);
                var overriddenMember = methodSymbol.GetOverriddenMember() ?? methodSymbol.GetInterfaceMember();
                if (index == -1 ||
                    overriddenMember == null)
                {
                    return;
                }

                var overriddenParameter = overriddenMember.Parameters[index];

                if (!TryGetNewParameterSyntax(parameter, overriddenParameter, out newParameter))
                {
                    return;
                }
                title = TitleGeneral;
            }

            RegisterCodeFix(context, root, parameter, newParameter, title);
        }

        private static void RegisterCodeFix(SonarCodeFixContext context, SyntaxNode root, ParameterSyntax parameter, ParameterSyntax newParameter, string codeFixTitle) =>
            context.RegisterCodeFix(
                codeFixTitle,
                c =>
                {
                    var newRoot = root.ReplaceNode(
                        parameter,
                        newParameter.WithTriviaFrom(parameter));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

        private static bool TryGetNewParameterSyntax(ParameterSyntax parameter, IParameterSymbol overriddenParameter, out ParameterSyntax newParameterSyntax)
        {
            if (!overriddenParameter.HasExplicitDefaultValue)
            {
                newParameterSyntax = parameter.WithDefault(null).WithAdditionalAnnotations(Formatter.Annotation);
                return true;
            }

            var defaultSyntax = (overriddenParameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ParameterSyntax)?.Default;
            if (defaultSyntax != null)
            {
                newParameterSyntax = parameter.WithDefault(defaultSyntax.WithoutTrivia().WithAdditionalAnnotations(Formatter.Annotation));
                return true;
            }

            newParameterSyntax = null;
            return false;
        }
    }
}
