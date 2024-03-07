namespace Vl13._2;

public class VlModuleBuilder
{
    private readonly List<VlImageInfo> _imageInfos = [];
    private readonly Dictionary<StringType, Dictionary<string, AsmType>> _structures = new();

    public readonly Dictionary<string, LocalInfo> Globals = new();
    public readonly Dictionary<string, StringType> GlobalsOfStructureTypes = new();

    public VlModuleBuilder(params Mli[] globals)
    {
        AddGlobals(globals);
        CreateBuildinFunctions();
    }

    public IReadOnlyDictionary<StringType, Dictionary<string, AsmType>> Structures => _structures;

    public bool HasGlobal(string name) => Globals.ContainsKey(name) || GlobalsOfStructureTypes.ContainsKey(name);

    public void AddGlobals(Mli[] globals)
    {
        foreach (var info in globals)
            foreach (var loc in ToLocals(info, GlobalsOfStructureTypes))
                Globals.Add(info.Name, loc);
    }

    public List<VlImageInfo> Compile()
    {
        var index = _imageInfos.FindIndex(x => x.Name == "dataFunc");
        if (index != -1) _imageInfos.RemoveAt(index);

        var dataFunc = AddFunction("dataFunc", [], [], []);
        foreach (var global in Globals)
            dataFunc.CreateDataLabel(global.Key);

        return _imageInfos;
    }

    private void CreateBuildinFunctions() => CreateInitFunction();

    private void CreateInitFunction()
    {
        var init = AddFunction("init", [], [], []);
        init.Init();
        init.CallFunc("main");
        init.End();
    }

    public AsmFunctionBuilder AddFunction(string name, Mli[] returnValues, Mli[] args, Mli[] locals)
    {
        var localsStructures = new Dictionary<string, StringType>();
        var argsInfos = ToInfos(args, localsStructures);
        var returnsInfos = ToInfos(returnValues, localsStructures);
        var localsInfos = ToInfos(locals, localsStructures);

        var func = new AsmFunctionBuilder(name, this, returnsInfos.Union(argsInfos).Select(x => x.Type).ToArray(),
            returnsInfos.Count);
        func.DeclareLocals(localsInfos.Union(argsInfos).Union(returnsInfos).ToArray(), localsStructures);

        foreach (var t in returnsInfos)
            func.SetLocal(t.Name, null, false);
        for (var index = argsInfos.Count - 1; index >= 0; index--)
            func.SetLocal(argsInfos[index].Name, null, false);

        _imageInfos.Add(func);
        return func;
    }

    public List<LocalInfo> ToInfos(Mli[] arr, Dictionary<string, StringType> localsStructures)
    {
        var locals = new List<LocalInfo>();

        foreach (var t in arr)
            locals.AddRange(ToLocals(t, localsStructures));

        return locals;
    }

    private List<LocalInfo> ToLocals(Mli t, Dictionary<string, StringType> localsStructures, string separator = ".")
    {
        if (!_structures.TryGetValue(t.Type, out var value))
            return [new LocalInfo(Enum.Parse<AsmType>(t.Type.Type), t.Name, t.IsByRef)];

        localsStructures.Add(t.Name, t.Type);
        return value.Select(x => new LocalInfo(x.Value, $"{t.Name}{separator}{x.Key}", t.IsByRef)).ToList();
    }

    public void AddStructure(string typeName, List<(string type, string name)> structure)
    {
        var lis = new List<LocalInfo>();

        foreach (var pair in structure)
            lis.AddRange(ToLocals(new Mli(new StringType(pair.type), pair.name), new Dictionary<string, StringType>()));

        _structures.Add(new StringType(typeName), lis.ToDictionary(x => x.Name, x => x.Type));
    }
}