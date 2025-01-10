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

using SonarAnalyzer.ShimLayer.AnalysisContext;

namespace SonarAnalyzer.AnalysisContext;

public sealed class SonarSymbolStartAnalysisContext : SonarAnalysisContextBase<SymbolStartAnalysisContextWrapper>
{
    public override Compilation Compilation => Context.Compilation;
    public override AnalyzerOptions Options => Context.Options;
    public override CancellationToken Cancel => Context.CancellationToken;
    public ISymbol Symbol => Context.Symbol;

    internal SonarSymbolStartAnalysisContext(SonarAnalysisContext analysisContext, SymbolStartAnalysisContextWrapper context) : base(analysisContext, context) { }

    public void RegisterCodeBlockAction(Action<SonarCodeBlockReportingContext> action) =>
        Context.RegisterCodeBlockAction(x => action(new(AnalysisContext, x)));

    public void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<SonarCodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct =>
        Context.RegisterCodeBlockStartAction<TLanguageKindEnum>(x => action(new(AnalysisContext, x)));

    public void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds) =>
        // https://github.com/SonarSource/sonar-dotnet/issues/8878
        throw new NotImplementedException("SonarOperationAnalysisContext wrapper type not implemented.");

    public void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action) =>
        // https://github.com/SonarSource/sonar-dotnet/issues/8878
        throw new NotImplementedException("SonarOperationBlockAnalysisContext wrapper type not implemented.");

    public void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action) =>
        // https://github.com/SonarSource/sonar-dotnet/issues/8878
        throw new NotImplementedException("SonarOperationBlockStartAnalysisContext wrapper type not implemented.");

    public void RegisterSymbolEndAction(Action<SonarSymbolReportingContext> action) =>
        Context.RegisterSymbolEndAction(x => action(new(AnalysisContext, x)));

    public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SonarSyntaxNodeReportingContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct =>
        Context.RegisterSyntaxNodeAction(x => action(new(AnalysisContext, x)), syntaxKinds);
}
