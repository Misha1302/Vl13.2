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

        sm.PopReg(rax);
        asm.mov(rsp, rbp);
        asm.pop(rdi);
        asm.pop(rsi);
        asm.pop(rbp);
        asm.ret();
    }

    public void Prolog()
    {
        asm.push(rbp);
        asm.push(rsi);
        asm.push(rdi);
        asm.mov(rbp, rsp);

        asm.sub(rsp, imageInfo.GetTotalSize());
    }

    public void Body()
    {
        foreach (var op in imageInfo.Image.Ops)
            emitOp(op);
    }
}