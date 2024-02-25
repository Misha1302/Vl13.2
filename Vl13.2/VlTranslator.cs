namespace Vl13._2;

using Iced.Intel;

public class VlTranslator(List<VlImageInfo> images)
{
    public Assembler Translate(IDebugData debugData)
    {
        var asm = new Assembler(64);

        var module = new VlModule
        {
            Assembler = asm,
            StackManager = new StackManager(asm, new StackPositioner(asm, r14, r15)),
            DebugData = debugData,
            Images = images,
            FunctionsLabels = new RefDictionary<string, Label>(
                images.Select(x => new KeyValuePair<string, Label>(x.Name, asm.CreateLabel(x.Name)))
            )
        };

        foreach (var image in images)
            new VlFunction(image, module).Translate();

        return module.Assembler;
    }
}