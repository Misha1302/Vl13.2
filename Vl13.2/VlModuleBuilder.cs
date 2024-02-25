namespace Vl13._2;

public class VlModuleBuilder
{
    public readonly List<VlImageInfo> ImageInfos = [];

    public VlModuleBuilder()
    {
        CreateInitFunction();
    }

    private void CreateInitFunction()
    {
        var init = AddFunction("init", [], AsmType.None, []);
        init.Init();
        init.CallFunc("main");
        init.End();
    }

    public AsmFunctionBuilder AddFunction(string name, LocalInfo[] args, AsmType returnType, LocalInfo[] locals)
    {
        var func = new AsmFunctionBuilder(name, args.Select(x => x.Type).ToArray(), returnType);
        func.DeclareLocals(locals.Union(args).ToArray());

        foreach (var t in args)
            func.SetLocal(t.Name, null);

        ImageInfos.Add(func);
        return func;
    }
}