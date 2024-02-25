namespace Vl13._2;

public partial record VlImageInfo(string Name, AsmType[] ArgTypes)
{
    public readonly VlImage Image = new();

    public int GetStackSizeInBytes()
    {
        var cur = 0;
        var max = 0;

        foreach (var op in Image.Ops)
        {
            cur += op.StackOutput();
            max = Math.Max(max, cur);
        }

        return max * 8 + 16; // 16 - Reserved space for temporary computing
    }

    public int GetLocalSizeInBytes()
    {
        return Image.Ops
            .Where(x => x.OpType == OpType.LocAddress)
            .DistinctBy(x => x.Arg<string>(0))
            .Count() * 8;
    }

    public static int RoundUpSize(int stackSize) =>
        stackSize % 16 == 0 ? stackSize : stackSize + 8;

    public int GetTotalSize() =>
        RoundUpSize(GetLocalSizeInBytes() + GetStackSizeInBytes());

    public static string GenerateLabelName(string name) =>
        $"{name}{Guid.NewGuid().ToString()}";
}

public partial record VlImageInfo
{
    public void PushI(long i) => Image.Emit(new Op(OpType.Push, i));
    public void PushF(double i) => Image.Emit(new Op(OpType.Push, i));
    public void Drop() => Image.Emit(new Op(OpType.Drop, null));
    public void Store64() => Image.Emit(new Op(OpType.Store64, null));
    public void Store32() => Image.Emit(new Op(OpType.Store32, null));
    public void Store16() => Image.Emit(new Op(OpType.Store16, null));
    public void Store8() => Image.Emit(new Op(OpType.Store8, null));
    public void Load64() => Image.Emit(new Op(OpType.Load64, null));
    public void Load32() => Image.Emit(new Op(OpType.Load32, null));
    public void Load16() => Image.Emit(new Op(OpType.Load16, null));
    public void Load8() => Image.Emit(new Op(OpType.Load8, null));
    public void I8ToI64() => Image.Emit(new Op(OpType.I8ToI64, null));
    public void I16ToI64() => Image.Emit(new Op(OpType.I16ToI64, null));
    public void I32ToI64() => Image.Emit(new Op(OpType.I32ToI64, null));
    public void I64ToI8() => Image.Emit(new Op(OpType.I64ToI8, null));
    public void I64ToI16() => Image.Emit(new Op(OpType.I64ToI16, null));
    public void I64ToI32() => Image.Emit(new Op(OpType.I64ToI32, null));
    public void I64ToF64() => Image.Emit(new Op(OpType.I64ToF64, null));
    public void F64ToI64() => Image.Emit(new Op(OpType.F64ToI64, null));
    public void Eq() => Image.Emit(new Op(OpType.Eq, null));
    public void Neq() => Image.Emit(new Op(OpType.Neq, null));
    public void Lt() => Image.Emit(new Op(OpType.Lt, null));
    public void Le() => Image.Emit(new Op(OpType.Le, null));
    public void Gt() => Image.Emit(new Op(OpType.Gt, null));
    public void Ge() => Image.Emit(new Op(OpType.Ge, null));
    public void Br(string labelName) => Image.Emit(new Op(OpType.Br, labelName));
    public void BrOne(string labelName) => Image.Emit(new Op(OpType.BrOne, labelName));
    public void BrZero(string labelName) => Image.Emit(new Op(OpType.BrZero, labelName));
    public void Add() => Image.Emit(new Op(OpType.Add, null));
    public void Sub() => Image.Emit(new Op(OpType.Sub, null));
    public void Mul() => Image.Emit(new Op(OpType.Mul, null));
    public void Div() => Image.Emit(new Op(OpType.Div, null));
    public void Mod() => Image.Emit(new Op(OpType.Mod, null));
    public void SetLabel(string labelName) => Image.Emit(new Op(OpType.SetLabel, labelName));

    public void CallFunc(string name, int argsCount, AsmType returnType) =>
        Image.Emit(new Op(OpType.CallFunc, name, argsCount, returnType));

    public void CallAddress() => Image.Emit(new Op(OpType.CallAddress, null));
    public void Ret() => Image.Emit(new Op(OpType.Ret, null));
    public void LocAddress(string locName, AsmType type) => Image.Emit(new Op(OpType.LocAddress, locName, type));
    public void Dup() => Image.Emit(new Op(OpType.Dup, null));
    public void Init() => Image.Emit(new Op(OpType.Init, null));
    public void End() => Image.Emit(new Op(OpType.End, null));

    public void CallSharp(Type t, string name, Type[]? parameters = null) =>
        Image.Emit(new Op(OpType.CallSharp, t, name, parameters!));
}