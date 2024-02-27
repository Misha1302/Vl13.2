namespace Vl13._2;

using Iced.Intel;

public class LabelsManager(VlModule module)
{
    private readonly GetOrAddCollection<Label> _col = new(module.Assembler.CreateLabel);

    public ref Label GetOrAddLabel(string name) => ref _col.GetOrAdd(name);
}