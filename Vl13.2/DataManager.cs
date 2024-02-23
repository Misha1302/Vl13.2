namespace Vl13._2;

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

    public Label DefineData(long value)
    {
        var label = asm.CreateLabel($"_data[{value}][{BitConverter.Int64BitsToDouble(value)}]");
        _keyLabelValueData.Add(label, value);
        return label;
    }

    public Label DefineData(double value) =>
        DefineData(BitConverter.DoubleToInt64Bits(value));
}