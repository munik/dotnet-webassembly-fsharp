module WebAssembly.FSharp.Generation

open WebAssembly.Instructions

let convertValueType : ValueType -> WebAssembly.WebAssemblyValueType = function
    | ValueType.Int32 -> WebAssembly.WebAssemblyValueType.Int32
    | ValueType.Int64 -> WebAssembly.WebAssemblyValueType.Int32
    | ValueType.Float32 -> WebAssembly.WebAssemblyValueType.Int32
    | ValueType.Float64 -> WebAssembly.WebAssemblyValueType.Int32

let convertBlockType : BlockType -> WebAssembly.BlockType = function
    | BlockType.Int32 -> WebAssembly.BlockType.Int32
    | BlockType.Int64 -> WebAssembly.BlockType.Int32
    | BlockType.Float32 -> WebAssembly.BlockType.Int32
    | BlockType.Float64 -> WebAssembly.BlockType.Int32
    | BlockType.Empty -> WebAssembly.BlockType.Empty

let genInstruction<'fn, 'glbl>
    (getFunctionIndex : 'fn -> uint32)
    (getGlobalIndex : 'glbl -> uint)
    (instruction : Instruction<'fn, 'glbl>) : WebAssembly.Instruction =
    match instruction with
    | Block blockType -> Block(convertBlockType blockType) :> WebAssembly.Instruction
    | Branch block -> Branch(block) :> WebAssembly.Instruction
    | BranchIf block -> BranchIf(block) :> WebAssembly.Instruction
    | BranchTable (defaultLabel, labels) ->
      BranchTable(defaultLabel,
                  labels |> Array.ofSeq) :> WebAssembly.Instruction
    | Call fn -> Call(getFunctionIndex fn) :> WebAssembly.Instruction
    | Drop -> Drop() :> WebAssembly.Instruction
    | Else -> Else() :> WebAssembly.Instruction
    | End -> End() :> WebAssembly.Instruction
    | GlobalGet glbl -> GlobalGet(getGlobalIndex glbl) :> WebAssembly.Instruction
    | GlobalSet glbl -> GlobalSet(getGlobalIndex glbl) :> WebAssembly.Instruction
    | If -> If() :> WebAssembly.Instruction
    | Int32Add -> Int32Add() :> WebAssembly.Instruction
    | Int32Constant value -> Int32Constant(value) :> WebAssembly.Instruction
    | Int32Equal -> Int32Equal() :> WebAssembly.Instruction
    | Int32EqualZero -> Int32EqualZero() :> WebAssembly.Instruction
    | Int32Load -> Int32Load() :> WebAssembly.Instruction
    | Int32NotEqual -> Int32NotEqual() :> WebAssembly.Instruction
    | Int32Store -> Int32Store() :> WebAssembly.Instruction
    | Int32Subtract -> Int32Subtract() :> WebAssembly.Instruction
    | LocalGet local -> LocalGet(local) :> WebAssembly.Instruction
    | LocalSet local -> LocalSet(local) :> WebAssembly.Instruction
    | LocalTee local -> LocalTee(local) :> WebAssembly.Instruction
    | Loop blockType -> Loop(convertBlockType blockType) :> WebAssembly.Instruction
    | Return -> Return() :> WebAssembly.Instruction

let getFunctionIndex<'fn> (fn : 'fn) : uint32 =
    let case, _ = Reflection.FSharpValue.GetUnionFields(fn, typeof<'fn>)
    case.Tag |> uint32

let getGlobalIndex<'glbl> (glbl : 'glbl) : uint32 =
    let case, _ = Reflection.FSharpValue.GetUnionFields(glbl, typeof<'glbl>)
    case.Tag |> uint32

let addFunction<'fn, 'glbl> (m : WebAssembly.Module)
    (fnDefinition : FunctionDefinition<'fn, 'glbl>) =
    let fnType = WebAssembly.WebAssemblyType()
    fnType.Parameters <- fnDefinition.ParameterTypes |> Seq.map convertValueType |> Array.ofSeq
    fnType.Returns <- [| fnDefinition.ReturnType |> convertValueType |]
    let fnTypeIndex = m.Types.Count |> uint32
    m.Types.Add(fnType)
    m.Functions.Add(WebAssembly.Function(fnTypeIndex))
    let fnBody = WebAssembly.FunctionBody()
    fnBody.Locals <- fnDefinition.Locals |> Seq.map (fun localDef ->
        let local = WebAssembly.Local()
        local.Count <- localDef.Count
        local.Type <- convertValueType localDef.Type
        local) |> Array.ofSeq
    fnBody.Code <- fnDefinition.Body |> Seq.map (genInstruction getFunctionIndex getGlobalIndex) |> Array.ofSeq
    m.Codes.Add(fnBody)

let addGlobal<'fn, 'glbl> (m : WebAssembly.Module)
    (globalDefinition : GlobalDefinition<'fn, 'glbl>) =
    let glbl = WebAssembly.Global(convertValueType globalDefinition.Type, globalDefinition.Initializer |> Seq.map (genInstruction getFunctionIndex getGlobalIndex) |> Array.ofSeq)
    glbl.IsMutable <- globalDefinition.IsMutable
    m.Globals.Add(glbl)

let generateModule<'fn, 'glbl> (moduleDefinition : ModuleDefinition<'fn, 'glbl>) =
    let m = WebAssembly.Module()
    moduleDefinition.Functions |> Seq.iter (addFunction m)
    moduleDefinition.Globals |> Seq.iter (addGlobal m)
    moduleDefinition.Exports
    |> Seq.iter (function
        | FunctionExport fn ->
          let case, _ = Reflection.FSharpValue.GetUnionFields(fn, typeof<'fn>)
          m.Exports.Add(WebAssembly.Export(case.Name, uint32 case.Tag, WebAssembly.ExternalKind.Function))
        | GlobalExport glbl ->
          let case, _ = Reflection.FSharpValue.GetUnionFields(glbl, typeof<'glbl>)
          m.Exports.Add(WebAssembly.Export(case.Name, uint32 case.Tag, WebAssembly.ExternalKind.Global)))
    m