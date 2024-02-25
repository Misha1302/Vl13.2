namespace Vl13._2;

using Iced.Intel;

public class VlTranslator(VlModule vlModule)
{
    public Assembler Translate(IDebugData debugData)
    {
        var assembler = new Assembler(64);
        var sm = new StackManager(assembler, new StackPositioner(assembler, r14, r15));
        var functionLabels = vlModule.ImageFactories.Select(x => assembler.CreateLabel(x.Name)).ToList();

        foreach (var image in vlModule.ImageFactories)
            new VlFunction(image, assembler, debugData, functionLabels, sm).Translate();

        return assembler;
    }
}