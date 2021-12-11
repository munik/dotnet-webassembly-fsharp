module WebAssembly.FSharp.Generation.Memory

let applyMemory<'mem> (m : WebAssembly.Module) (getMemoryIndex : 'mem -> uint32) (memoryDef : MemoryDefinition<'mem>) =
    let mem = WebAssembly.Memory(memoryDef.MinimumPages, memoryDef.MaximumPages)
    m.Memories.Add(mem)
