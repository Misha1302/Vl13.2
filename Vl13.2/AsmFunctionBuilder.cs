namespace Vl13._2;

public record AsmFunctionBuilder(string Name, VlModuleBuilder Module, AsmType[] ArgTypes) : VlImageInfo(Name, ArgTypes)
{
    public Dictionary<string, LocalInfo> LocalsList = new();
    private Dictionary<string, string> _localsStructures = new();

    public void Write(Type valueType) => CallSharp(typeof(Console), nameof(Console.Write), [valueType]);

    public void WriteLine(Type? valueType) =>
        CallSharp(typeof(Console), nameof(Console.WriteLine), valueType == null ? [] : [valueType]);

    public void DeclareLocals(IEnumerable<LocalInfo> localInfos, Dictionary<string, string> localsStructures)
    {
        LocalsList = localInfos.ToDictionary(x => x.Name, x => x);
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

    public void AddLocal(Mli li)
    {
        var lis = Module.ToInfos([li], _localsStructures);
        foreach (var l in lis)
            LocalsList.Add(l.Name, l);
    }

    public void SetLocal(string locName, Action? value = null, bool canSetByRef = true)
    {
        value?.Invoke();

        OpLoc(
            locName,
            loc =>
            {
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
            },
            fieldName => SetLocal($"{locName}_{fieldName}"),
            true
        );
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
            name => GetLocal($"{locName}_{name}"),
            false
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
                        if (LocalsList[locName].Type == AsmType.I64)
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
        OpLoc(
            locName,
            info => LocAddress(locName, info.Type),
            fldName => FieldAddress(locName, fldName),
            false
        );
    }

    private void OpLoc(
        string locName,
        Action<LocalInfo> loc,
        Action<string> structure,
        bool reverse
    )
    {
        if (!_localsStructures.TryGetValue(locName, out var type))
            loc(LocalsList[locName]);
        else if (reverse)
            foreach (var pair in Module.Structures[type].Reverse())
                structure(pair.Key);
        else
            foreach (var pair in Module.Structures[type])
                structure(pair.Key);
    }

    private void OpGlobal(
        string locName,
        Action<LocalInfo> global,
        Action<string> structure,
        bool reverse
    )
    {
        if (!Module.GlobalsOfStructureTypes.TryGetValue(locName, out var type))
            global(Module.Globals[locName]);
        else if (reverse)
            foreach (var pair in Module.Structures[type].Reverse())
                structure(pair.Key);
        else
            foreach (var pair in Module.Structures[type])
                structure(pair.Key);
    }

    public void Condition(Action cond, Action ifBlock, Action elseBlock)
    {
        var endLbl = GenerateLabelName("endCond");
        var elseLbl = GenerateLabelName("elseCond");

        cond();
        BrZero(elseLbl);

        ifBlock();
        Br(endLbl);

        SetLabel(elseLbl);
        elseBlock();
        SetLabel(endLbl);
    }

    public void LoadDataFromLabel(string name)
    {
        OpGlobal(
            name,
            _ => LoadDataFromLabel(name, Module.Globals[name].Type),
            field => LoadDataFromLabel($"{name}_{field}"),
            false
        );
    }

    public new void StoreDataToLabel(string name)
    {
        OpGlobal(
            name,
            _ => base.StoreDataToLabel(name),
            field => base.StoreDataToLabel($"{name}_{field}"),
            true
        );
    }

    public void CallAddress(string[] args)
    {
        base.CallAddress(ToTypes(args));
    }

    private AsmType[] ToTypes(string[] args)
    {
        var list = new List<AsmType>();

        foreach (var arg in args.Select(x => x.ToUpper()))
            if (!Module.Structures.TryGetValue(arg, out var value))
                list.Add(Enum.Parse<AsmType>(arg));
            else list.AddRange(value.Select(x => x.Value));

        return list.ToArray();
    }

    public void DropCatch()
    {
        CallSharp(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.DropAddress));
        Drop();
    }

    public void ThrowEx()
    {
        var address = Guid.NewGuid().ToString();
        var rsp = Guid.NewGuid().ToString();
        var rbp = Guid.NewGuid().ToString();

        AddLocal(new Mli("I64", address));
        AddLocal(new Mli("I64", rsp));
        AddLocal(new Mli("I64", rbp));

        LocAddress(address);
        LocAddress(rsp);
        LocAddress(rbp);

        CallSharp(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.PopAddress));

        GetLocal(address);
        GetLocal(rsp);
        GetLocal(rbp);

        JumpToAddress();
    }


    public void TryCatch(Action tryAct, Action catchAct)
    {
        var catchName = "catchFunc_" + Guid.NewGuid();
        var tryCatchEndName = "finallyFunc_" + Guid.NewGuid();

        LabelAddress(catchName);
        PushRsp();
        PushRbp();
        CallSharp(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.PushAddress), [typeof(long)]);
        Drop();
        tryAct();
        DropCatch();
        Br(tryCatchEndName);

        SetLabel(catchName);
        catchAct();

        SetLabel(tryCatchEndName);
    }
}