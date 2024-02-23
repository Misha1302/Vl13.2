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
            case OpType.PushI64:
                PushConst(op.Arg<long>(0));
                break;
            case OpType.PushF64:
                PushConst(op.Arg<double>(0));
                break;
            case OpType.Drop:
                _sm.Drop();
                break;
            case OpType.StoreI64:
                _sm.PopRegs(rax, r10); // value, reference
                _asm.mov(__[r10], rax); // *(&variable) = value
                break;
            case OpType.StoreI32:
                _sm.PopRegs(rax, r10); // value, reference
                _asm.mov(__[r10], eax); // *(&variable) = value
                break;
            case OpType.StoreI16:
                _sm.PopRegs(rax, r10); // value, reference
                _asm.mov(__[r10], ax); // *(&variable) = value
                break;
            case OpType.StoreI8:
                _sm.PopRegs(rax, r10); // value, reference
                _asm.mov(__[r10], al); // *(&variable) = value
                break;
            case OpType.StoreF64:
                _sm.PopRegs(rax, r10); // equals to storei64 'cause no need to save value to xmm0
                _asm.mov(__[r10], rax);
                break;
            case OpType.LoadI64:
                _sm.LoadI64();
                break;
            case OpType.LoadI32:
                _sm.LoadI32();
                break;
            case OpType.LoadI16:
                _sm.LoadI16();
                break;
            case OpType.LoadI8:
                _sm.LoadI8();
                break;
            case OpType.LoadF64:
                break;
            case OpType.I8ToI64:
                break;
            case OpType.I16ToI64:
                break;
            case OpType.I32ToI64:
                break;
            case OpType.I64ToI8:
                break;
            case OpType.I64ToI16:
                break;
            case OpType.I64ToI32:
                break;
            case OpType.I64ToF64:
                break;
            case OpType.F64ToI64:
                break;
            case OpType.Eq:
                _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.EqI)));
                break;
            case OpType.Neq:
                _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.NeqI)));
                break;
            case OpType.Lt:
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
            case OpType.AddI64:
                BinaryOperation(_asm.add);
                break;
            case OpType.AddF64:
                BinaryOperation(_asm.addsd);
                break;
            case OpType.SubI64:
                BinaryOperation(_asm.sub);
                break;
            case OpType.SubF64:
                BinaryOperation(_asm.subsd);
                break;
            case OpType.MulI64:
                BinaryOperation(_asm.imul);
                break;
            case OpType.MulF64:
                BinaryOperation(_asm.mulsd);
                break;
            case OpType.DivI64:
                _asm.xor(rdx, rdx);
                BinaryOperation((_, b) => _asm.idiv(b));
                break;
            case OpType.DivF64:
                BinaryOperation(_asm.divsd);
                break;
            case OpType.ModI64:
                _asm.xor(rdx, rdx);
                BinaryOperation((_, b) => _asm.idiv(b));
                break;
            case OpType.ModF64:
                _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.RemF64)));
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
                _sm.Push(rax);
                break;
            case OpType.Dup:
                _sm.Dup();
                break;
            default:
                Thrower.Throw<object>(new ArgumentOutOfRangeException());
                break;
        }
    }

    private void PushConst<T>(T value) where T : struct =>
        _sm.Push(__[_dataManager.DefineData(value)]);

    private void CmpAndJump(Op op, int cmpValue)
    {
        _sm.PopReg(rax);
        _asm.cmp(rax, cmpValue);
        _asm.je(_labelsManager.GetOrAddLabel(op.Arg<string>(0)));
    }

    private void BinaryOperation(Action<AssemblerRegister64, AssemblerRegister64> act)
    {
        _sm.PopReg(r10);
        _sm.PopReg(rax);

        act(rax, r10);

        _sm.Push(rax);
    }

    private void BinaryOperation(Action<AssemblerRegisterXMM, AssemblerRegisterXMM> act)
    {
        _sm.PopReg(xmm1);
        _sm.PopReg(xmm0);

        act(xmm0, xmm1);

        _sm.Push(xmm0);
    }
}