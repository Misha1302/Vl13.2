namespace Vl13._2;

public class VlModuleBuilder
{
    private readonly List<VlImageInfo> _imageInfos = [];
    private readonly Dictionary<string, Dictionary<string, AsmType>> _structures = new();

    public readonly Dictionary<string, LocalInfo> Globals;
    public readonly Dictionary<string, string> GlobalsOfStructureTypes = new();

    public VlModuleBuilder(params Mli[] globals)
    {
        Globals = ToDict(globals);
        CreateBuildinFunctions();
    }

    public IReadOnlyDictionary<string, Dictionary<string, AsmType>> Structures => _structures;

    private Dictionary<string, LocalInfo> ToDict(Mli[] globals)
    {
        var dictionary = new Dictionary<string, LocalInfo>();

        foreach (var info in globals)
            foreach (var loc in ToLocals(info, GlobalsOfStructureTypes))
                dictionary.Add(info.Name, loc);
        return dictionary;
    }

    public List<VlImageInfo> Compile()
    {
        var index = _imageInfos.FindIndex(x => x.Name == "dataFunc");
        if (index != -1) _imageInfos.RemoveAt(index);

        var dataFunc = AddFunction("dataFunc", [], []);
        foreach (var global in Globals)
            dataFunc.CreateDataLabel(global.Key);

        return _imageInfos;
    }

    private void CreateBuildinFunctions() => CreateInitFunction();

    private void CreateInitFunction()
    {
        var init = AddFunction("init", [], []);
        init.Init();
        init.CallFunc("main");
        init.End();
    }

    public AsmFunctionBuilder AddFunction(string name, Mli[] args, Mli[] locals)
    {
        var localsStructures = new Dictionary<string, string>();
        var argsInfos = ToInfos(args, localsStructures);
        var localsInfos = ToInfos(locals, localsStructures);

        var func = new AsmFunctionBuilder(name, this, argsInfos.Select(x => x.Type).ToArray());
        func.DeclareLocals(localsInfos.Union(argsInfos).ToArray(), localsStructures);

        foreach (var t in argsInfos)
            func.SetLocal(t.Name, null, false);

        _imageInfos.Add(func);
        return func;
    }

    public List<LocalInfo> ToInfos(Mli[] arr, Dictionary<string, string> localsStructures)
    {
        var locals = new List<LocalInfo>();

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