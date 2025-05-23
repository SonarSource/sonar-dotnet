﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggerFieldsShouldBePrivateStaticReadonly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1312";
    private const string MessageFormat = "Make the logger '{0}' private static readonly.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    private static readonly KnownAssembly[] LoggingFrameworks =
        [
            KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
            KnownAssembly.NLog,
            KnownAssembly.Serilog,
            KnownAssembly.Log4Net,
            KnownAssembly.CastleCore,
        ];

    private static readonly ImmutableArray<KnownType> Loggers = ImmutableArray.Create(
        KnownType.Microsoft_Extensions_Logging_ILogger,
        KnownType.Microsoft_Extensions_Logging_ILogger_TCategoryName,
        KnownType.NLog_ILogger,
        KnownType.NLog_ILoggerBase,
        KnownType.NLog_Logger,
        KnownType.Serilog_ILogger,
        KnownType.log4net_ILog,
        KnownType.log4net_Core_ILogger,
        KnownType.Castle_Core_Logging_ILogger);

    private static readonly HashSet<SyntaxKind> InvalidAccessModifiers =
        [
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword,
            SyntaxKind.PublicKeyword
        ];

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(LoggingFrameworks))
            {
                cc.RegisterNodeAction(c =>
                {
                    foreach (var invalid in InvalidFields((FieldDeclarationSyntax)c.Node, c.SemanticModel))
                    {
                        c.ReportIssue(Rule, invalid, invalid.ValueText);
                    }
                },
                SyntaxKind.FieldDeclaration);
            }
        });

    private static IEnumerable<SyntaxToken> InvalidFields(BaseFieldDeclarationSyntax field, SemanticModel model)
    {
        if (field.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword))
            && field.Modifiers.Any(x => x.IsKind(SyntaxKind.ReadOnlyKeyword))
            && field.Modifiers.All(x => !x.IsAnyKind(InvalidAccessModifiers)))
        {
            yield break;
        }

        foreach (var variable in field.Declaration.Variables.Where(ShouldRaise))
        {
            yield return variable.Identifier;
        }

        bool ShouldRaise(VariableDeclaratorSyntax variable) =>
            model.GetDeclaredSymbol(variable) is { } symbol
            && !symbol.ContainingType.IsInterface() // exclude default interface implementation fields
            && symbol.GetSymbolType().DerivesOrImplementsAny(Loggers);
    }
}
