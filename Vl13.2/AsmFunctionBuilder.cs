namespace Vl13._2;

public class AsmFunctionBuilder : VlImageInfo
{
    private Dictionary<string, LocalInfo> _localsList = new();

    public void Write(Type valueType) => CallSharp(typeof(Console), nameof(Console.Write), [valueType]);

    public void WriteLine(Type? valueType) =>
        CallSharp(typeof(Console), nameof(Console.WriteLine), valueType == null ? [] : [valueType]);

    public void DeclareLocals(params LocalInfo[] localInfos) =>
        _localsList = localInfos.ToDictionary(x => x.Name, x => x);

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

    public void SetLocal(string locName, Action value)
    {
        value();
        LocAddress(locName, _localsList[locName].Type);
        Store64();
    }

    public void GetLocal(string locName)
    {
        LocAddress(locName, _localsList[locName].Type);
        Load64();
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
}