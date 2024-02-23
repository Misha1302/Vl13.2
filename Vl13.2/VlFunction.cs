namespace Vl13._2;

using Iced.Intel;

/// <summary>
///     https://en.wikipedia.org/wiki/X86_calling_conventions
/// </summary>
public class VlFunction
{
    private readonly VlImageInfo _imageInfo;

    private readonly Assembler _asm;

    private readonly EnterLeaveFunctionManager _functionManager;
    private readonly DataManager _dataManager;
    private readonly LabelsManager _labelsManager;
    private readonly StackManager _sm;
    private readonly CallManager _callManager;
    private readonly LocalsManager _localsManager;

    public VlFunction(VlImageInfo imageInfo, Assembler asm)
    {
        _asm = asm;
        _imageInfo = imageInfo;

        _localsManager = new LocalsManager();

        _labelsManager = new LabelsManager(_asm);
        _dataManager = new DataManager(_asm);

        _sm = new StackManager(_asm, new StackPositioner(_imageInfo.GetLocalSizeInBytes()));
        _functionManager = new EnterLeaveFunctionManager(_asm, _sm, _imageInfo, _labelsManager, EmitOp);
        _callManager = new CallManager(_asm, _sm);
    }

    public void Translate()
    {
        _labelsManager.GetOrAddLabel("return_label");

        ValidateImage();

        _functionManager.Prolog();
        _functionManager.Body();
        _functionManager.Epilogue();

        _dataManager.EmitData();
    }

    private void ValidateImage()
    {
        if (_imageInfo.Image.Ops[^1].OpType != OpType.Ret)
            Thrower.Throw(new InvalidOperationException());
    }

    private void EmitOp(Op op)
    {
        switch (op.OpType)
        {
            case OpType.Push:
                if (op.Params?[0] is long)
                    PushConst(op.Arg<long>(0));
                else PushConst(op.Arg<double>(0));
                break;
            case OpType.Drop:
                _sm.Drop();
                break;
            case OpType.Store64:
                _sm.PopRegs(r10, rax); // reference, value
                _asm.mov(__[r10], rax); // *(&variable) = value
                break;
            case OpType.Store32:
                _sm.PopRegs(r10, rax); // reference, value
                _asm.mov(__[r10], eax); // *(&variable) = value
                break;
            case OpType.Store16:
                _sm.PopRegs(r10, rax); // reference, value
                _asm.mov(__[r10], ax); // *(&variable) = value
                break;
            case OpType.Store8:
                _sm.PopRegs(r10, rax); // reference, value
                _asm.mov(__[r10], al); // *(&variable) = value
                break;
            case OpType.Load64:
                MultitypeOp(() => _sm.Load64(AsmType.I64), () => _sm.Load64(AsmType.F64));
                break;
            case OpType.Load32:
                MultitypeOp(_sm.LoadI32,
                    () => Thrower.Throw(
                        new InvalidOperationException("This operation cannot be applied to floating point value"))
                );
                break;
            case OpType.Load16:
                MultitypeOp(_sm.LoadI16,
                    () => Thrower.Throw(
                        new InvalidOperationException("This operation cannot be applied to floating point value"))
                );
                break;
            case OpType.Load8:
                MultitypeOp(_sm.LoadI8,
                    () => Thrower.Throw(
                        new InvalidOperationException("This operation cannot be applied to floating point value"))
                );
                break;
            case OpType.I8ToI64:
            case OpType.I16ToI64:
            case OpType.I32ToI64:
            case OpType.I64ToI8:
            case OpType.I64ToI16:
            case OpType.I64ToI32:
            case OpType.I64ToF64:
            case OpType.F64ToI64:
                _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), op.OpType.ToString()));
                break;
            case OpType.Eq:
                if (_sm.GetTypeInTop() == AsmType.I64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.EqI)));
                else if (_sm.GetTypeInTop() == AsmType.F64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.EqF)));
                else Thrower.Throw(new InvalidOperationException("Invalid type"));
                break;
            case OpType.Neq:
                if (_sm.GetTypeInTop() == AsmType.I64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.NeqI)));
                else if (_sm.GetTypeInTop() == AsmType.F64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.NeqF)));
                else Thrower.Throw(new InvalidOperationException("Invalid type"));
                break;
            case OpType.Lt:
                if (_sm.GetTypeInTop() == AsmType.I64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.LtI)));
                else if (_sm.GetTypeInTop() == AsmType.F64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.LtF)));
                else Thrower.Throw(new InvalidOperationException("Invalid type"));
                break;
            case OpType.Le:
                break;
            case OpType.Gt:
                break;
            case OpType.Ge:
                break;
            case OpType.Br:
                _asm.jmp(_labelsManager.GetOrAddLabel(op.Arg<string>(0)));
                break;
            case OpType.BrOne:
                CmpAndJump(op, 1);
                break;
            case OpType.BrZero:
                CmpAndJump(op, 0);
                break;
            case OpType.Ret:
                _asm.jmp(_labelsManager.GetOrAddLabel("return_label"));
                break;
            case OpType.Add:
                MultitypeOp(() => BinaryOperation(_asm.add), () => BinaryOperation(_asm.addsd));
                break;
            case OpType.Sub:
                MultitypeOp(() => BinaryOperation(_asm.sub), () => BinaryOperation(_asm.subsd));
                break;
            case OpType.Mul:
                MultitypeOp(() => BinaryOperation(_asm.imul), () => BinaryOperation(_asm.mulsd));
                break;
            case OpType.Div:
                MultitypeOp(() =>
                {
                    _asm.xor(rdx, rdx);
                    BinaryOperation((_, b) => _asm.idiv(b));
                }, () => BinaryOperation(_asm.divsd));
                break;
            case OpType.Mod:
                MultitypeOp(() =>
                    {
                        _asm.xor(rdx, rdx);
                        BinaryOperation((_, b) => _asm.idiv(b), rdx);
                    },
                    () => _callManager.Call(
                        ReflectionManager.Get(typeof(VlRuntimeHelper),
                            nameof(VlRuntimeHelper.RemF64))
                    )
                );
                break;
            case OpType.SetLabel:
                _asm.Label(ref _labelsManager.GetOrAddLabel(op.Arg<string>(0)));
                break;
            case OpType.CallSharp:
                var tuple = ReflectionManager.Get(
                    op.Arg<Type>(0),
                    op.Arg<string>(1),
                    op.Arg<Type[]>(2)
                );
                _callManager.Call(tuple);
                break;
            case OpType.CallFunc:
                break;
            case OpType.CallAddress:
                break;
            case OpType.LocAddress:
                _asm.mov(rax, rbp);
                _asm.sub(rax, _localsManager.GetOrAddLocal(op.Arg<string>(0)));
                _sm.PushAddress(rax, op.Arg<AsmType>(1));
                break;
            case OpType.Dup:
                _sm.Dup();
                break;
            default:
                Thrower.Throw<object>(new ArgumentOutOfRangeException());
                break;
        }
    }

    private static Type[] GetGenerics(OpType t)
    {
        return t switch
        {
            OpType.I8ToI64 => [typeof(sbyte), typeof(long)],
            OpType.I16ToI64 => [typeof(short), typeof(long)],
            OpType.I32ToI64 => [typeof(int), typeof(long)],
            OpType.I64ToI8 => [typeof(long), typeof(sbyte)],
            OpType.I64ToI16 => [typeof(long), typeof(short)],
            OpType.I64ToI32 => [typeof(long), typeof(int)],
            OpType.I64ToF64 => [typeof(long), typeof(double)],
            OpType.F64ToI64 => [typeof(double), typeof(long)],
            _ => Thrower.Throw<Type[]>(new ArgumentOutOfRangeException(nameof(t), t, null))
        };
    }

    private void PushConst<T>(T value) where T : struct =>
        _sm.Push(__[_dataManager.DefineData(value)], value is int or long ? AsmType.I64 : AsmType.F64);

    private void CmpAndJump(Op op, int cmpValue)
    {
        _sm.PopReg(rax);
        _asm.cmp(rax, cmpValue);
        _asm.je(_labelsManager.GetOrAddLabel(op.Arg<string>(0)));
    }

    private void BinaryOperation(Action<AssemblerRegister64, AssemblerRegister64> act,
        AssemblerRegister64 outputReg = default)
    {
        _sm.PopReg(r10);
        _sm.PopReg(rax);

        act(rax, r10);

        _sm.Push(outputReg == default ? rax : outputReg);
    }

    private void BinaryOperation(Action<AssemblerRegisterXMM, AssemblerRegisterXMM> act)
    {
        _sm.PopReg(xmm1);
        _sm.PopReg(xmm0);

        act(xmm0, xmm1);

        _sm.Push(xmm0);
    }

    private void MultitypeOp(Action i64, Action f64)
    {
        if (_sm.GetTypeInTop() == AsmType.I64)
            i64();
        else if (_sm.GetTypeInTop() == AsmType.F64)
            f64();
        else Thrower.Throw(new InvalidOperationException("Invalid type"));
    }
}