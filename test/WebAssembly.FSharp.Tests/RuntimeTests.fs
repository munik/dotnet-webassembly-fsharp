namespace WebAssembly.FSharp.Runtime.Tests

open WebAssembly.FSharp.Generation
open WebAssembly.FSharp.Generation.Modules
open NUnit.Framework
open WebAssembly.Runtime
open System.Runtime.InteropServices

module RudimentaryUTF32StringEquals =
    type FunctionId = StrEq
    type MemoryId = MainMemory

    [<AbstractClass>]
    type Exports =
        abstract member StrEq : int32 * int32 -> int32
        abstract member MainMemory : UnmanagedMemory

    let moduleDefinition = {
        Functions = [
            { Id = StrEq;
              ParameterTypes = [ Int32; Int32 ]
              ReturnType = Int32
              Locals = [ { Count = 1u; Type = Int32 } ]
              Body = [
                LocalGet 0u
                Int32Load
                LocalGet 1u
                Int32Load
                Int32NotEqual
                If
                Int32Constant 0
                Return
                End

                Block BlockType.Empty // block 0

                // store length and return true if length is zero
                LocalGet 0u
                Int32Load
                LocalTee 2u
                Int32EqualZero
                BranchIf 0u

                // store pointer to first char
                LocalGet 0u
                Int32Constant 4
                Int32Add
                LocalSet 0u
                LocalGet 1u
                Int32Constant 4
                Int32Add
                LocalSet 1u

                Loop BlockType.Empty // block 1
                LocalGet 0u
                Int32Load
                LocalGet 1u
                Int32Load
                Int32NotEqual
                If
                Int32Constant 0
                Return
                Else
                LocalGet 2u
                Int32Constant 1
                Int32Subtract
                LocalTee 2u
                Int32EqualZero
                BranchIf 0u

                LocalGet 0u
                Int32Constant 4
                Int32Add
                LocalSet 0u

                LocalGet 1u
                Int32Constant 4
                Int32Add
                LocalSet 1u

                Branch 1u
                End // if

                End // loop (block 1)
                End // block 0

                Int32Constant 1
                End
              ] }
        ]
        Globals = []
        Exports = [ FunctionExport StrEq; MemoryExport MainMemory ]
        Memories = [ { Id = MainMemory; MinimumPages = 1u; MaximumPages = 2u } ]
    }

    let storeString (start : System.IntPtr) =
        let mutable nextOffset = 0
        fun (str : string) ->
            let bytes = System.Text.Encoding.UTF32.GetBytes(str)
            let ptr = nextOffset
            Marshal.WriteInt32(System.IntPtr.Add(start, ptr), bytes.Length / 4);
            let offset = ptr + 4
            for i in 0 .. bytes.Length - 1 do
                Marshal.WriteInt32(System.IntPtr.Add(start, offset + i), bytes.[i] |> int)
            nextOffset <- offset + bytes.Length
            ptr

    let dumpMemory (start : System.IntPtr) (length : int) =
        for i in 0 .. length - 1 do
            System.Console.Error.Write(Marshal.ReadByte(System.IntPtr.Add(start, i)))
            System.Console.Error.Write(" ")
            if i % 4 = 3 then System.Console.Error.WriteLine()

    let invokeExport fn moduleDefinition =
        let m = generateModule moduleDefinition
        let instanceCreator = m.Compile<Exports>()
        use instance = instanceCreator.Invoke(WebAssembly.Runtime.ImportDictionary())
        fn instance.Exports

    [<Test>]
    let emptyBoth () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString ""
            let str2 = storeString ""
            Assert.AreEqual(1, exports.StrEq(str1, str2)))

    [<Test>]
    let emptyLeft () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString ""
            let str2 = storeString "a"
            Assert.AreEqual(0, exports.StrEq(str1, str2)))

    [<Test>]
    let emptyRight () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString "a"
            let str2 = storeString ""
            Assert.AreEqual(0, exports.StrEq(str1, str2)))

    [<Test>]
    let equalOneChar () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString "a"
            let str2 = storeString "a"
            Assert.AreEqual(1, exports.StrEq(str1, str2)))

    [<Test>]
    let sameLengthAndContent () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString "ab"
            let str2 = storeString "ab"
            Assert.AreEqual(1, exports.StrEq(str1, str2)))

    [<Test>]
    let diffLength () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString "a"
            let str2 = storeString "ab"
            Assert.AreEqual(0, exports.StrEq(str1, str2)))

    [<Test>]
    let sameLengthDiffContent () = 
        moduleDefinition
        |> invokeExport (fun exports ->
            let storeString = storeString exports.MainMemory.Start
            let str1 = storeString "ab"
            let str2 = storeString "ac"
            Assert.AreEqual(0, exports.StrEq(str1, str2)))

