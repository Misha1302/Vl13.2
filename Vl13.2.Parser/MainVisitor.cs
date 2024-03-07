namespace Vl13._2.Parser;

using System.Reflection;
using Antlr4.Runtime.Tree;
using Vl13._2.Parser.Content;
using static None;

// ReSharper disable ConvertToConstant.Local
public class MainVisitor : GrammarBaseVisitor<None>
{
    private static readonly HashSet<Type> _allowedTypes = [typeof(long), typeof(int), typeof(double), typeof(bool)];
    public readonly VlModuleBuilder Module = new();
    private readonly string _noneType = "NONE";

    private AsmFunctionBuilder _curFunc = null!;
    private PreVisitor _preVisitor = null!;

    private int _exprLevel;

    private static string ReturnAddress(int index) => $"returnValue<{index}>";

    public override None Visit(IParseTree tree)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_preVisitor is not null)
            return base.Visit(tree);

        _preVisitor = new PreVisitor();
        _preVisitor.Visit(tree);

        return base.Visit(tree);
    }


    public override None VisitFunctionDecl(GrammarParser.FunctionDeclContext context)
    {
        var args = context.varDecl().Select(x =>
            new ModuleLocalInfo(
                x.type().IDENTIFIER().GetText(),
                x.IDENTIFIER().GetText(),
                x.type().ampersand() != null
            )
        ).ToArray();

        var retType = context.type().GetText().ToUpper();
        ModuleLocalInfo[]? returnValues = null;
        if (retType != _noneType)
        {
            var countArgs = !Module.Structures.ContainsKey(retType) ? 1 : Module.Structures[retType].Count;
            returnValues = Enumerable.Range(0, countArgs)
                .Select(x => new ModuleLocalInfo("I64", ReturnAddress(x), true)).ToArray();
            args = returnValues.Union(args).ToArray();
        }

        _curFunc = Module.AddFunction(context.IDENTIFIER().GetText(), args, [], returnValues?.Length ?? 0);
        Visit(context.block());
        _curFunc.Ret();
        _curFunc = null!;

        return Nothing;
    }

    public override None VisitInclude(GrammarParser.IncludeContext context)
    {
        var path = context.STRING().GetText()[1..^1];

        Predicate<MethodInfo> canIncludeMethod = mi =>
            mi.IsStatic && !mi.IsAbstract && !mi.ContainsGenericParameters &&
            !mi.DeclaringType!.ContainsGenericParameters &&
            !mi.IsConstructedGenericMethod &&
            mi.GetParameters().All(x => _allowedTypes.Contains(x.ParameterType)) &&
            (mi.ReturnType == typeof(void) || _allowedTypes.Contains(mi.ReturnType));

        Func<Type, MethodInfo, string> makeAlias = (type, info) =>
        {
            var types = TypesToString(info.GetParameters());
            var s = $"{type.Namespace}.{type.Name}.{info.Name}";
            return types.Length != 0 ? $"{s}.{types}" : s;
        };

        if (path == "main")
        {
            ReflectionManager.AddAllMethods(Assembly.GetExecutingAssembly(), makeAlias, canIncludeMethod);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                ReflectionManager.AddAllMethods(assembly, makeAlias, canIncludeMethod);
            ReflectionManager.AddAllMethods(Assembly.Load("System.Console"), makeAlias, canIncludeMethod);
        }
        else
        {
            ReflectionManager.AddAllMethods(Assembly.Load(path), makeAlias, canIncludeMethod);
        }

        return Nothing;
    }

    public override None VisitVarDecl(GrammarParser.VarDeclContext context)
    {
        _curFunc.AddLocal(
            new ModuleLocalInfo(
                context.type().IDENTIFIER().GetText(),
                context.IDENTIFIER().GetText(),
                context.type().ampersand() != null
            )
        );

        return Nothing;
    }

    public override None VisitLabel(GrammarParser.LabelContext context)
    {
        _curFunc.SetLabel(context.IDENTIFIER().GetText());
        return Nothing;
    }

    public override None VisitJmp(GrammarParser.JmpContext context)
    {
        _curFunc.Br(context.IDENTIFIER().GetText());
        return Nothing;
    }

    public override None VisitVarSet(GrammarParser.VarSetContext context)
    {
        string locName;

        if (context.varDecl() != null)
        {
            Visit(context.varDecl());
            locName = context.varDecl().IDENTIFIER().GetText();
        }
        else
        {
            locName = context.IDENTIFIER().GetText();
        }

        _exprLevel++;
        Visit(context.expression());
        _exprLevel--;

        if (Module.HasGlobal(locName)) _curFunc.StoreDataToLabel(locName);
        else if (_curFunc.HasLocal(locName)) _curFunc.SetLocal(locName);
        else Thrower.Throw(new InvalidOperationException());

        return Nothing;
    }

    public override None VisitSumSubExpr(GrammarParser.SumSubExprContext context)
    {
        _exprLevel++;
        foreach (var expr in context.expression())
            Visit(expr);
        _exprLevel--;

        if (context.PLUS() != null) _curFunc.Add();
        else if (context.MINUS() != null) _curFunc.Sub();

        return Nothing;
    }

    public override None VisitMulDivModExpr(GrammarParser.MulDivModExprContext context)
    {
        _exprLevel++;
        foreach (var expr in context.expression())
            Visit(expr);
        _exprLevel--;

        if (context.STAR() != null) _curFunc.Mul();
        else if (context.DIV() != null) _curFunc.Div();
        else if (context.MOD() != null) _curFunc.Mod();

        return Nothing;
    }

    public override None VisitFloatExpr(GrammarParser.FloatExprContext context)
    {
        _curFunc.PushF(TextToType.ToDouble(context.GetText()));

        return Nothing;
    }

    public override None VisitGetAddressExpr(GrammarParser.GetAddressExprContext context)
    {
        var name = context.IDENTIFIER().GetText();
        if (_curFunc.HasLocal(name))
            _curFunc.LocAddress(name);
        else _curFunc.LabelAddress(name);

        return Nothing;
    }

    public override None VisitIdentifierExpr(GrammarParser.IdentifierExprContext context)
    {
        var locName = context.IDENTIFIER().GetText();

        if (Module.HasGlobal(locName)) _curFunc.LoadDataFromLabel(locName);
        else if (_curFunc.HasLocal(locName)) _curFunc.GetLocal(locName);
        else Thrower.Throw(new InvalidOperationException());

        return Nothing;
    }

    public override None VisitGlobalDecl(GrammarParser.GlobalDeclContext context)
    {
        Module.AddGlobals([
            new ModuleLocalInfo(context.varDecl().type().GetText(), context.varDecl().IDENTIFIER().GetText())
        ]);
        return Nothing;
    }

    public override None VisitIntExpr(GrammarParser.IntExprContext context)
    {
        _curFunc.PushI(TextToType.ToInt(context.INT().GetText()));
        return Nothing;
    }

    public override None VisitRet(GrammarParser.RetContext context)
    {
        if (context.expression() != null)
        {
            _exprLevel++;
#warning
            Visit(context.expression());
            for (var i = 0; i < _curFunc.ReturnsCount; i++)
                _curFunc.SetLocal(ReturnAddress(i));
            _exprLevel--;
        }

        _curFunc.Ret();
        return Nothing;
    }

    public override None VisitWhile(GrammarParser.WhileContext context)
    {
        _curFunc.While(
            () => Visit(context.expression()),
            () => Visit(context.block())
        );

        return Nothing;
    }

    public override None VisitFor(GrammarParser.ForContext context)
    {
        _curFunc.For(
            () => Visit(context.line(0)),
            () => Visit(context.line(1)),
            () => Visit(context.line(2)),
            () => Visit(context.block())
        );

        return Nothing;
    }

    public override None VisitCallExpr(GrammarParser.CallExprContext context)
    {
        var fName = context.expression(0).GetText();

        _exprLevel++;
        foreach (var e in context.expression().Skip(1).Reverse())
            Visit(e);
        _exprLevel--;

        Action a =
            ReflectionManager.Methods.TryGetValue(fName, out var tuple)
                ? () => CallSharp(tuple)
                : fName switch
                {
                    "f64Toi64" => _curFunc.F64ToI64,
                    "i64Toi32" => _curFunc.I64ToI32,
                    "i64Toi16" => _curFunc.I64ToI16,
                    "i64Toi8" => _curFunc.I64ToI8,
                    "i8Toi64" => _curFunc.I8ToI64,
                    "i16Toi64" => _curFunc.I16ToI64,
                    _ => CallFunc
                };

        a();


        return Nothing;


        void CallSharp((MethodInfo mi, nint ptr) value)
        {
            _curFunc.CallSharp(value);
            if (_exprLevel == 0)
                _curFunc.Drop();
        }

        void CallFunc()
        {
            var returnType = _preVisitor.Functions[fName].returnType;
            var isNotNone = returnType != _noneType;
            var returnValueLocalName = Guid.NewGuid().ToString();

            if (isNotNone)
            {
                _curFunc.AddLocal(new ModuleLocalInfo(returnType, returnValueLocalName));
                _curFunc.LocAddress(returnValueLocalName);
            }

            _curFunc.CallFunc(fName);

            if (_exprLevel != 0 && isNotNone)
                _curFunc.GetLocal(returnValueLocalName);
        }
    }

    public override None VisitStruct(GrammarParser.StructContext context)
    {
        Module.AddStructure(context.IDENTIFIER().GetText(),
            context.varDecl().Select(x => (x.type().GetText(), x.IDENTIFIER().GetText())).ToList()
        );

        return Nothing;
    }

    public override None VisitAddressCallExpr(GrammarParser.AddressCallExprContext context)
    {
        var returnType = context.type()[^1].GetText();
        var isNotNone = returnType != _noneType;
        var returnValueLocalName = Guid.NewGuid().ToString();

        _exprLevel++;
        foreach (var e in context.expression().Skip(1).Reverse())
            Visit(e);
        _exprLevel--;

        if (isNotNone)
        {
            _curFunc.AddLocal(new ModuleLocalInfo(returnType, returnValueLocalName));
            _curFunc.LocAddress(returnValueLocalName);
        }

        var typeContexts = context.type();

        var types = typeContexts.Select(
            (t, index) => isNotNone && index == 0
                ? "I64"
                : t.GetText()
        ).ToArray();

        Visit(context.expression(0));
        _curFunc.CallAddress(types);

        if (_exprLevel != 0 && isNotNone)
            _curFunc.GetLocal(returnValueLocalName);

        return Nothing;
    }

    public override None VisitIf(GrammarParser.IfContext context)
    {
        _curFunc.Condition(() =>
            {
                _exprLevel++;
                Visit(context.expression());
                _exprLevel--;
            },
            () => Visit(context.block()),
            () =>
            {
                if (context.@else() != null)
                    Visit(context.@else());
            }
        );

        return Nothing;
    }

    public override None VisitElse(GrammarParser.ElseContext context)
    {
        if (context.@if() != null)
            Visit(context.@if());
        else Visit(context.block());

        return Nothing;
    }

    public override None VisitCmpExpr(GrammarParser.CmpExprContext context)
    {
        _exprLevel++;
        foreach (var e in context.expression())
            Visit(e);
        _exprLevel--;

        if (context.LT() != null) _curFunc.Lt();
        else if (context.LE() != null) _curFunc.Le();
        else if (context.GT() != null) _curFunc.Gt();
        else if (context.GE() != null) _curFunc.Ge();

        return Nothing;
    }

    public override None VisitEqExpr(GrammarParser.EqExprContext context)
    {
        _exprLevel++;
        foreach (var e in context.expression())
            Visit(e);
        _exprLevel--;

        if (context.EQ() != null) _curFunc.Eq();
        else if (context.NEQ() != null) _curFunc.Neq();

        return Nothing;
    }

    private static string TypesToString(ParameterInfo[] parameters)
    {
        return string.Join("", parameters.Select(x =>
            x.ParameterType == typeof(long) ? "i64" :
            x.ParameterType == typeof(int) ? "i32" :
            x.ParameterType == typeof(double) ? "f64" :
            x.ParameterType == typeof(bool) ? "i8" :
            Thrower.Throw<string>(new InvalidOperationException("Unknown type"))
        ));
    }
}