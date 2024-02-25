namespace Vl13._2;

using Iced.Intel;

public class VlTranslator(VlModule vlModule)
{
    public Assembler Translate(IDebugData debugData)
    {
        var assembler = new Assembler(64);

        foreach (var image in vlModule.ImageFactories)
            new VlFunction(image, assembler, debugData).Translate();

        return assembler;
    }
}