namespace Vl13._2;

using Iced.Intel;

public class StackManager(VlModule module, StackPositioner sp)
{
    private readonly Stack<AsmType> _types = new();

    public void AddTypes(List<AsmType> asmTypes)
    {
        foreach (var type in asmTypes)
            Push(null, type);
    }

    public AsmType GetTypeInTop() =>
        _types.Peek();

    public int TypesCount() =>
        _types.Count;

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
        Push(() => sp.Next(reg), AsmType.I64);
    }

    public void Push(AssemblerRegisterXMM reg)
    {
        Push(() => sp.Next(reg), AsmType.F64);
    }

    public void Drop()
    {
        Pop(() => sp.Prev(), true);
    }

    public void Skip()
    {
        Push(() => sp.Next(null), AsmType.None);
    }

    public void PopRegs(params AssemblerRegister64[] regs) =>
        Array.ForEach(regs, Pop);

    public void Push(AssemblerMemoryOperand memOp, AsmType refType)
    {
        Push(() =>
        {
            module.Assembler.mov(rax, memOp);
            sp.Next(rax);
        }, refType);
    }

    public void Load64(AsmType t)
    {
        Push(() =>
        {
            Pop(rax);
            module.Assembler.mov(rax, __[rax]);
            sp.Next(rax);
        }, t);
    }

    public void LoadI32()
    {
        Push(() =>
        {
            Pop(rax);
            module.Assembler.mov(eax, __[rax]);
            sp.Next(() => module.Assembler.mov(sp.Peek(), eax));
        }, AsmType.I64);
    }

    public void LoadI16()
    {
        Push(() =>
        {
            Pop(rax);
            module.Assembler.mov(ax, __[rax]);
            module.Assembler.and(rax, ushort.MaxValue); // zero extra bits
            sp.Next(() => module.Assembler.mov(sp.Peek(), ax));
        }, AsmType.I64);
    }

    public void LoadI8()
    {
        Push(() =>
        {
            Pop(rax);
            module.Assembler.mov(al, __[rax]);
            module.Assembler.and(rax, byte.MaxValue); // zero extra bits
            sp.Next(() => module.Assembler.mov(sp.Peek(), al));
        }, AsmType.I64);
    }


    public void Dup() =>
        Push(sp.Peek() - 8, GetTypeInTop());

    public void PushAddress(AssemblerRegister64 reg, AsmType refType)
    {
        Push(() => sp.Next(reg), refType);
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


    private void Pop(Action? act, bool canBeNone = false)
    {
        if (!canBeNone && GetTypeInTop() == AsmType.None)
            Thrower.Throw(new InvalidOperationException("Invalid type"));

        act?.Invoke();
        CheckStackOverflowIfNeed();
        _types.Pop();
    }

    private void Push(Action? act, AsmType type)
    {
        act?.Invoke();
        CheckStackOverflowIfNeed();
        _types.Push(type);
    }

    private void CheckStackOverflowIfNeed()
    {
        if (!module.TranslateData.CheckStackOverflow)
            return;

        /*
        if (ulong)index > (ulong)maxIndexValue then
            int3
        */

        var @else = module.LabelsManager.GetOrAddLabel(Guid.NewGuid().ToString());

        module.Assembler.cmp(sp.Index, sp.MaxIndexValue);
        module.Assembler.jbe(@else.Label);

        module.Assembler.sub(rsp, 32);
        module.Assembler.mov(rcx, sp.Index);
        module.Assembler.call(ReflectionManager.GetPtr(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.StackOverflow)));
        module.Assembler.int3();

        module.CurrentFunction.SetLabel(@else);
    }
}