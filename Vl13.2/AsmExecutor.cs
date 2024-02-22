namespace Vl13._2;

using System.Runtime.InteropServices;
using Iced.Intel;

public static class AsmExecutor
{
    private const uint PageExecuteReadwrite = 0x40;
    private const uint MemCommit = 0x1000;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    public static void PrintCode(Assembler asm)
    {
        foreach (var i in asm.Instructions)
            Console.WriteLine(i.ToString().Replace(",", ", "));
    }

    public static nint MakeFunction(Assembler asm)
    {
        const ulong rip = 0x10;
        var stream = new MemoryStream();
        asm.Assemble(new StreamCodeWriter(stream), rip);

        var ptr = VirtualAlloc(IntPtr.Zero, (uint)stream.Length, MemCommit, PageExecuteReadwrite);
        Marshal.Copy(stream.ToArray(), 0, ptr, (int)stream.Length);

        return ptr;
    }
}