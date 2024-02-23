namespace Vl13._2;

using Iced.Intel;

public class StackManager(Assembler asm, StackPositioner sp)
{
    public void PopReg(AssemblerRegister64 reg) => asm.mov(reg, sp.Prev());
    public void PopRegs(params AssemblerRegister64[] regs) => Array.ForEach(regs, PopReg);
    public void PopReg(AssemblerRegisterXMM reg) => asm.movq(reg, sp.Prev());

    public void Push(AssemblerRegister64 reg) => asm.mov(sp.Next(), reg);
    public void Push(AssemblerRegisterXMM reg) => asm.movq(sp.Next(), reg);

    public void Drop() => sp.Prev();
    public void Skip() => sp.Next();

    public void Push(AssemblerMemoryOperand memOp)
    {
        asm.mov(rax, memOp);
        Push(rax);
    }

    public void LoadI64()
    {
        PopReg(rax);
        asm.mov(rax, __[rax]);
        asm.mov(sp.Next(), rax);
    }

    public void LoadI32()
    {
        PopReg(rax);
        asm.mov(eax, __[rax]);
        asm.mov(sp.Next(), eax);
    }

    public void LoadI16()
    {
        PopReg(rax);
        asm.mov(ax, __[rax]);
        asm.and(rax, ushort.MaxValue); // zero extra bits
        asm.mov(sp.Next(), ax);
    }

    public void LoadI8()
    {
        PopReg(rax);
        asm.mov(al, __[rax]);
        asm.and(rax, byte.MaxValue); // zero extra bits
        asm.mov(sp.Next(), al);
    }

    public void Dup() => Push(sp.Peek() + 8);
}