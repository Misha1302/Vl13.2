namespace Vl13._2;

public class VlImage
{
    private readonly List<Op> _ops = [];
    public IReadOnlyList<Op> Ops => _ops;

    public void Emit(Op o) => _ops.Add(o);
}