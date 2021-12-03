module WebAssembly.FSharp.Generation.Instructions

open WebAssembly.Instructions

let convertValueType : ValueType -> WebAssembly.WebAssemblyValueType = function
    | ValueType.Int32 -> WebAssembly.WebAssemblyValueType.Int32
    | ValueType.Int64 -> WebAssembly.WebAssemblyValueType.Int64
    | ValueType.Float32 -> WebAssembly.WebAssemblyValueType.Float32
    | ValueType.Float64 -> WebAssembly.WebAssemblyValueType.Float64

let convertBlockType : BlockType -> WebAssembly.BlockType = function
    | BlockType.Int32 -> WebAssembly.BlockType.Int32
    | BlockType.Int64 -> WebAssembly.BlockType.Int64
    | BlockType.Float32 -> WebAssembly.BlockType.Float32
    | BlockType.Float64 -> WebAssembly.BlockType.Float64
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
    | Int64Add -> Int64Add() :> WebAssembly.Instruction
    | Float32Add -> Float32Add() :> WebAssembly.Instruction
    | Float64Add -> Float64Add() :> WebAssembly.Instruction
    | Int32Constant value -> Int32Constant(value) :> WebAssembly.Instruction
    | Int64Constant value -> Int64Constant(value) :> WebAssembly.Instruction
    | Float32Constant value -> Float32Constant(value) :> WebAssembly.Instruction
    | Float64Constant value -> Float64Constant(value) :> WebAssembly.Instruction
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
