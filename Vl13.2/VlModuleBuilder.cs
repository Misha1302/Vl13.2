namespace Vl13._2;

public class VlModuleBuilder
{
    public readonly List<VlImageInfo> ImageInfos = [];
    private readonly Dictionary<string, Dictionary<string, AsmType>> _structures = new();


    public VlModuleBuilder()
    {
        CreateInitFunction();
    }

    public IReadOnlyDictionary<string, Dictionary<string, AsmType>> Structures => _structures;

    private void CreateInitFunction()
    {
        var init = AddFunction("init", this, [], AsmType.None, []);
        init.Init();
        init.CallFunc("main");
        init.End();
    }

    public AsmFunctionBuilder AddFunction(
        string name,
        VlModuleBuilder module,
        Mli[] args,
        AsmType returnType,
        Mli[] locals
    )
    {
        var argsInfos = ToInfos(args, out var localsStructures);
        var localsInfos = ToInfos(locals, out localsStructures);

        var func = new AsmFunctionBuilder(name, module, argsInfos.Select(x => x.Type).ToArray(), returnType);
        func.DeclareLocals(localsInfos.Union(argsInfos).ToArray(), localsStructures);

        foreach (var t in argsInfos)
            func.SetLocal(t.Name, null);

        ImageInfos.Add(func);
        return func;
    }

    private List<LocalInfo> ToInfos(Mli[] arr, out Dictionary<string, string> localsStructures)
    {
        var locals = new List<LocalInfo>();
        localsStructures = new Dictionary<string, string>();

        foreach (var t in arr)
            locals.AddRange(ToLocals(t, localsStructures));

        return locals;
    }

    private List<LocalInfo> ToLocals(Mli t, Dictionary<string, string> localsStructures)
    {
        if (_structures.TryGetValue(t.Type, out var value))
        {
            localsStructures.Add(t.Name, t.Type);
            return value.Select(x => new LocalInfo(x.Value, t.Name + "_" + x.Key)).ToList();
        }

        return [new LocalInfo(Enum.Parse<AsmType>(t.Type), t.Name)];
    }

    public void AddStructure(string typeName, Dictionary<string, AsmType> structure)
    {
        _structures.Add(typeName, structure);
    }
}