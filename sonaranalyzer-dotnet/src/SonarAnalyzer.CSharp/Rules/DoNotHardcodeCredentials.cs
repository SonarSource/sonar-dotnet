/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Text.RegularExpressions;
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
    public sealed class DoNotHardcodeCredentials : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2068";
        private const string MessageFormat = "Remove this hard-coded password.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> passwordVariants = new HashSet<string>
        {
            "password", "passwd",
            "achinsinsi", "adgangskode", "codice", "contrasena", "contrasenya", "contrasinal",
            "cynfrinair", "facal-faire", "facalfaire", "fjaleklaim", "focalfaire", "geslo", "haslo",
            "heslo", "iphasiwedi", "jelszo", "kalmarsirri", "katalaluan", "katasandi", "kennwort",
            "kode", "kupuhipa", "loluszais", "losen", "losenord", "lozinka", "lykilorth", "mathkau",
            "modpas", "motdepasse", "olelohuna", "oroigbaniwole", "parol", "parola", "parole", "parool",
            "pasahitza", "pasiwedhi", "passe", "passord", "passwort", "passwuert", "paswoodu", "phasewete",
            "salasana", "sandi", "senha", "sifre", "sifreya", "slaptazois", "tenimiafina", "upufaalilolilo",
            "wachtwoord", "wachtwurd", "wagwoord"
        };

        private static readonly Regex passwordValuePattern
            = new Regex(string.Format(@"\b({0})\b[:=]\S", string.Join("|", passwordVariants)),
                        RegexOptions.Compiled);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(VerifyDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeActionInNonGenerated(VerifyAssignment, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void VerifyAssignment(SyntaxNodeAnalysisContext context)
        {
            if (context.IsTest())
            {
                return;
            }

            var assignment = context.Node as AssignmentExpressionSyntax;
            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                !assignment.Left.IsKnownType(KnownType.System_String, context.SemanticModel) ||
                !assignment.Right.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return;
            }

            string variableName = (assignment.Left as IdentifierNameSyntax)?.Identifier.ValueText;
            string variableValue = (assignment.Right as LiteralExpressionSyntax)?.Token.ValueText;

            if (DoesContainPassword(variableName, variableValue))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, assignment.GetLocation()));
            }
        }

        private static void VerifyDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsTest())
            {
                return;
            }

            var declaration = context.Node as VariableDeclarationSyntax;
            foreach (var variableDeclarator in declaration.Variables)
            {
                if (variableDeclarator.IsDeclarationKnownType(KnownType.System_String, context.SemanticModel) &&
                    DoesContainPassword(variableDeclarator))
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, variableDeclarator.GetLocation()));
                }
            }
        }

        private static bool DoesContainPassword(VariableDeclaratorSyntax variableDeclarator)
        {
            string variableName = variableDeclarator?.Identifier.ValueText;
            var literalExpression = variableDeclarator?.Initializer?.Value as LiteralExpressionSyntax;
            if (literalExpression == null ||
                !literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            return DoesContainPassword(variableName, literalExpression.Token.ValueText);
        }

        private static bool DoesContainPassword(string variableName, string variableValue)
        {
            if (string.IsNullOrWhiteSpace(variableValue))
            {
                return false;
            }

            if (passwordVariants.Contains(variableName?.ToLowerInvariant()))
            {
                return true;
            }

            return passwordValuePattern.IsMatch(variableValue.ToLowerInvariant());
        }
    }
}
