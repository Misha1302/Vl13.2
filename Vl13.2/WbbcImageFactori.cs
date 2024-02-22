namespace Vl13._2;

public class VlImageInfo
{
    public readonly VlImage Image = new();
    
    public int LocalSizeInBytes = 0;

    public void PushI64(long i) => Image.Emit(new Op(OpType.PushI64, i));
    public void PushF64(double d) => Image.Emit(new Op(OpType.PushF64, d));
    public void Drop() => Image.Emit(new Op(OpType.Drop, null));
    public void StoreI64() => Image.Emit(new Op(OpType.StoreI64, null));
    public void StoreI32() => Image.Emit(new Op(OpType.StoreI32, null));
    public void StoreI16() => Image.Emit(new Op(OpType.StoreI16, null));
    public void StoreI8() => Image.Emit(new Op(OpType.StoreI8, null));
    public void StoreF64() => Image.Emit(new Op(OpType.StoreF64, null));
    public void LoadI64() => Image.Emit(new Op(OpType.LoadI64, null));
    public void LoadI32() => Image.Emit(new Op(OpType.LoadI32, null));
    public void LoadI16() => Image.Emit(new Op(OpType.LoadI16, null));
    public void LoadI8() => Image.Emit(new Op(OpType.LoadI8, null));
    public void LoadF64() => Image.Emit(new Op(OpType.LoadF64, null));
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
    public void AddI64() => Image.Emit(new Op(OpType.AddI64, null));
    public void AddF64() => Image.Emit(new Op(OpType.AddF64, null));
    public void SubI64() => Image.Emit(new Op(OpType.SubI64, null));
    public void SubF64() => Image.Emit(new Op(OpType.SubF64, null));
    public void MulI64() => Image.Emit(new Op(OpType.MulI64, null));
    public void MulF64() => Image.Emit(new Op(OpType.MulF64, null));
    public void DivI64() => Image.Emit(new Op(OpType.DivI64, null));
    public void DivF64() => Image.Emit(new Op(OpType.DivF64, null));
    public void ModI64() => Image.Emit(new Op(OpType.ModI64, null));
    public void ModF64() => Image.Emit(new Op(OpType.ModF64, null));
    public void SetLabel(string labelName) => Image.Emit(new Op(OpType.SetLabel, labelName));

    public void CallSharp(Type t, string name, Type[]? parameters = null) =>
        Image.Emit(new Op(OpType.CallSharp, t, name, parameters!));

    public void CallFunc() => Image.Emit(new Op(OpType.CallFunc, null));
    public void CallAddress() => Image.Emit(new Op(OpType.CallAddress, null));
    public void Ret() => Image.Emit(new Op(OpType.Ret, null));

    public void LocAddress(string locName) => Image.Emit(new Op(OpType.LocAddress, locName));
}