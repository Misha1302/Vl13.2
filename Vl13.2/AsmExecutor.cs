namespace Vl13._2;

using System.Runtime.InteropServices;
using Iced.Intel;

public static partial class AsmExecutor
{
    private const uint PageExecuteReadwrite = 0x40;
    private const uint MemCommit = 0x1000;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    public static void PrintCode(Assembler asm, DebugData debugData)
    {
        for (var index = 0; index < asm.Instructions.Count; index++)
        {
            var i = asm.Instructions[index];

            foreach (var op in debugData.Data[index])
                Console.WriteLine($"\n>>>>>> {op}");

            Console.WriteLine(i.ToString().Replace(",", ", "));
        }
    }

    public static unsafe delegate*<T> MakeFunction<T>(Assembler asm)
    {
        const ulong rip = 0x10;
        var stream = new MemoryStream();
        asm.Assemble(new StreamCodeWriter(stream), rip);

        var ptr = VirtualAlloc(IntPtr.Zero, (uint)stream.Length, MemCommit, PageExecuteReadwrite);
        Marshal.Copy(stream.ToArray(), 0, ptr, (int)stream.Length);

        return (delegate*<T>)ptr;
    }

    public static void PrintCode(VlModuleBuilder module)
    {
        foreach (var structure in module.Structures)
            Console.WriteLine($"struct {structure.Key} [{string.Join(", ", structure.Value)}]");

        foreach (var image in module.Compile())
        {
            Console.WriteLine($">>> >>> >>> image: {image.Name}");

            foreach (var op in image.Image.Ops)
                Console.WriteLine(op);
        }
    }
}