module WebAssembly.FSharp.Generation.Globals

open WebAssembly.FSharp.Generation.Instructions

let applyGlobal<'fn, 'glbl> (m : WebAssembly.Module) (getFunctionIndex : 'fn -> uint32) (getGlobalIndex : 'glbl -> uint32) (globalDefinition : GlobalDefinition<'fn, 'glbl>) =
    let glbl = WebAssembly.Global(convertValueType globalDefinition.Type, globalDefinition.Initializer |> Seq.map (genInstruction getFunctionIndex getGlobalIndex) |> Array.ofSeq)
    glbl.IsMutable <- globalDefinition.IsMutable
    m.Globals.Add(glbl)
