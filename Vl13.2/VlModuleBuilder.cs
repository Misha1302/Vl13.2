namespace Vl13._2;

public class VlModuleBuilder
{
    private readonly List<VlImageInfo> _imageInfos = [];
    private readonly Dictionary<VlType, Dictionary<string, AsmType>> _structures = new();

    public readonly Dictionary<string, LocalInfo> Globals = new();
    public readonly Dictionary<string, VlType> GlobalsOfStructureTypes = new();

    public VlModuleBuilder()
    {
        CreateBuildinFunctions();
    }

    public IReadOnlyDictionary<VlType, Dictionary<string, AsmType>> Structures => _structures;

    public bool HasGlobal(string name) => Globals.ContainsKey(name) || GlobalsOfStructureTypes.ContainsKey(name);

    public void AddGlobals(Mli global)
    {
        foreach (var loc in ToLocals(global, GlobalsOfStructureTypes))
            Globals.Add(global.Name, loc);
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

    public AsmFunctionBuilder AddFunction(string name, List<Mli> returnValues, List<Mli> args, List<Mli> locals)
    {
        var localsStructures = new Dictionary<string, VlType>();
        var argsInfos = ToInfos(args, localsStructures);
        var returnsInfos = ToInfos(returnValues, localsStructures);
        var localsInfos = ToInfos(locals, localsStructures);

        var func = new AsmFunctionBuilder(
            name,
            this,
            returnsInfos.Union(argsInfos).Select(x => x.Type).ToList(),
            returnsInfos.Count
        );
        func.DeclareLocals(localsInfos.Union(argsInfos).Union(returnsInfos).ToArray(), localsStructures);

        foreach (var t in returnsInfos)
            func.SetLocal(t.Name, null, false);
        for (var index = argsInfos.Count - 1; index >= 0; index--)
            func.SetLocal(argsInfos[index].Name, null, false);

        _imageInfos.Add(func);
        return func;
    }

    public List<LocalInfo> ToInfos(List<Mli> arr, Dictionary<string, VlType> localsStructures)
    {
        var locals = new List<LocalInfo>();

        foreach (var t in arr)
            locals.AddRange(ToLocals(t, localsStructures));

        return locals;
    }

    private List<LocalInfo> ToLocals(Mli t, Dictionary<string, VlType> localsStructures, string separator = ".")
    {
        if (!_structures.TryGetValue(t.Type, out var value))
            return [new LocalInfo(Enum.Parse<AsmType>(t.Type.MainType.Type), t.Name, t.IsByRef)];

        localsStructures.Add(t.Name, t.Type);
        return value.Select(x => new LocalInfo(x.Value, $"{t.Name}{separator}{x.Key}", t.IsByRef)).ToList();
    }

    public void AddStructure(string typeName, List<(string type, string name)> structure)
    {
        var lis = new List<LocalInfo>();

        foreach (var pair in structure)
            lis.AddRange(ToLocals(new Mli(new VlType(pair.type), pair.name), new Dictionary<string, VlType>()));

        _structures.Add(new VlType(typeName), lis.ToDictionary(x => x.Name, x => x.Type));
    }
}