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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggersShouldBeNamedForEnclosingType : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3416";
    private const string MessageFormat = "Update this logger to use its enclosing type.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    private static readonly KnownAssembly[] SupportedFrameworks =
        [
            KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
            KnownAssembly.NLog,
            KnownAssembly.Log4Net,
        ];

    private static readonly ImmutableArray<KnownType> Loggers = ImmutableArray.Create(
        KnownType.Microsoft_Extensions_Logging_ILogger,
        KnownType.Microsoft_Extensions_Logging_ILogger_TCategoryName,
        KnownType.NLog_Logger,
        KnownType.NLog_ILogger,
        KnownType.NLog_ILoggerBase,
        KnownType.log4net_ILog);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(SupportedFrameworks))
            {
                cc.RegisterNodeAction(Process, SyntaxKind.InvocationExpression);
            }
        });

    private static void Process(SonarSyntaxNodeReportingContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.GetName() is "GetLogger" or "CreateLogger"
            && EnclosingTypeNode(invocation) is { } enclosingType // filter out top-level statements, choose new() first and then enclosing type
            && ExtractArgument(invocation) is { } argument
            && context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
            && IsValidMethod(method)
            && !MatchesEnclosingType(argument, enclosingType, context.Model))
        {
            context.ReportIssue(Rule, argument);
        }
    }

    private static SyntaxNode EnclosingTypeNode(InvocationExpressionSyntax invocation)
    {
        var ancestors = invocation.Ancestors();
        return (SyntaxNode)ancestors.OfType<ObjectCreationExpressionSyntax>().FirstOrDefault() // prioritize new() over enclosing type
                    ?? ancestors.OfType<TypeDeclarationSyntax>().FirstOrDefault();
    }

    // Extracts T for generic argument, nameof or typeof expressions
    private static SyntaxNode ExtractArgument(InvocationExpressionSyntax invocation)
    {
        // CreateLogger<T>
        if (ExtractGeneric(invocation) is { } generic)
        {
            return generic;
        }
        else if (invocation.ArgumentList?.Arguments.Count == 1)
        {
            return invocation.ArgumentList.Arguments[0].Expression switch
            {
                TypeOfExpressionSyntax typeOf => typeOf.Type,                                   // CreateLogger(typeof(T))
                MemberAccessExpressionSyntax memberAccess => ExtractTypeOfName(memberAccess),   // CreateLogger(typeof(T).Name)
                InvocationExpressionSyntax innerInvocation => ExtractNameOf(innerInvocation),   // CreateLogger(nameof(T))
                _ => null
            };
        }
        else
        {
            return null;
        }
    }

    private static bool IsValidMethod(IMethodSymbol method)
    {
        return Matches(KnownType.Microsoft_Extensions_Logging_ILoggerFactory, true)
            || Matches(KnownType.Microsoft_Extensions_Logging_LoggerFactoryExtensions, false)
            || Matches(KnownType.NLog_LogManager, false)
            || Matches(KnownType.NLog_LogFactory, true)
            || Matches(KnownType.log4net_LogManager, false)
            || MatchesGeneric();

        bool Matches(KnownType containingType, bool checkDerived) =>
            method.HasContainingType(containingType, checkDerived)
            && method.Parameters.Length == 1
            && method.Parameters[0].Type.IsAny(KnownType.System_String, KnownType.System_Type);

        bool MatchesGeneric() =>
            method.ContainingType.Is(KnownType.Microsoft_Extensions_Logging_LoggerFactoryExtensions)
            && method.TypeParameters.Length == 1;
    }

    private static bool MatchesEnclosingType(SyntaxNode argument, SyntaxNode enclosingNode, SemanticModel model) =>
        model.GetTypeInfo(argument).Type is { } argumentType
        && EnclosingTypeSymbol(model, enclosingNode) is { } enclosingType
        && (enclosingType.Equals(argumentType)
            || argumentType.TypeKind is TypeKind.TypeParameter  // Do not raise on CreateLogger<T> if T is not concrete
            || enclosingType.DerivesOrImplementsAny(Loggers));  // Do not raise on Decorator pattern

    private static ITypeSymbol EnclosingTypeSymbol(SemanticModel model, SyntaxNode enclosingNode) =>
        (model.GetDeclaredSymbol(enclosingNode) ?? model.GetSymbolInfo(enclosingNode).Symbol).GetSymbolType();

    private static SyntaxNode ExtractGeneric(InvocationExpressionSyntax invocation)
    {
        var genericName = invocation.Expression switch
        {
            GenericNameSyntax g => g, // CreateLogger<T>
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name as GenericNameSyntax, // A..B.CreateLogger<T>
            _ => null
        };

        return genericName?.TypeArgumentList?.Arguments.Count == 1
            ? genericName.TypeArgumentList.Arguments[0]
            : null;
    }

    private static SyntaxNode ExtractTypeOfName(MemberAccessExpressionSyntax memberAccess) =>
        memberAccess.Expression is TypeOfExpressionSyntax typeOf
        && memberAccess.GetName() is "Name" or "FullName" or "AssemblyQualifiedName"
            ? typeOf.Type
            : null;

    private static SyntaxNode ExtractNameOf(InvocationExpressionSyntax invocation) =>
        invocation.NameIs("nameof") && invocation.ArgumentList?.Arguments.Count == 1
            ? invocation.ArgumentList?.Arguments[0].Expression
            : null;
}
