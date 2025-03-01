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

namespace SonarAnalyzer.Core.Rules;

public abstract class LooseFilePermissionsBase<TSyntaxKind> : HotspotDiagnosticAnalyzer
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S2612";
    protected const string Everyone = "Everyone";
    protected const string MessageFormat = "Make sure this permission is safe.";

    protected readonly DiagnosticDescriptor Rule;

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract void VisitAssignments(SonarSyntaxNodeReportingContext context);
    protected abstract void VisitInvocations(SonarSyntaxNodeReportingContext context);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected LooseFilePermissionsBase(IAnalyzerConfiguration configuration) : base(configuration) =>
        Rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(c =>
        {
            if (!IsEnabled(c.Options))
            {
                return;
            }

            c.RegisterNodeAction(Language.GeneratedCodeRecognizer, VisitInvocations, Language.SyntaxKind.InvocationExpression);
            c.RegisterNodeAction(Language.GeneratedCodeRecognizer, VisitAssignments, Language.SyntaxKind.IdentifierName);
        });

    protected bool IsFileAccessPermissions(SyntaxNode node, SemanticModel model) =>
        Language.Syntax.NodeIdentifier(node) is { } identifier
        && LooseFilePermissionsConfig.WeakFileAccessPermissions.Contains(identifier.Text)
        && node.IsKnownType(KnownType.Mono_Unix_FileAccessPermissions, model);
}
