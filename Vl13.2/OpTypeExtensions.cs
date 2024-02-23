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

    public static bool IsPush(this OpType v) => v is OpType.PushF64 or OpType.PushI64;
    public static bool IsDrop(this OpType v) => v is OpType.Drop;
    public static bool IsDup(this OpType v) => v is OpType.Dup;

    public static bool IsStore(this OpType v) =>
        v is OpType.StoreI8 or OpType.StoreI16 or OpType.StoreI32 or OpType.StoreI64 or OpType.StoreF64;

    public static bool IsLoad(this OpType v) =>
        v is OpType.LoadI8 or OpType.LoadI16 or OpType.LoadI32 or OpType.LoadI64 or OpType.LoadF64 or OpType.LocAddress;

    public static bool IsConv(this OpType v) =>
        v is OpType.I8ToI64 or OpType.I16ToI64 or OpType.I32ToI64 or OpType.I64ToI8 or OpType.I64ToI16
            or OpType.I64ToI32 or OpType.I64ToF64 or OpType.F64ToI64;

    public static bool IsCmp(this OpType v) =>
        v is OpType.Eq or OpType.Neq or OpType.Lt or OpType.Le or OpType.Gt or OpType.Ge;

    public static bool IsBranch(this OpType v) =>
        v is OpType.Br or OpType.BrZero or OpType.BrOne or OpType.Ret;

    public static bool IsMathOp(this OpType v) =>
        v is OpType.AddF64 or OpType.AddI64 or OpType.SubI64 or OpType.SubF64 or OpType.MulI64 or OpType.MulF64
            or OpType.DivI64 or OpType.DivF64 or OpType.ModI64 or OpType.ModF64;

    public static bool IsSetLabel(this OpType v) => v is OpType.SetLabel;
    public static bool IsCall(this OpType v) => v is OpType.CallAddress or OpType.CallFunc or OpType.CallSharp;
}