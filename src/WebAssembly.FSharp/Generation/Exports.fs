module WebAssembly.FSharp.Generation.Exports

open WebAssembly.FSharp.Generation

let applyExport<'fn, 'glbl, 'mem> (m : WebAssembly.Module) (getFunctionInfo : 'fn -> uint32 * string) (getGlobalInfo : 'glbl -> uint32 * string) (getMemoryInfo : 'mem -> uint32 * string) (export : ExportDefinition<'fn, 'glbl, 'mem>) =
    match export with
    | FunctionExport fn ->
      let functionIndex, functionName = getFunctionInfo fn
      m.Exports.Add(WebAssembly.Export(functionName, functionIndex, WebAssembly.ExternalKind.Function))
    | GlobalExport glbl ->
      let globalIndex, globalName = getGlobalInfo glbl
      m.Exports.Add(WebAssembly.Export(globalName, globalIndex, WebAssembly.ExternalKind.Global))
    | MemoryExport mem ->
      let memoryIndex, memoryName = getMemoryInfo mem
      m.Exports.Add(WebAssembly.Export(memoryName, memoryIndex, WebAssembly.ExternalKind.Memory))
