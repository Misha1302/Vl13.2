namespace Vl13._2;

using Iced.Intel;

/// <summary>
///     https://en.wikipedia.org/wiki/X86_calling_conventions
/// </summary>
public class VlFunction(VlImageInfo imageInfo, VlModule module)
{
    private readonly DataManager _dataManager = new(module.Assembler);
    private readonly LabelsManager _labelsManager = new(module.Assembler);
    private readonly CallManager _callManager = new(module.Assembler, module.StackManager);
    private readonly LocalsManager _localsManager = new();

    public void Translate()
    {
        module.StackManager.ResetTypes();
        module.StackManager.AddTypes(imageInfo.ArgTypes);
        _labelsManager.GetOrAddLabel("return_label");

        EmitDebug(new Op(OpType.Function, imageInfo.Name));
        ValidateImage();

        module.Assembler.Label(ref module.FunctionsLabels[imageInfo.Name]);

        Prolog();
        foreach (var op in imageInfo.Image.Ops)
            EmitOp(op);

        _dataManager.EmitData();
    }

    private void Prolog()
    {
        module.Assembler.push(rbp);
        module.Assembler.mov(rbp, rsp);
        module.Assembler.sub(rsp, imageInfo.GetTotalSize(module));
    }

    private void EmitDebug(Op op)
    {
        module.DebugData.Emit(op, module.Assembler.Instructions.Count);
    }

    private void ValidateImage()
    {
        if (imageInfo.Image.Ops[^1].OpType is not OpType.Ret and not OpType.End)
            Thrower.Throw(new InvalidOperationException());
    }

    private void EmitOp(Op op)
    {
        EmitDebug(op);

        switch (op.OpType)
        {
            case OpType.Init:
                module.Assembler.push(rsi);
                module.Assembler.push(rdi);

                module.Assembler.push(r15);
                module.Assembler.push(r14);

                module.Assembler.mov(r14, 0);
                module.Assembler.mov(rcx, module.TranslateData.StackMaxSizeIn64);
                module.Assembler.call(ReflectionManager.GetPtr(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.Alloc)));
                module.Assembler.mov(r15, rax);

                break;
            case OpType.End:
                module.StackManager.Pop(rax);

                module.Assembler.push(rax);
                module.Assembler.push(rax);
                module.Assembler.mov(rcx, r15);
                module.Assembler.call(ReflectionManager.GetPtr(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.Free)));
                module.Assembler.pop(rax);
                module.Assembler.pop(rax);

                module.Assembler.pop(r14);
                module.Assembler.pop(r15);

                module.Assembler.pop(rdi);
                module.Assembler.pop(rsi);

                module.Assembler.mov(rsp, rbp);
                module.Assembler.pop(rbp);

                module.Assembler.ret();
                break;
            case OpType.Push:
                if (op.Params?[0] is long)
                    PushConst(op.Arg<long>(0));
                else PushConst(op.Arg<double>(0));
                break;
            case OpType.Drop:
                module.StackManager.Drop();
                break;
            case OpType.Store64:
                module.StackManager.PopRegs(r10, rax); // reference, value
                module.Assembler.mov(__[r10], rax); // *(&variable) = value
                break;
            case OpType.Store32:
                module.StackManager.PopRegs(r10, rax); // reference, value
                module.Assembler.mov(__[r10], eax); // *(&variable) = value
                break;
            case OpType.Store16:
                module.StackManager.PopRegs(r10, rax); // reference, value
                module.Assembler.mov(__[r10], ax); // *(&variable) = value
                break;
            case OpType.Store8:
                module.StackManager.PopRegs(r10, rax); // reference, value
                module.Assembler.mov(__[r10], al); // *(&variable) = value
                break;
            case OpType.Load64:
                MultitypeOp(() => module.StackManager.Load64(AsmType.I64),
                    () => module.StackManager.Load64(AsmType.F64));
                break;
            case OpType.Load32:
                MultitypeOp(module.StackManager.LoadI32,
                    () => Thrower.Throw(
                        new InvalidOperationException("This operation cannot be applied to floating point value"))
                );
                break;
            case OpType.Load16:
                MultitypeOp(module.StackManager.LoadI16,
                    () => Thrower.Throw(
                        new InvalidOperationException("This operation cannot be applied to floating point value"))
                );
                break;
            case OpType.Load8:
                MultitypeOp(module.StackManager.LoadI8,
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
                if (module.StackManager.GetTypeInTop() == AsmType.I64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.EqI)));
                else if (module.StackManager.GetTypeInTop() == AsmType.F64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.EqF)));
                else Thrower.Throw(new InvalidOperationException("Invalid type"));
                break;
            case OpType.Neq:
                if (module.StackManager.GetTypeInTop() == AsmType.I64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.NeqI)));
                else if (module.StackManager.GetTypeInTop() == AsmType.F64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.NeqF)));
                else Thrower.Throw(new InvalidOperationException("Invalid type"));
                break;
            case OpType.Lt:
                if (module.StackManager.GetTypeInTop() == AsmType.I64)
                    _callManager.Call(ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.LtI)));
                else if (module.StackManager.GetTypeInTop() == AsmType.F64)
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
                module.Assembler.jmp(_labelsManager.GetOrAddLabel(op.Arg<string>(0)));
                break;
            case OpType.BrOne:
                CmpAndJump(op, 1);
                break;
            case OpType.BrZero:
                CmpAndJump(op, 0);
                break;
            case OpType.Ret:
                module.Assembler.mov(rsp, rbp);
                module.Assembler.pop(rbp);
                module.Assembler.ret();
                break;
            case OpType.Add:
                MultitypeOp(() => BinaryOperation(module.Assembler.add), () => BinaryOperation(module.Assembler.addsd));
                break;
            case OpType.Sub:
                MultitypeOp(() => BinaryOperation(module.Assembler.sub), () => BinaryOperation(module.Assembler.subsd));
                break;
            case OpType.Mul:
                MultitypeOp(() => BinaryOperation(module.Assembler.imul),
                    () => BinaryOperation(module.Assembler.mulsd));
                break;
            case OpType.Div:
                MultitypeOp(() =>
                {
                    module.Assembler.xor(rdx, rdx);
                    BinaryOperation((_, b) => module.Assembler.idiv(b));
                }, () => BinaryOperation(module.Assembler.divsd));
                break;
            case OpType.Mod:
                MultitypeOp(() =>
                    {
                        module.Assembler.xor(rdx, rdx);
                        BinaryOperation((_, b) => module.Assembler.idiv(b), rdx);
                    },
                    () => _callManager.Call(
                        ReflectionManager.Get(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.RemF64))
                    )
                );
                break;
            case OpType.SetLabel:
                module.Assembler.Label(ref _labelsManager.GetOrAddLabel(op.Arg<string>(0)));
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
                var fName = op.Arg<string>(0);
                var f = module.Images.First(x => x.Name == fName);

                module.StackManager.SubTypes(f.ArgTypes.Length);
                module.Assembler.call(module.FunctionsLabels[fName]);
                module.StackManager.AddTypes([f.ReturnType]);
                break;
            case OpType.CallAddress:
                break;
            case OpType.LocAddress:
                module.Assembler.mov(rax, rbp);
                module.Assembler.sub(rax, _localsManager.GetOrAddLocal(op.Arg<string>(0)));
                module.StackManager.PushAddress(rax, op.Arg<AsmType>(1));
                break;
            case OpType.Dup:
                module.StackManager.Dup();
                break;
            case OpType.Prolog:
            case OpType.Epilogue:
            case OpType.Body:
                break;
            default:
                Thrower.Throw<object>(new ArgumentOutOfRangeException());
                break;
        }
    }

    private void PushConst<T>(T value) where T : struct =>
        module.StackManager.Push(__[_dataManager.DefineData(value)], value is int or long ? AsmType.I64 : AsmType.F64);

    private void CmpAndJump(Op op, int cmpValue)
    {
        module.StackManager.Pop(rax);
        module.Assembler.cmp(rax, cmpValue);
        module.Assembler.je(_labelsManager.GetOrAddLabel(op.Arg<string>(0)));
    }

    private void BinaryOperation(Action<AssemblerRegister64, AssemblerRegister64> act,
        AssemblerRegister64 outputReg = default)
    {
        module.StackManager.Pop(r10);
        module.StackManager.Pop(rax);

        act(rax, r10);

        module.StackManager.Push(outputReg == default ? rax : outputReg);
    }

    private void BinaryOperation(Action<AssemblerRegisterXMM, AssemblerRegisterXMM> act)
    {
        module.StackManager.Pop(xmm1);
        module.StackManager.Pop(xmm0);

        act(xmm0, xmm1);

        module.StackManager.Push(xmm0);
    }

    private void MultitypeOp(Action i64, Action f64)
    {
        if (module.StackManager.GetTypeInTop() == AsmType.I64)
            i64();
        else if (module.StackManager.GetTypeInTop() == AsmType.F64)
            f64();
        else Thrower.Throw(new InvalidOperationException("Invalid type"));
    }
}