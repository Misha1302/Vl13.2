namespace Vl13._2;

using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

/// <summary>
///     https://en.wikipedia.org/wiki/X86_calling_conventions
/// </summary>
public class VlFunction
{
    private readonly VlImageInfo _vlImageFactory;

    private readonly LabelsManager _labelsManager;
    private readonly Dictionary<Label, long> _keyLabelValueData = new();
    private readonly Assembler _asm;

    private readonly StackManager _sm;
    private readonly CallManager _callManager;
    private readonly LocalsManager _localsManager = new();

    public VlFunction(VlImageInfo vlImageFactory, Assembler asm)
    {
        _asm = asm;
        _vlImageFactory = vlImageFactory;
        _labelsManager = new LabelsManager(_asm);
        _sm = new StackManager(_vlImageFactory.LocalSizeInBytes);
        _callManager = new CallManager(_asm, _sm);
    }

    private int StackSizeInBytes
    {
        get
        {
            var stackSize = CalcStackSize();
            return stackSize * 8 % 16 == 0 ? stackSize * 8 : stackSize * 8 + 8;
        }
    }

    private int CalcStackSize()
    {
        var cur = 0;
        var max = 0;

        foreach (var op in _vlImageFactory.Image.Ops)
        {
            cur += op.OpType.StackOutput();
            max = Math.Max(max, cur);
        }

        return max + 16; // 16 - Reserved space for temporary computing
    }

    public void Translate()
    {
        _labelsManager.GetOrAddLabel("return_label");

        if (_vlImageFactory.Image.Ops[^1].OpType != OpType.Ret)
            Thrower.Throw(new InvalidOperationException());

        Prolog();
        Body();
        Epilogue();

        EmitData();
    }

    private void EmitData()
    {
        foreach (var pair in _keyLabelValueData)
        {
            var label = pair.Key;
            _asm.Label(ref label);

            _asm.dq(pair.Value);
        }
    }

    private void Body()
    {
        foreach (var op in _vlImageFactory.Image.Ops)
            EmitOp(op);
    }

    private void Epilogue()
    {
        _asm.Label(ref _labelsManager.GetOrAddLabel("return_label"));

        PopReg(rax);
        _asm.mov(rsp, rbp);
        _asm.pop(rdi);
        _asm.pop(rsi);
        _asm.pop(rbp);
        _asm.ret();
    }

    private void Prolog()
    {
        _asm.push(rbp);
        _asm.push(rsi);
        _asm.push(rdi);
        _asm.mov(rbp, rsp);

        _asm.sub(rsp, _vlImageFactory.LocalSizeInBytes + StackSizeInBytes);
    }

    private void EmitOp(Op op)
    {
        switch (op.OpType)
        {
            case OpType.PushI64:
                PushConstI64(op.Arg<long>(0));
                break;
            case OpType.PushF64:
                PushConstF64(op.Arg<double>(0));
                break;
            case OpType.Drop:
                _sm.Prev();
                break;
            case OpType.StoreI64:
                PopReg(rax); // value
                PopReg(r10); // reference
                _asm.mov(__[r10], rax); // *(&variable) = value
                break;
            case OpType.StoreI32:
                break;
            case OpType.StoreI16:
                break;
            case OpType.StoreI8:
                break;
            case OpType.StoreF64:
                break;
            case OpType.LoadI64:
                PopReg(rax); // reference
                _asm.mov(rax, __[rax]);
                _asm.mov(_sm.Next(), rax); // *(address) 
                break;
            case OpType.LoadI32:
                break;
            case OpType.LoadI16:
                break;
            case OpType.LoadI8:
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
                BinaryOperation((_, b) => _asm.idiv(b), rdx);
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

                _asm.mov(_sm.Next(), rax);
                break;
            default:
                Thrower.Throw<object>(new ArgumentOutOfRangeException());
                break;
        }
    }

    private void PushConstF64(double value)
    {
        _asm.mov(rax, __[DefineData(BitConverter.DoubleToInt64Bits(value))]);
        _asm.mov(_sm.Next(), rax);
    }

    private void PushConstI64(long value)
    {
        _asm.mov(rax, __[DefineData(value)]);
        _asm.mov(_sm.Next(), rax);
    }

    private void CmpAndJump(Op op, int cmdValue)
    {
        PopReg(rax);
        _asm.cmp(rax, cmdValue);
        _asm.je(_labelsManager.GetOrAddLabel(op.Arg<string>(0)));
    }

    private void BinaryOperation(Action<AssemblerRegister64, AssemblerRegister64> act,
        AssemblerRegister64 outputReg = default)
    {
        PopReg(r10);
        PopReg(rax);

        act(rax, r10);

        _asm.mov(_sm.Next(), outputReg == default ? rax : outputReg);
    }

    private void BinaryOperation(Action<AssemblerRegisterXMM, AssemblerRegisterXMM> act,
        AssemblerRegisterXMM outputReg = default)
    {
        PopReg(xmm1);
        PopReg(xmm0);

        act(xmm0, xmm1);

        _asm.movq(_sm.Next(), outputReg == default ? xmm0 : outputReg);
    }

    private void PopReg(AssemblerRegister64 reg) =>
        _asm.mov(reg, _sm.Prev());

    private void PopReg(AssemblerRegisterXMM reg) =>
        _asm.movq(reg, _sm.Prev());

    private Label DefineData(long value)
    {
        var l = _asm.CreateLabel($"_data[{value}][{BitConverter.Int64BitsToDouble(value)}]");
        _keyLabelValueData.Add(l, value);
        return l;
    }
}

public class LocalsManager
{
    private int _address = 8;

    private readonly GetOrAddCollection<int> _col;

    public LocalsManager()
    {
        _col = new GetOrAddCollection<int>(GetNextAddress);
    }

    private int GetNextAddress(string name)
    {
        _address += 8;
        return _address;
    }

    public int GetOrAddLocal(string name) => _col.GetOrAdd(name);
}

public class LabelsManager(Assembler asm)
{
    private readonly GetOrAddCollection<Label> _col = new(asm.CreateLabel);

    public ref Label GetOrAddLabel(string name) => ref _col.GetOrAdd(name);
}

public class GetOrAddCollection<T>(Func<string, T> valueCreator)
{
    private readonly Dictionary<string, T> _labels = new();

    public ref T GetOrAdd(string name)
    {
        if (!_labels.ContainsKey(name)) // no need to call func if key contains
            _labels.Add(name, valueCreator(name));

        return ref new[] { _labels[name] }[0];
    }
}