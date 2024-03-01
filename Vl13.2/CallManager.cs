namespace Vl13._2;

using System.Reflection;
using System.Runtime.CompilerServices;
using Iced.Intel;

public class CallManager(Assembler asm, StackManager sm)
{
    private static readonly (AssemblerRegister64, AssemblerRegisterXMM)[] _arr =
        [(rcx, xmm0), (rdx, xmm1), (r8, xmm2), (r9, xmm3)];

    private static void LoadArgsSharp(MethodBase mi, Assembler asm, StackManager sm, out int allocatedBytes)
    {
        allocatedBytes = 0;

        var parameters = mi.GetParameters();

        if (parameters.Length > _arr.Length && parameters.Length % 2 != 0)
        {
            asm.sub(rsp, 8);
            allocatedBytes += 8;
        }

        for (var index = parameters.Length - 1; index >= 0; index--)
            if (index < _arr.Length)
            {
                if (sm.GetTypeInTop() == AsmType.I64)
                    sm.Pop(_arr[index].Item1);
                else sm.Pop(_arr[index].Item2);
            }
            else
            {
                sm.Pop(rax);
                asm.push(rax);
                allocatedBytes += 8;
            }

        asm.sub(rsp, 32);
        allocatedBytes += 32;
    }

    private static void Clear(Assembler asm, int allocatedBytes)
    {
        asm.add(rsp, allocatedBytes);
    }

    public void Call((MethodInfo mi, nint ptr) tuple)
    {
        LoadArgsSharp(tuple.mi, asm, sm, out var allocatedBytes);
        asm.call((ulong)tuple.ptr);
        Clear(asm, allocatedBytes);
        PushReturnValue(tuple);
    }
    
    private void PushReturnValue((MethodInfo mi, nint ptr) tuple)
    {
        var rett = tuple.mi.ReturnType;

        if (rett == typeof(long) || rett == typeof(int) || rett == typeof(nint))
            sm.Push(rax);
        else if (rett == typeof(double))
            sm.Push(xmm0);
        else if (rett == typeof(void))
            sm.Skip(); // just skip. Need to be dropped
        else Thrower.Throw(new Exception("Invalid type"));
    }
}