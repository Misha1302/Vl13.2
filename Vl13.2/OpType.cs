namespace Vl13._2;

public enum OpType
{
    // ------- stack
    PushI64,
    PushF64,

    Drop,
    Dup,

    // ------- memory
    StoreI64,
    StoreI32,
    StoreI16,
    StoreI8,
    StoreF64,

    LoadI64,
    LoadI32,
    LoadI16,
    LoadI8,
    LoadF64,

    LocAddress,

    // ------- conv
    I8ToI64,
    I16ToI64,
    I32ToI64,

    I64ToI8,
    I64ToI16,
    I64ToI32,

    I64ToF64,
    F64ToI64,

    // ------- cmp
    Eq,
    Neq,
    Lt,
    Le,
    Gt,
    Ge,

    // ------- branches
    Br,
    BrOne,
    BrZero,
    Ret,

    // ------- math ops
    AddI64,
    AddF64,

    SubI64,
    SubF64,

    MulI64,
    MulF64,

    DivI64,
    DivF64,

    ModI64,
    ModF64,

    // ------- labels
    SetLabel,

    // ------- calls
    CallSharp,
    CallFunc,
    CallAddress
}