namespace Vl13._2;

public class VlModuleBuilder
{
    public readonly List<VlImageInfo> ImageInfos = [];
    private readonly Dictionary<string, Dictionary<string, AsmType>> _structures = new();
    public readonly Dictionary<string, LocalInfo> Globals;
    public readonly Dictionary<string, string> GlobalsOfStructureTypes = new();


    public VlModuleBuilder(params Mli[] globals)
    {
        var dictionary = new Dictionary<string, LocalInfo>();

        foreach (var info in globals)
        {
            foreach (var loc in ToLocals(info, GlobalsOfStructureTypes))
                dictionary.Add(info.Name, loc);
        }

        Globals = dictionary;
        CreateInitFunction();
    }

    public IReadOnlyDictionary<string, Dictionary<string, AsmType>> Structures => _structures;

    private void CreateInitFunction()
    {
        var init = AddFunction("init", this, [], AsmType.None, []);
        init.Init();
        init.CallFunc("main");
        init.End();

        foreach (var global in Globals)
            init.CreateDataLabel(global.Key);
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
            func.SetLocal(t.Name, null, false);

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
        if (!_structures.TryGetValue(t.Type, out var value))
            return [new LocalInfo(Enum.Parse<AsmType>(t.Type), t.Name, t.IsByRef)];

        localsStructures.Add(t.Name, t.Type);
        return value.Select(x => new LocalInfo(x.Value, t.Name + "_" + x.Key, t.IsByRef)).ToList();
    }

    public void AddStructure(string typeName, Dictionary<string, AsmType> structure)
    {
        _structures.Add(typeName, structure);
    }
}