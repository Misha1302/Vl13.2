namespace Vl13._2.Parser;

using System.Reflection;
using Antlr4.Runtime.Tree;
using Vl13._2.Parser.Content;
using static None;

public class MainVisitor : GrammarBaseVisitor<None>
{
    private static readonly HashSet<Type> _allowedTypes = [typeof(long), typeof(int), typeof(double), typeof(bool)];
    public readonly VlModuleBuilder Module = new();
    private AsmFunctionBuilder _curFunc = null!;
    private PreVisitor _preVisitor = null!; // ReSharper disable ConvertToConstant.Local
    private readonly string _returnAddress = "returnValue<>";
    private readonly string _noneType = "none";

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

        if (context.type().GetText() != _noneType)
            args = new[] { new ModuleLocalInfo("I64", _returnAddress, true) }.Union(args).ToArray();

        _curFunc = Module.AddFunction(context.IDENTIFIER().GetText(), args, []);
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

        _curFunc.SetLocal(locName, () => Visit(context.expression()));
        return Nothing;
    }

    public override None VisitSumSubExpr(GrammarParser.SumSubExprContext context)
    {
        foreach (var expr in context.expression())
            Visit(expr);

        if (context.PLUS() != null) _curFunc.Add();
        else if (context.MINUS() != null) _curFunc.Sub();

        return Nothing;
    }

    public override None VisitMulDivModExpr(GrammarParser.MulDivModExprContext context)
    {
        foreach (var expr in context.expression())
            Visit(expr);

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
        if (_curFunc.LocalsList.ContainsKey(name))
            _curFunc.LocAddress(name);
        else _curFunc.LabelAddress(name);

        return Nothing;
    }

    public override None VisitIdentifierExpr(GrammarParser.IdentifierExprContext context)
    {
        _curFunc.GetLocal(context.IDENTIFIER().GetText());
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
            _curFunc.SetLocal(_returnAddress, () => Visit(context.expression()));

        _curFunc.Ret();
        return Nothing;
    }

    public override None VisitCallExpr(GrammarParser.CallExprContext context)
    {
        var fName = context.expression(0).GetText();

        foreach (var e in context.expression().Skip(1).Reverse())
            Visit(e);

        if (ReflectionManager.Methods.TryGetValue(fName, out var tuple))
        {
            _curFunc.CallSharp(tuple);
        }
        else if (fName == "f64Toi64")
        {
            _curFunc.F64ToI64();
        }
        else if (fName == "i64Toi32")
        {
            _curFunc.I64ToI32();
        }
        else if (fName == "i64Toi16")
        {
            _curFunc.I64ToI16();
        }
        else if (fName == "i64Toi8")
        {
            _curFunc.I64ToI8();
        }
        else if (fName == "i8ToI64")
        {
            _curFunc.I8ToI64();
        }
        else if (fName == "i16Toi64")
        {
            _curFunc.I16ToI64();
        }
        else
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

            if (isNotNone)
                _curFunc.GetLocal(returnValueLocalName);
        }

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