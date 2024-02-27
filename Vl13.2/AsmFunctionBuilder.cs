namespace Vl13._2;

public record AsmFunctionBuilder(string Name, VlModuleBuilder Module, AsmType[] ArgTypes, AsmType ReturnType)
    : VlImageInfo(Name, ArgTypes, ReturnType)
{
    private Dictionary<string, LocalInfo> _localsList = new();
    private Dictionary<string, string> _localsStructures = new();

    public void Write(Type valueType) => CallSharp(typeof(Console), nameof(Console.Write), [valueType]);

    public void WriteLine(Type? valueType) =>
        CallSharp(typeof(Console), nameof(Console.WriteLine), valueType == null ? [] : [valueType]);

    public void DeclareLocals(IEnumerable<LocalInfo> localInfos, Dictionary<string, string> localsStructures)
    {
        _localsList = localInfos.ToDictionary(x => x.Name, x => x);
        _localsStructures = localsStructures;
    }

    public void While(Action condition, Action body)
    {
        var whileStart = GenerateLabelName("while_start");
        var whileEnd = GenerateLabelName("while_end");

        SetLabel(whileStart);
        // condition
        condition();
        BrZero(whileEnd);

        // body
        body();

        Br(whileStart);

        SetLabel(whileEnd);
    }

    public void Ret(Action result)
    {
        result();
        Ret();
    }

    public void SetLocal(string locName, Action? value, bool canSetByRef = true)
    {
        value?.Invoke();

        var loc = _localsList[locName];

        if (loc.IsByRef && canSetByRef)
        {
            GetLocal(locName, false); // load pointer
            Store64();
        }
        else
        {
            LocAddress(locName, loc.Type);
            Store64();
        }
    }

    public void GetLocal(string locName, bool canGetByRef = true)
    {
        OpLoc(locName,
            info =>
            {
                if (info.IsByRef && canGetByRef)
                {
                    GetLocal(locName, false);
                    Load64();
                }
                else
                {
                    LocAddress(locName, info.Type);
                    Load64();
                }
            },
            name => GetLocal($"{locName}_{name}")
        );
    }

    public void LessThan(Action a, Action b) => BinaryOp(a, b, Lt);
    public void Add(Action a, Action b) => BinaryOp(a, b, Add);

    private static void BinaryOp(Action a, Action b, Action op)
    {
        a();
        b();
        op();
    }

    public void IncLoc(string locName)
    {
        SetLocal(
            locName,
            () =>
                Add(
                    () => GetLocal(locName),
                    () =>
                    {
                        if (_localsList[locName].Type == AsmType.I64)
                            PushI(1);
                        else PushF(1.0);
                    }
                )
        );
    }

    public void For(Action init, Action cond, Action endOfBody, Action body)
    {
        init();

        While(
            cond,
            () =>
            {
                body();
                endOfBody();
            }
        );
    }

    public void Mul(Action a, Action b) => BinaryOp(a, b, Mul);

    public void CallFunc(string name, params Action[] args)
    {
        for (var index = args.Length - 1; index >= 0; index--)
            args[index]();

        base.CallFunc(name);
    }

    public void SetField(string structName, string fieldName, Action action)
    {
        SetLocal($"{structName}_{fieldName}", action);
    }

    public void IncField(string structName, string fieldName)
    {
        IncLoc($"{structName}_{fieldName}");
    }

    public void GetField(string structName, string fieldName)
    {
        GetLocal($"{structName}_{fieldName}");
    }

    public void FieldAddress(string structName, string fieldName)
    {
        LocAddress($"{structName}_{fieldName}");
    }

    public void LocAddress(string locName)
    {
        OpLoc(locName, info => LocAddress(locName, info.Type), fldName => FieldAddress(locName, fldName));
    }

    private void OpLoc(
        string locName,
        Action<LocalInfo> loc,
        Action<string> structure
    )
    {
        if (!_localsStructures.TryGetValue(locName, out var type))
            loc(_localsList[locName]);
        else
            foreach (var pair in Module.Structures[type])
                structure(pair.Key);
    }
}