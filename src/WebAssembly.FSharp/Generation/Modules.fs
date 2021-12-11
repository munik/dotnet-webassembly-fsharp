module WebAssembly.FSharp.Generation.Modules

let generateModuleDynamicNames (getFunctionInfo : 'fn -> uint32 * string) (getGlobalInfo : 'glbl -> uint32 * string) (getMemoryInfo : 'mem -> uint32 * string) (moduleDefinition : ModuleDefinition<'fn, 'glbl, 'mem>) =
    let m = WebAssembly.Module()
    moduleDefinition.Functions |> Seq.iter (Functions.applyFunction m (getFunctionInfo >> fst) (getGlobalInfo >> fst))
    moduleDefinition.Globals |> Seq.iter (Globals.applyGlobal m (getFunctionInfo >> fst) (getGlobalInfo >> fst))
    moduleDefinition.Exports |> Seq.iter (Exports.applyExport m getFunctionInfo getGlobalInfo getMemoryInfo)
    m

let getUnionCaseInfo<'a> (a : 'a) : uint32 * string =
    let case, _ = Reflection.FSharpValue.GetUnionFields(a, typeof<'a>)
    uint32 case.Tag, case.Name

let getTag<'a> = getUnionCaseInfo<'a> >> fst

let generateModule (moduleDefinition : ModuleDefinition<'fn, 'glbl, 'mem>) =
    let m = WebAssembly.Module()
    moduleDefinition.Functions |> Seq.iter (Functions.applyFunction m getTag getTag)
    moduleDefinition.Globals |> Seq.iter (Globals.applyGlobal m getTag getTag)
    moduleDefinition.Exports |> Seq.iter (Exports.applyExport m getUnionCaseInfo getUnionCaseInfo getUnionCaseInfo)
    moduleDefinition.Memories |> Seq.iter (Memory.applyMemory m getTag)
    m
