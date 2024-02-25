namespace Vl13._2;

using Iced.Intel;

public class StackManager(Assembler asm, StackPositioner sp)
{
    private readonly Stack<AsmType> _types = new();

    public void AddTypes(AsmType[] asmTypes)
    {
        foreach (var asmType in asmTypes)
            _types.Push(asmType);
    }

    public AsmType GetTypeInTop() =>
        _types.Peek();

    public void Pop(AssemblerRegister64 reg)
    {
        asm.mov(reg, sp.Prev());
        _types.Pop();
    }

    public void Pop(AssemblerRegisterXMM reg)
    {
        asm.movq(reg, sp.Prev());
        _types.Pop();
    }

    public void Push(AssemblerRegister64 reg)
    {
        _types.Push(AsmType.I64);
        sp.Next(reg);
    }

    public void Push(AssemblerRegisterXMM reg)
    {
        _types.Push(AsmType.F64);
        sp.Next(reg);
    }

    public void Drop()
    {
        sp.Prev();
        _types.Pop();
    }

    public void Skip()
    {
        _types.Push(AsmType.None);
        sp.Next(null);
    }

    public void PopRegs(params AssemblerRegister64[] regs) =>
        Array.ForEach(regs, Pop);

    public void Push(AssemblerMemoryOperand memOp, AsmType refType)
    {
        asm.mov(rax, memOp);
        sp.Next(rax);
        _types.Push(refType);
    }

    public void Load64(AsmType t)
    {
        Pop(rax);
        asm.mov(rax, __[rax]);
        sp.Next(rax);
        _types.Push(t);
    }

    public void LoadI32()
    {
        Pop(rax);
        asm.mov(eax, __[rax]);
        sp.Next(() => asm.mov(sp.Peek(), eax));
        _types.Push(AsmType.I64);
    }

    public void LoadI16()
    {
        Pop(rax);
        asm.mov(ax, __[rax]);
        asm.and(rax, ushort.MaxValue); // zero extra bits
        sp.Next(() => asm.mov(sp.Peek(), ax));
        _types.Push(AsmType.I64);
    }

    public void LoadI8()
    {
        Pop(rax);
        asm.mov(al, __[rax]);
        asm.and(rax, byte.MaxValue); // zero extra bits
        sp.Next(() => asm.mov(sp.Peek(), al));
        _types.Push(AsmType.I64);
    }


    public void Dup() =>
        Push(sp.Peek() - 8, GetTypeInTop());

    public void PushAddress(AssemblerRegister64 reg, AsmType refType)
    {
        _types.Push(refType);
        sp.Next(reg);
    }

    public void SubTypes(int count)
    {
        for (var i = 0; i < count; i++)
            _types.Pop();
    }
}