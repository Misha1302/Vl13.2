namespace Vl13._2;

public static class OpTypeExtensions
{
    public static int StackOutput(this OpType value)
    {
        if (value.IsPush()) return 1;
        if (value.IsDup()) return 1;
        if (value.IsDrop()) return -1;
        if (value.IsStore()) return -1;
        if (value.IsLoad()) return 1;
        if (value.IsConv()) return 0;
        if (value.IsCmp()) return -1;
        if (value.IsBranch()) return 0;
        if (value.IsMathOp()) return -1;
        if (value.IsSetLabel()) return 0;
        if (value.IsCall()) return 1;

        return Thrower.Throw<int>(new ArgumentOutOfRangeException());
    }

    public static bool IsPush(this OpType v) => v is OpType.Push;
    public static bool IsDrop(this OpType v) => v is OpType.Drop;
    public static bool IsDup(this OpType v) => v is OpType.Dup;

    public static bool IsStore(this OpType v) =>
        v is OpType.Store8 or OpType.Store16 or OpType.Store32 or OpType.Store64;

    public static bool IsLoad(this OpType v) =>
        v is OpType.Load8 or OpType.Load16 or OpType.Load32 or OpType.Load64 or OpType.Load64 or OpType.LocAddress;

    public static bool IsConv(this OpType v) =>
        v is OpType.I8ToI64 or OpType.I16ToI64 or OpType.I32ToI64
            or OpType.I64ToI32 or OpType.I64ToI16 or OpType.I64ToI8
            or OpType.I64ToF64 or OpType.F64ToI64;

    public static bool IsCmp(this OpType v) =>
        v is OpType.Eq or OpType.Neq or OpType.Lt or OpType.Le or OpType.Gt or OpType.Ge;

    public static bool IsBranch(this OpType v) =>
        v is OpType.Br or OpType.BrZero or OpType.BrOne or OpType.Ret;

    public static bool IsMathOp(this OpType v) =>
        v is OpType.Add or OpType.Sub or OpType.Mul or OpType.Div or OpType.Mod;

    public static bool IsSetLabel(this OpType v) => v is OpType.SetLabel;
    public static bool IsCall(this OpType v) => v is OpType.CallAddress or OpType.CallFunc or OpType.CallSharp;
}