namespace Vl13._2;

public enum OpType
{
    // ------- stack
    Push,

    Drop,
    Dup,

    // ------- memory
    Store64,
    Store32,
    Store16,
    Store8,

    Load64,
    Load32,
    Load16,
    Load8,

    LocAddress,
    LabelAddress,

    StoreDataToLabel,
    LoadDataFromLabel,
    CreateDataLabel,

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

    JumpToAddress,

    Ret,

    // ------- math ops
    Add,
    Sub,
    Mul,
    Div,
    Mod,

    // ------- labels
    SetLabel,

    // ------- calls
    CallSharp,
    CallFunc,
    CallAddress,

    // ------- others
    Prolog,
    Epilogue,
    Body,
    Init,
    End,
    Function,
    Nop,
    PushRsp,
    PushRbp
}