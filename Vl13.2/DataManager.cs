namespace Vl13._2;

using System.Runtime.CompilerServices;
using Iced.Intel;

public class DataManager(Assembler asm)
{
    private readonly Dictionary<Label, long> _keyLabelValueData = new();

    public void EmitData()
    {
        foreach (var pair in _keyLabelValueData)
        {
            var label = pair.Key;
            asm.Label(ref label);

            asm.dq(pair.Value);
        }
    }

    public Label DefineData<T>(T value) where T : struct
    {
        var label = asm.CreateLabel($"_data[{value}]");
        _keyLabelValueData.Add(label, Unsafe.BitCast<T, long>(value));
        return label;
    }
}