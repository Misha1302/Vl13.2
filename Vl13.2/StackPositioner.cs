namespace Vl13._2;

using Iced.Intel;

public class StackPositioner(Assembler asm, AssemblerRegister64 index, AssemblerRegister64 pointer)
{
    private AssemblerMemoryOperand StackPos =>
        __[pointer + index * 8];

    public void Next(AssemblerRegister64 reg) =>
        Next(() => asm.mov(StackPos, reg));

    public void Next(AssemblerRegisterXMM reg) =>
        Next(() => asm.movq(StackPos, reg));

    public AssemblerMemoryOperand Prev()
    {
        asm.dec(index);
        return StackPos;
    }

    public AssemblerMemoryOperand Peek() => StackPos;


    public void Next(Action? act)
    {
        act?.Invoke();
        asm.inc(index);
    }
}