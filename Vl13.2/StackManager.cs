namespace Vl13._2;

using Iced.Intel;

public class StackManager(Assembler asm, StackPositioner sp)
{
    public void PopReg(AssemblerRegister64 reg) => asm.mov(reg, sp.Prev());
    public void PopReg(AssemblerRegisterXMM reg) => asm.movq(reg, sp.Prev());

    public void Push(AssemblerRegister64 reg) => asm.mov(sp.Next(), reg);
    public void Push(AssemblerRegisterXMM reg) => asm.movq(sp.Next(), reg);

    public void Drop() => sp.Prev();
    public void Skip() => sp.Next();
}