namespace WebAssembly.FSharp.Tests

open WebAssembly.FSharp
open WebAssembly.FSharp.Generation
open NUnit.Framework

module SimpleFunction =
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

    let invokeExport fn moduleDefinition =
        let m = generateModule moduleDefinition
        let instanceCreator = m.Compile<Exports>()
        use instance = instanceCreator.Invoke(WebAssembly.Runtime.ImportDictionary())
        fn instance.Exports

    [<Test>]
    let TestExportA () =
        moduleDefinition
        |> invokeExport (fun exports ->
            Assert.AreEqual(5, exports.A(1, 2)))

    [<Test>]
    let TestExportB () =
        moduleDefinition
        |> invokeExport (fun exports ->
            Assert.AreEqual(2, exports.B()))
