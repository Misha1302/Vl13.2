namespace Vl13._2;

using Iced.Intel;

public class StackManager(Assembler asm, StackPositioner sp)
{
    private readonly Stack<AsmType> _types = new();

    public AsmType GetTypeInTop() =>
        _types.Peek();

    public void PopReg(AssemblerRegister64 reg)
    {
        asm.mov(reg, sp.Prev());
        _types.Pop();
    }

    public void PopReg(AssemblerRegisterXMM reg)
    {
        asm.movq(reg, sp.Prev());
        _types.Pop();
    }

    public void Push(AssemblerRegister64 reg)
    {
        _types.Push(AsmType.I64);
        asm.mov(sp.Next(), reg);
    }

    public void Push(AssemblerRegisterXMM reg)
    {
        _types.Push(AsmType.F64);
        asm.movq(sp.Next(), reg);
    }

    public void Drop()
    {
        sp.Prev();
        _types.Pop();
    }

    public void Skip()
    {
        _types.Push(AsmType.None);
        sp.Next();
    }

    public void PopRegs(params AssemblerRegister64[] regs) =>
        Array.ForEach(regs, PopReg);

    public void Push(AssemblerMemoryOperand memOp, AsmType refType)
    {
        asm.mov(rax, memOp);
        asm.mov(sp.Next(), rax);
        _types.Push(refType);
    }

    public void Load64(AsmType t)
    {
        PopReg(rax);
        asm.mov(rax, __[rax]);
        asm.mov(sp.Next(), rax);
        _types.Push(t);
    }

    public void LoadI32()
    {
        PopReg(rax);
        asm.mov(eax, __[rax]);
        asm.mov(sp.Next(), eax);
        _types.Push(AsmType.I64);
    }

    public void LoadI16()
    {
        PopReg(rax);
        asm.mov(ax, __[rax]);
        asm.and(rax, ushort.MaxValue); // zero extra bits
        asm.mov(sp.Next(), ax);
        _types.Push(AsmType.I64);
    }

    public void LoadI8()
    {
        PopReg(rax);
        asm.mov(al, __[rax]);
        asm.and(rax, byte.MaxValue); // zero extra bits
        asm.mov(sp.Next(), al);
        _types.Push(AsmType.I64);
    }


    public void Dup() =>
        Push(sp.Peek() + 8, GetTypeInTop());

    public void PushAddress(AssemblerRegister64 reg, AsmType refType)
    {
        _types.Push(refType);
        asm.mov(sp.Next(), reg);
    }
}