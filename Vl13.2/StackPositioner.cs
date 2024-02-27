namespace Vl13._2;

using Iced.Intel;

public record StackPositioner(Assembler Asm, AssemblerRegister64 Index, AssemblerRegister64 Pointer, int MaxIndexValue)
{
    private AssemblerMemoryOperand StackPos =>
        __[Pointer + Index * 8];

    public void Next(AssemblerRegister64 reg) =>
        Next(() => Asm.mov(StackPos, reg));

    public void Next(AssemblerRegisterXMM reg) =>
        Next(() => Asm.movq(StackPos, reg));

    public AssemblerMemoryOperand Prev()
    {
        Asm.dec(Index);
        return StackPos;
    }

    public AssemblerMemoryOperand Peek() => StackPos;


    public void Next(Action? act)
    {
        act?.Invoke();
        Asm.inc(Index);
    }
}