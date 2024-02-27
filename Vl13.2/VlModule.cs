namespace Vl13._2;

using Iced.Intel;

public class VlModule
{
    public Assembler Assembler = null!;
    public RefDictionary<string, Label> FunctionsLabels = null!;
    public StackManager StackManager = null!;
    public IDebugData DebugData = null!;
    public List<VlImageInfo> Images = null!;
    public TranslateData TranslateData = null!;
}