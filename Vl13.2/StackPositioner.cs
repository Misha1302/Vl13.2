namespace Vl13._2;

using Iced.Intel;

public class StackPositioner(int offset)
{
    private int _curStackPos = offset + 8;

    private AssemblerMemoryOperand StackPos =>
        __[rbp - _curStackPos];

    public AssemblerMemoryOperand Next()
    {
        var mem = StackPos;
        _curStackPos += 8;
        return mem;
    }

    public AssemblerMemoryOperand Prev()
    {
        if (_curStackPos <= offset)
            Thrower.Throw(new InvalidOperationException("Stack is clear"));

        _curStackPos -= 8;
        return StackPos;
    }

    public AssemblerMemoryOperand Peek() =>
        StackPos;
}