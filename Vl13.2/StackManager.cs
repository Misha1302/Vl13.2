namespace Vl13._2;

using Iced.Intel;

public class StackManager(VlModule module, StackPositioner sp)
{
    private readonly Stack<AsmType> _types = new();

    public void AddTypes(AsmType[] asmTypes)
    {
        foreach (var type in asmTypes)
            _types.Push(type);
    }

    public AsmType GetTypeInTop() =>
        _types.Peek();

    public void Pop(AssemblerRegister64 reg)
    {
        Pop(() => module.Assembler.mov(reg, sp.Prev()));
    }

    public void Pop(AssemblerRegisterXMM reg)
    {
        Pop(() => module.Assembler.movq(reg, sp.Prev()));
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
        Pop(() => sp.Prev());
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
        module.Assembler.mov(rax, memOp);
        sp.Next(rax);
        _types.Push(refType);
    }

    public void Load64(AsmType t)
    {
        Pop(rax);
        module.Assembler.mov(rax, __[rax]);
        sp.Next(rax);
        _types.Push(t);
    }

    public void LoadI32()
    {
        Pop(rax);
        module.Assembler.mov(eax, __[rax]);
        sp.Next(() => module.Assembler.mov(sp.Peek(), eax));
        _types.Push(AsmType.I64);
    }

    public void LoadI16()
    {
        Pop(rax);
        module.Assembler.mov(ax, __[rax]);
        module.Assembler.and(rax, ushort.MaxValue); // zero extra bits
        sp.Next(() => module.Assembler.mov(sp.Peek(), ax));
        _types.Push(AsmType.I64);
    }

    public void LoadI8()
    {
        Pop(rax);
        module.Assembler.mov(al, __[rax]);
        module.Assembler.and(rax, byte.MaxValue); // zero extra bits
        sp.Next(() => module.Assembler.mov(sp.Peek(), al));
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
            Pop(null);
    }

    public void ResetTypes()
    {
        _types.Clear();
    }


    private void Pop(Action? act)
    {
        act?.Invoke();

        /*
        if (ulong)index > (ulong)maxIndexValue then
            int3
        */

        if (module.TranslateData.CheckStackOverflow)
        {
            var @else = module.Assembler.CreateLabel();

            module.Assembler.cmp(sp.Index, sp.MaxIndexValue);
            module.Assembler.jbe(@else);

            module.Assembler.int3();

            module.Assembler.Label(ref @else);
        }

        _types.Pop();
    }
}