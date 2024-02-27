namespace Vl13._2;

using System.Runtime.CompilerServices;
using Iced.Intel;

public class DataManager(VlModule module)
{
    private readonly Dictionary<Label, long> _keyLabelValueData = new();

    public void EmitData()
    {
        foreach (var pair in _keyLabelValueData)
        {
            var label = pair.Key;

            module.Assembler.Label(ref label);
            module.Assembler.dq(pair.Value);
        }
    }

    public Label DefineData<T>(T value) where T : struct
    {
        var label = module.Assembler.CreateLabel($"_data[{value}]");
        _keyLabelValueData.Add(label, Unsafe.BitCast<T, long>(value));
        return label;
    }
}