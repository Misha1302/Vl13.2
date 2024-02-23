namespace Vl13._2;

using Iced.Intel;

public class EnterLeaveFunctionManager(
    Assembler asm,
    StackManager sm,
    VlImageInfo imageInfo,
    LabelsManager labelsManager,
    Action<Op> emitOp
)
{
    public void Epilogue()
    {
        asm.Label(ref labelsManager.GetOrAddLabel("return_label"));

        sm.PopReg(AssemblerRegisters.rax);
        asm.mov(AssemblerRegisters.rsp, AssemblerRegisters.rbp);
        asm.pop(AssemblerRegisters.rdi);
        asm.pop(AssemblerRegisters.rsi);
        asm.pop(AssemblerRegisters.rbp);
        asm.ret();
    }

    public void Prolog()
    {
        asm.push(AssemblerRegisters.rbp);
        asm.push(AssemblerRegisters.rsi);
        asm.push(AssemblerRegisters.rdi);
        asm.mov(AssemblerRegisters.rbp, AssemblerRegisters.rsp);

        asm.sub(AssemblerRegisters.rsp, imageInfo.GetTotalSize());
    }

    public void Body()
    {
        foreach (var op in imageInfo.Image.Ops)
            emitOp(op);
    }
}