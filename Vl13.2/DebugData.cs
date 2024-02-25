namespace Vl13._2;

public class DebugData : IDebugData
{
    private readonly List<(int, Op)> _data = [];

    public ILookup<int, Op> Data => _data.ToLookup(x => x.Item1, x => x.Item2);

    public void Emit(Op op, int asmInstructionIndex)
    {
        _data.Add((asmInstructionIndex, op));
    }
}