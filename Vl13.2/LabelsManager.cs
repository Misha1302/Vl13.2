namespace Vl13._2;

using Iced.Intel;

public class LabelsManager(Assembler asm)
{
    private readonly GetOrAddCollection<Label> _col = new(asm.CreateLabel);

    public ref Label GetOrAddLabel(string name) => ref _col.GetOrAdd(name);
}