using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer
{
    public static class SyntaxNodeExtension
    {
        public static string Dump(this SyntaxNode node)
        {
            return Regex.Replace(node.ToString(), @"\t|\n|\r", " ");
        }
    }

    public class MLIRExporter
    {
        public MLIRExporter(TextWriter w, SemanticModel model, bool withLoc)
        {
            writer = w;
            semanticModel = model;
            exportsLocations = withLoc;
        }

        public void ExportFunction(MethodDeclarationSyntax method)
        {
            if (method.Body == null)
            {
                return;
            }

            if (IsTooClomplexForMLIROrTheCFG(method))
            {
                writer.WriteLine($"// Skipping function {method.Identifier.ValueText}{GetAnonymousArgumentsString(method)}, it contains poisonous unsupported syntaxes");
                writer.WriteLine();
                return;
            }
            blockCounter = 0;
            blockMap.Clear();

            opCounter = 0;
            opMap.Clear();

            var returnType = HasNoReturn(method) ?
                "()" :
                MLIRType(method.ReturnType);
            writer.WriteLine($"func @{method.Identifier.ValueText}{GetAnonymousArgumentsString(method)} -> {returnType} {GetLocation(method)} {{");
            CreateEntryBlock(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            foreach (var block in cfg.Blocks)
            {
                ExportBlock(block, block == cfg.EntryBlock, method);
            }
            writer.WriteLine("}");
        }

        private bool IsTooClomplexForMLIROrTheCFG(MethodDeclarationSyntax method)
        {
            return method.DescendantNodes().Any(n =>
            n.IsKind(SyntaxKind.ForEachStatement) ||
            n.IsKind(SyntaxKind.AwaitExpression) ||
            n.IsKind(SyntaxKind.YieldReturnStatement) ||
            n.IsKind(SyntaxKind.YieldBreakStatement) ||
            n.IsKind(SyntaxKind.TryStatement) ||
            n.IsKind(SyntaxKind.UsingStatement) ||
            n.IsKind(SyntaxKind.LogicalAndExpression) ||
            n.IsKind(SyntaxKind.LogicalOrExpression) ||
            n.IsKind(SyntaxKind.ConditionalExpression) ||
            n.IsKind(SyntaxKind.ConditionalAccessExpression) ||
            n.IsKind(SyntaxKind.CoalesceExpression) ||
            n.IsKind(SyntaxKind.SwitchStatement) ||
            n.IsKind(SyntaxKind.ParenthesizedLambdaExpression) ||
            n.IsKind(SyntaxKind.SimpleLambdaExpression) ||
            n.IsKind(SyntaxKind.FixedStatement) ||
            n.IsKind(SyntaxKind.CheckedStatement) ||
            n.IsKind(SyntaxKind.CheckedExpression) ||
            n.IsKind(SyntaxKind.UncheckedExpression) ||
            n.IsKind(SyntaxKind.UncheckedStatement)
            );
        }

        private void CreateEntryBlock(MethodDeclarationSyntax method)
        {
            writer.WriteLine($"^entry {GetArgumentsString(method)}:");
            foreach (var param in method.ParameterList.Parameters)
            {
                if(string.IsNullOrEmpty(param.Identifier.ValueText))
                {
                    // An unnamed parameter cannot be used inside the function
                    continue;
                }
                var id = OpId(param);
                writer.WriteLine($"%{id} = cbde.alloca {MLIRType(param)} {GetLocation(param)}");
                writer.WriteLine($"cbde.store %{param.Identifier.ValueText}, %{id} : memref<{MLIRType(param)}> {GetLocation(param)}");
            }
            writer.WriteLine("br ^0");
            writer.WriteLine();
        }

        private bool HasNoReturn(MethodDeclarationSyntax method)
        {
            return semanticModel.GetTypeInfo(method.ReturnType).Type.SpecialType == SpecialType.System_Void;
        }

        private void ExportBlock(Block block, bool isEntryBlock, MethodDeclarationSyntax parentMethod)
        {
            if (block is ExitBlock && !HasNoReturn(parentMethod))
            {
               // If the method returns, it will have an explicit return, no need for this spurious block
                return;
            }
            else
            {
                writer.WriteLine($"^{BlockId(block)}: // {block.GetType().Name}"); // TODO: Block arguments...
            }
            // MLIR encodes blocks relationships in operations, not in blocks themselves
            foreach(var op in block.Instructions)
            {
                ExtractInstruction(op);
            }
            // MLIR encodes blocks relationships in operations, not in blocks themselves
            // So we need to add the corresponding operations at the end...
            switch (block)
            {
                case JumpBlock jb:
                    switch (jb.JumpNode)
                    {
                        case ReturnStatementSyntax ret:
                            if (ret.Expression == null)
                            {
                                writer.WriteLine($"return {GetLocation(ret)}");
                            }
                            else
                            {
                                writer.WriteLine($"return %{OpId(ret.Expression)} : {MLIRType(ret.Expression)} {GetLocation(ret)}");
                            }
                            break;
                        case BreakStatementSyntax breakStmt:
                            writer.WriteLine($"br ^{BlockId(jb.SuccessorBlock)} {GetLocation(breakStmt)} // break");
                            break;
                        case ContinueStatementSyntax continueStmt:
                            writer.WriteLine($"br ^{BlockId(jb.SuccessorBlock)} {GetLocation(continueStmt)} // continue");
                            break;
                        case ThrowStatementSyntax throwStmt:
                            // TODO : Should we transfert to a catch block if we are inside a try/catch?
                            writer.WriteLine($"cbde.throw %{OpId(throwStmt.Expression)} :  {MLIRType(throwStmt.Expression)} {GetLocation(throwStmt)}");
                            break;
                        default:
                            Debug.Assert(false, "Unknown kind of JumpBlock");
                            break;
                    }
                    break;
                case BinaryBranchBlock bbb:
                    var cond = GetCondition(bbb);
                    if (null == cond)
                    {
                        Debug.Assert(bbb.BranchingNode.Kind() == SyntaxKind.ForStatement);
                        writer.WriteLine($"br ^{BlockId(bbb.TrueSuccessorBlock)}");
                    }
                    else
                    {
                        writer.WriteLine($"cond_br %{OpId(cond)}, ^{BlockId(bbb.TrueSuccessorBlock)}, ^{BlockId(bbb.FalseSuccessorBlock)} {GetLocation(cond)}");
                    }
                    /*
                     * Up to now, we do exactly the same for all cases that may have created a BinaryBranchBlock
                     * maybe later, depending on the reason (if vs for?) we'll do something different
                     *
                    var condStatement = bbb.BranchingNode.Parent;
                    switch (condStatement.Kind())
                    {
                        case SyntaxKind.ConditionalExpression: // a ? b : c
                            var cond = condStatement as ConditionalExpressionSyntax;
                            writer.WriteLine($"cond_br %{OpId(cond.Condition)}, ^{BlockId(bbb.TrueSuccessorBlock)}, ^{BlockId(bbb.FalseSuccessorBlock)}");
                            break;
                        case SyntaxKind.IfStatement:
                            var ifCond = condStatement as IfStatementSyntax;
                            writer.WriteLine($"cond_br %{OpId(ifCond.Condition)}, ^{BlockId(bbb.TrueSuccessorBlock)}, ^{BlockId(bbb.FalseSuccessorBlock)}");
                            break;
                        case SyntaxKind.ForEachStatement:
                        case SyntaxKind.CoalesceExpression:
                        case SyntaxKind.ConditionalAccessExpression:
                        case SyntaxKind.LogicalAndExpression:
                        case SyntaxKind.LogicalOrExpression:
                        case SyntaxKind.ForStatement:
                        case SyntaxKind.CatchFilterClause:
                        default:
                            writer.WriteLine($"// Unhandled branch {bbb.BranchingNode.Kind().ToString()}");
                            break;

                    }*/
                    break;
                case SimpleBlock sb:
                    writer.WriteLine($"br ^{BlockId(sb.SuccessorBlock)}");
                    break;
                case ExitBlock eb:
                    // If we reach this point, it means the function has no return, we must manually add one
                    writer.WriteLine("return");
                    break;

            }
            writer.WriteLine();
        }

        private SyntaxNode GetCondition(BinaryBranchBlock bbb)
        {
            // For an if or a while, bbb.BranchingNode represent the condition, not the statement that holds the condition
            // For a for, bbb.BranchingNode represents the for. Since for is a statement, not an expression, if we
            // see a for, we know it's at the top level of the expression tree, so it cannot be a for inside of a if condition
            switch (bbb.BranchingNode.Kind())
            {
                case SyntaxKind.ForStatement:
                    var forStmt = bbb.BranchingNode as ForStatementSyntax;
                    return forStmt.Condition;
                case SyntaxKind.ForEachStatement:
                    Debug.Assert(false, "Not ready to handle those");
                    return null;
                default:
                    return bbb.BranchingNode;
            }
        }

        private string GetArgumentsString(MethodDeclarationSyntax method)
        {
            if (method.ParameterList.Parameters.Count == 0)
            {
                return string.Empty;
            }
            int paramCount = 0;
            var args = method.ParameterList.Parameters.Select(
                p => {
                    ++paramCount;
                    var paramName = string.IsNullOrEmpty(p.Identifier.ValueText) ?
                        ".param" + paramCount.ToString() :
                        p.Identifier.ValueText;
                    return $"%{paramName} : {MLIRType(p)}";
                }
                );
            return '(' + string.Join(", ", args) + ')';
        }

        private string GetAnonymousArgumentsString(MethodDeclarationSyntax method)
        {
            var args = method.ParameterList.Parameters.Select(p => MLIRType(p));
            return '(' + string.Join(", ", args) + ')';
        }

        private string MLIRType(ParameterSyntax p)
        {
            var symbolType = semanticModel.GetDeclaredSymbol(p).GetSymbolType();
            return symbolType == null ? "none" : MLIRType(symbolType);
        }

        private string MLIRType(ExpressionSyntax e) =>
            e.RemoveParentheses().Kind() == SyntaxKind.NullLiteralExpression ? "none" : MLIRType(semanticModel.GetTypeInfo(e).Type);

        private string MLIRType(VariableDeclaratorSyntax v) => MLIRType(semanticModel.GetDeclaredSymbol(v).GetSymbolType());

        private string MLIRType(ITypeSymbol csType)
        {
            Debug.Assert(csType != null);
            if (csType.SpecialType == SpecialType.System_Boolean)
            {
                return "i1";
            }
            else if (csType.SpecialType == SpecialType.System_Int32)
            {
                return "i32";
            }
            else
            {
                return "none";
            }
        }

        private bool IsTypeKnown(ITypeSymbol csType)
        {
            return csType != null &&
                (csType.SpecialType == SpecialType.System_Boolean ||
                csType.SpecialType == SpecialType.System_Int32);
        }

        private ExpressionSyntax getAssignmentValue(ExpressionSyntax rhs)
        {
            while (rhs.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                rhs = (rhs as AssignmentExpressionSyntax).Right;
            }
            return rhs;
        }

        private void ExtractInstruction(SyntaxNode op)
        {
            switch (op.Kind())
            {
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                    ExtractBinaryExpression(op);
                    break;
                case SyntaxKind.NumericLiteralExpression:
                    {
                        var lit = op as LiteralExpressionSyntax;
                        writer.WriteLine($"%{OpId(op)} = constant {lit.Token.ValueText} : {MLIRType(lit)} {GetLocation(op)}");
                        break;
                    }
                case SyntaxKind.EqualsExpression:
                    ExportComparison("eq", op);
                    break;
                case SyntaxKind.NotEqualsExpression:
                    ExportComparison("ne", op);
                    break;
                case SyntaxKind.GreaterThanExpression:
                    ExportComparison("sgt", op);
                    break;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    ExportComparison("sge", op);
                    break;
                case SyntaxKind.LessThanExpression:
                    ExportComparison("slt", op);
                    break;
                case SyntaxKind.LessThanOrEqualExpression:
                    ExportComparison("sle", op);
                    break;
                case SyntaxKind.IdentifierName:
                    ExportIdentifierName(op);
                    break;
                case SyntaxKind.VariableDeclarator:
                    {
                        var decl = op as VariableDeclaratorSyntax;
                        var id = OpId(decl);
                        if (!IsTypeKnown(semanticModel.GetDeclaredSymbol(decl).GetSymbolType()))
                        {
                            // No need to write the variable, all references to it will be replaced by "unknown"
                            return;
                        }
                        writer.WriteLine($"%{id} = cbde.alloca {MLIRType(decl)} {GetLocation(decl)} // {decl.Identifier.ValueText}");
                        if (decl.Initializer != null)
                        {
                            if (!SupportedTypes(decl.Initializer.Value))
                            {
                                writer.WriteLine("// Initialized with unknown data");
                                break;
                            }
                            var value = getAssignmentValue(decl.Initializer.Value);
                            writer.WriteLine($"cbde.store %{OpId(value)}, %{id} : memref<{MLIRType(decl)}> {GetLocation(decl)}");
                        }
                    }
                    break;
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        ExportSimpleAssignment(op);
                        break;
                    }
                default:
                    if (op is ExpressionSyntax expr && !(op.Kind() is SyntaxKind.NullLiteralExpression))
                    {
                        var exprType = semanticModel.GetTypeInfo(expr).Type;
                        if (exprType == null)
                        {
                            // Some intermediate expressions have no type (member access, initialization of member...)
                            // and therefore, they have no real value associated to them, we can just ignore them
                            break;
                        }
                        writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(exprType)} {GetLocation(op)} // {op.Dump()} ({op.Kind()})");
                    }
                    else
                    {
                        writer.WriteLine($"%{OpId(op)} = cbde.unknown : none {GetLocation(op)} // {op.Dump()} ({op.Kind()})");
                    }
                    break;
            }
        }

        private void ExportSimpleAssignment(SyntaxNode op)
        {
            var assign = op as AssignmentExpressionSyntax;
            if (!SupportedTypes(assign))
            {
                return;
            }
            var lhs = semanticModel.GetSymbolInfo(assign.Left).Symbol.DeclaringSyntaxReferences[0].GetSyntax();
            var rhsType = semanticModel.GetTypeInfo(assign.Right).Type;
            string rhsId;
            if (rhsType.Kind == SymbolKind.ErrorType)
            {
                rhsId = UniqueOpId();
                writer.WriteLine($"%{rhsId} = cbde.unknown  : {MLIRType(assign)}");
            }
            else
            {
                rhsId = OpId(getAssignmentValue(assign.Right));
            }
            writer.WriteLine($"cbde.store %{rhsId}, %{OpId(lhs)} : memref<{MLIRType(assign)}> {GetLocation(op)}");
        }

        private void ExportIdentifierName(SyntaxNode op)
        {
            var id = op as IdentifierNameSyntax;
            var declSymbol = semanticModel.GetSymbolInfo(id).Symbol;
            if (declSymbol == null)
            {
                // In case of an unresolved call, just skip it
                writer.WriteLine($"// Unresolved: {id.Identifier.ValueText}");
                return;
            }
            if (declSymbol.DeclaringSyntaxReferences.Length == 0)
            {
                // The entity comes from another assembly... We can ignore it, it's not a variable
                // TODO : Check what happens for external constants, fields and properties...
                return;
            }
            var decl = declSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            if (decl == null ||                    // Not sure if we can be in this situation...
                decl is MethodDeclarationSyntax || // We will fetch the function only when looking at the function call itself
                decl is ClassDeclarationSyntax     // In "Class.member", we are not interested in the "Class" part
                )
            {
                // We will fetch the function only when looking at the function call itself, we just skip the identifier
                return;
            }

            if (declSymbol is IFieldSymbol fieldSymbol && fieldSymbol.HasConstantValue)
            {
                writer.WriteLine($"%{OpId(op)} = constant {fieldSymbol.ConstantValue.ToString()} : {MLIRType(fieldSymbol.Type)} {GetLocation(op)}");
                return;
            }
            else if (declSymbol is IFieldSymbol || !SupportedTypes(id))
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(id)} {GetLocation(op)} // Variable of unknown type {id.Identifier.ValueText}");
                return;
            }
            writer.WriteLine($"%{OpId(op)} = cbde.load %{OpId(decl)} : memref<{MLIRType(id)}> {GetLocation(op)}");
        }

        private void ExtractBinaryExpression(SyntaxNode op)
        {
            var binExpr = op as BinaryExpressionSyntax;
            if (!SupportedTypes(binExpr.Left, binExpr.Right,binExpr))
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(binExpr)} {GetLocation(binExpr)} // Binary expression on unsupported types {op.Dump()}");
                return;
            }
            // TODO : C#8 : Use switch expression
            string opName;
            switch (binExpr.Kind())
            {
                case SyntaxKind.AddExpression: opName = "addi"; break;
                case SyntaxKind.SubtractExpression: opName = "subi"; break;
                case SyntaxKind.MultiplyExpression: opName = "muli"; break;
                case SyntaxKind.DivideExpression:  opName = "divis"; break;
                case SyntaxKind.ModuloExpression:  opName = "remis"; break;
                default:
                    {
                        writer.WriteLine($"%{OpId(op)} = cbde.unknown : {MLIRType(binExpr)} {GetLocation(binExpr)} // Unknown operator {op.Dump()}");
                        return;
                    }
            }

            writer.WriteLine($"%{OpId(op)} = {opName} %{OpId(binExpr.Left)}, %{OpId(binExpr.Right)} : {MLIRType(binExpr)} {GetLocation(binExpr)}");
        }

        private bool SupportedTypes(params ExpressionSyntax [] exprs)
        {
            return exprs.All(expr => IsTypeKnown(semanticModel.GetTypeInfo(expr).Type));
        }

        private void ExportComparison(string compName, SyntaxNode op)
        {
            var binExpr = op as BinaryExpressionSyntax;
            if (!SupportedTypes(binExpr.Left, binExpr.Right))
            {
                writer.WriteLine($"%{OpId(op)} = cbde.unknown : i1  {GetLocation(binExpr)} // comparison of unknown type: {op.Dump()}");
                return;
            }
            // The type is the type of the operands, not of the result, which is always i1
            writer.WriteLine($"%{OpId(op)} = cmpi \"{compName}\", %{OpId(binExpr.Left)}, %{OpId(binExpr.Right)} : {MLIRType(binExpr.Left)} {GetLocation(binExpr)}");

        }

        private string GetLocation(SyntaxNode node)
        {
            if (!exportsLocations)
            {
                return string.Empty;
            }
            // TODO: We should decide which of GetLineSpan or GetMappedLineSpan is better to use
            var loc = node.GetLocation().GetLineSpan();
            var location = $"loc(\"{loc.Path}\"" +
                $" :{loc.StartLinePosition.Line}" +
                $" :{loc.StartLinePosition.Character})";

            return location;
        }

        private readonly TextWriter writer;
        private readonly SemanticModel semanticModel;
        private readonly bool exportsLocations;
        private readonly Dictionary<Block, int> blockMap = new Dictionary<Block, int>();
        private int blockCounter = 0;
        private readonly Dictionary<SyntaxNode, int> opMap = new Dictionary<SyntaxNode, int>();
        private int opCounter = 0;

        public int BlockId(Block cfgBlock) =>
            this.blockMap.GetOrAdd(cfgBlock, b => this.blockCounter++);
        public string OpId(SyntaxNode node)
        {
            return this.opMap.GetOrAdd(node.RemoveParentheses(), b => this.opCounter++).ToString();
        }
        public string UniqueOpId()
        {
            return (opCounter++).ToString();
        }
    }
}
