namespace SonarAnalyzer.FSharp

open FSharp.Analyzers.SDK
open FSharp.Analyzers.SDK.Common

[<Analyzer>]
let myAnalyzer : Analyzer =
    fun ctx ->
        let diagnostics =
            ctx.TypedTree
            |> Seq.collect (fun tree ->
                tree.Declarations
                |> Seq.collect (fun decl ->
                    match decl with
                    | FSharpDeclaration.FunctionOrValue (func, _) when func.IsMemberOrFunctionOrValue ->
                        let diagnostic =
                            { Range = func.DeclarationLocation
                              Message = "Function or value declaration"
                              Severity = Severity.Warning
                              Code = "FS0001" }
                        [diagnostic]
                    | _ -> []))
        diagnostics |> Seq.toList
