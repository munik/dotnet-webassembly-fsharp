module WebAssembly.FSharp.Generation.Modules

let generateModuleDynamicNames<'fn, 'glbl> (getFunctionInfo : 'fn -> uint32 * string) (getGlobalInfo : 'glbl -> uint32 * string) (moduleDefinition : ModuleDefinition<'fn, 'glbl>) =
    let m = WebAssembly.Module()
    moduleDefinition.Functions |> Seq.iter (Functions.applyFunction m (getFunctionInfo >> fst) (getGlobalInfo >> fst))
    moduleDefinition.Globals |> Seq.iter (Globals.applyGlobal m (getFunctionInfo >> fst) (getGlobalInfo >> fst))
    moduleDefinition.Exports |> Seq.iter (Exports.applyExport m getFunctionInfo getGlobalInfo)
    m

let getUnionCaseInfo<'a> (a : 'a) : uint32 * string =
    let case, _ = Reflection.FSharpValue.GetUnionFields(a, typeof<'a>)
    uint32 case.Tag, case.Name

let getTag<'a> = getUnionCaseInfo<'a> >> fst

let generateModule<'fn, 'glbl> (moduleDefinition : ModuleDefinition<'fn, 'glbl>) =
    let m = WebAssembly.Module()
    moduleDefinition.Functions |> Seq.iter (Functions.applyFunction m getTag getTag)
    moduleDefinition.Globals |> Seq.iter (Globals.applyGlobal m getTag getTag)
    moduleDefinition.Exports |> Seq.iter (Exports.applyExport m getUnionCaseInfo getUnionCaseInfo)
    m
