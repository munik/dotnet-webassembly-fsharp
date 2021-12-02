# F# wrapper for WebAssembly for .NET

An F# wrapper around the [dotnet-webassembly](https://github.com/ryanLamansky/dotnet-webassembly/) project, which is
> A library able to create, read, modify, write and execute WebAssembly (WASM) files from .NET-based applications. Execution does not use an interpreter or a 3rd party library: WASM instructions are mapped to their .NET equivalents and converted to native machine language by the .NET JIT compiler.

## Goals

- A little higher-level WebAssembly module generation toolkit than the base library provides.
- An idiomatic F# approach to generating and running WebAssembly programs.
- Compile-time safety for as many aspects of module generation as possible without sacrificing expressiveness nor using any of the more exotic F# features (e.g. code quotations).

## Getting Started

At the moment, this library is very nascent, both in terms of the maturity of the abstraction and the completeness of the implementation (e.g. currently not all of the functionality of the base library is supported).
However, here's a simple example that works now:

```fsharp
type FunctionId = A | B

[<AbstractClass>]
type Exports =
    abstract member A : int32 * int32 -> int32
    abstract member B : unit -> int32

let moduleDefinition = {
    Functions = [
        { Id = A
            ParameterTypes = [ Int32; Int32 ]
            ReturnType = Int32
            Locals = []
            Body = [ LocalGet 0u
                     LocalGet 1u
                     Call B
                     Int32Add
                     Int32Add
                     End ] }
        { Id = B
          ParameterTypes = [ ]
          ReturnType = Int32
          Locals = []
          Body = [ Int32Constant 2
                   End ] }

    ]
    Globals = []
    Exports = [ FunctionExport A; FunctionExport B ]
}

let m = generateModule moduleDefinition
let instanceCreator = m.Compile<Exports>()
use instance = instanceCreator.Invoke(WebAssembly.Runtime.ImportDictionary())
instance.Exports.A(1, 2)
// returns 5
```
