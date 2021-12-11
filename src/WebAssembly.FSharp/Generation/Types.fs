namespace WebAssembly.FSharp.Generation

type BlockType =
    | Int32
    | Int64
    | Float32
    | Float64
    | Empty

type ValueType = 
    | Int32
    | Int64
    | Float32
    | Float64

type Instruction<'fn, 'glbl> =
    | Block of blockType : BlockType
    | Branch of block : uint32
    | BranchIf of block : uint32
    | BranchTable of defaultLabel : uint32 * labels : uint32 list
    | Call of fn : 'fn
    | Drop
    | Else
    | End
    | GlobalGet of glbl : 'glbl
    | GlobalSet of glbl : 'glbl
    | If
    | Int32Add
    | Int64Add
    | Float32Add
    | Float64Add
    | Int32Constant of value : int32
    | Int64Constant of value : int64
    | Float32Constant of value : float32
    | Float64Constant of value : float
    | Int32Equal
    | Int32EqualZero
    | Int32Load
    | Int32NotEqual
    | Int32Store
    | Int32Subtract
    | LocalGet of uint32
    | LocalSet of uint32
    | LocalTee of uint32
    | Loop of block : BlockType
    | Return

type LocalDefinition = {
    Count : uint32
    Type : ValueType
}

type FunctionDefinition<'fn, 'glbl> = {
    Id : 'fn
    ParameterTypes : ValueType list
    ReturnType : ValueType
    Locals : LocalDefinition list
    Body : Instruction<'fn, 'glbl> list
}

type ExportDefinition<'fn, 'glbl, 'mem> =
    | FunctionExport of 'fn
    | GlobalExport of 'glbl
    | MemoryExport of 'mem

type GlobalDefinition<'fn, 'glbl> = {
    Id : 'glbl
    Type : ValueType
    IsMutable : bool
    Initializer : Instruction<'fn, 'glbl> list
}

type MemoryDefinition<'mem> = {
    Id : 'mem
    MinimumPages : uint32
    MaximumPages : uint32
}

type ModuleDefinition<'fn, 'glbl, 'mem> = {
    Functions : FunctionDefinition<'fn, 'glbl> list
    Exports : ExportDefinition<'fn, 'glbl, 'mem> list
    Globals : GlobalDefinition<'fn, 'glbl> list
    Memories : MemoryDefinition<'mem> list
}
