namespace Vl13._2;

using Iced.Intel;

public class VlTranslator(List<VlImageInfo> images)
{
    public Assembler Translate(IDebugData debugData, TranslateData translateData)
    {
        var asm = new Assembler(64);

        var module = new VlModule
        {
            Assembler = asm,
            DebugData = debugData,
            Images = images,
            TranslateData = translateData
        };
        module.LabelsManager = new LabelsManager(module);
        module.StackManager =
            new StackManager(module, new StackPositioner(asm, r14, r15, translateData.StackMaxSizeIn64));

        foreach (var image in images)
            module.Translate(image);

        return module.Assembler;
    }
}