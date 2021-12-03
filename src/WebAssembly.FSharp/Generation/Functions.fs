module WebAssembly.FSharp.Generation.Functions

open WebAssembly.FSharp.Generation.Instructions

let generateLocal (localDef : LocalDefinition) =
    let local = WebAssembly.Local()
    local.Count <- localDef.Count
    local.Type <- convertValueType localDef.Type
    local

let applyFunction<'fn, 'glbl> (m : WebAssembly.Module) (getFunctionIndex : 'fn -> uint32) (getGlobalIndex : 'glbl -> uint32) (fnDefinition : FunctionDefinition<'fn, 'glbl>) : unit =
    let fnType = WebAssembly.WebAssemblyType()
    fnType.Parameters <- fnDefinition.ParameterTypes |> Seq.map convertValueType |> Array.ofSeq
    fnType.Returns <- [| fnDefinition.ReturnType |> convertValueType |]
    let fnTypeIndex = m.Types.Count |> uint32
    m.Types.Add(fnType)
    m.Functions.Add(WebAssembly.Function(fnTypeIndex))
    let fnBody = WebAssembly.FunctionBody()
    fnBody.Locals <- fnDefinition.Locals |> Seq.map generateLocal |> Array.ofSeq
    fnBody.Code <- fnDefinition.Body |> Seq.map (genInstruction getFunctionIndex getGlobalIndex) |> Array.ofSeq
    m.Codes.Add(fnBody)
