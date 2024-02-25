namespace Vl13._2;

using Iced.Intel;

public class VlModule
{
    public required Assembler Assembler;
    public required RefDictionary<string, Label> FunctionsLabels;
    public required StackManager StackManager;
    public required IDebugData DebugData;
    public required List<VlImageInfo> Images;
}