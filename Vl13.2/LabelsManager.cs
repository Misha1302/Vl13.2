namespace Vl13._2;

public class LabelsManager(VlModule module)
{
    private readonly GetOrAddCollection<VlLabel> _col = new(name => new VlLabel(module.Assembler.CreateLabel(name)));
    public IReadOnlyDictionary<string, VlLabel> Labels => _col.Items;

    public VlLabel GetOrAddLabel(string name) => _col.GetOrAdd(name);
}