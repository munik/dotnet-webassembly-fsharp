namespace WebAssembly.FSharp.Tests

open WebAssembly.FSharp
open WebAssembly.FSharp.Generation
open NUnit.Framework

module ValueTypes =
    type FunctionId = I32 | I64 | F32 | F64

    [<AbstractClass>]
    type Exports =
        abstract member I32 : int32 -> int32
        abstract member I64 : int64 -> int64
        abstract member F32 : float32 -> float32
        abstract member F64 : float -> float

    let moduleDefinition = {
        Functions = [
            { Id = I32;
              ParameterTypes = [ Int32 ]
              ReturnType = Int32
              Locals = []
              Body = [ LocalGet 0u; Int32Constant 1; Int32Add; End ] }
            { Id = I64;
              ParameterTypes = [ Int64 ]
              ReturnType = Int64
              Locals = []
              Body = [ LocalGet 0u; Int64Constant 1L; Int64Add; End ] }
            { Id = F32;
              ParameterTypes = [ Float32 ]
              ReturnType = Float32
              Locals = []
              Body = [ LocalGet 0u; Float32Constant 1.0F; Float32Add; End ] }
            { Id = F64;
              ParameterTypes = [ Float64 ]
              ReturnType = Float64
              Locals = []
              Body = [ LocalGet 0u; Float64Constant 1.0; Float64Add; End ] }
        ]
        Globals = []
        Exports = [ FunctionExport I32; FunctionExport I64; FunctionExport F32; FunctionExport F64 ]
    }

    let invokeExport fn moduleDefinition =
        let m = generateModule moduleDefinition
        let instanceCreator = m.Compile<Exports>()
        use instance = instanceCreator.Invoke(WebAssembly.Runtime.ImportDictionary())
        fn instance.Exports

    [<Test>]
    let I32 () =
        moduleDefinition
        |> invokeExport (fun exports ->
            Assert.AreEqual(2, exports.I32(1)))

    [<Test>]
    let I64 () =
        moduleDefinition
        |> invokeExport (fun exports ->
            Assert.AreEqual(2L, exports.I64(1L)))

    [<Test>]
    let F32 () =
        moduleDefinition
        |> invokeExport (fun exports ->
            Assert.AreEqual(2.0F, exports.F32(1.0F)))

    [<Test>]
    let F64 () =
        moduleDefinition
        |> invokeExport (fun exports ->
            Assert.AreEqual(2.0, exports.F64(1.0)))

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
