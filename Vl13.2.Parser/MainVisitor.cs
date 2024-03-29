﻿namespace Vl13._2.Parser;

using System.Reflection;
using Antlr4.Runtime.Tree;
using Vl13._2.Parser.Content;
using static None;

// ReSharper disable ConvertToConstant.Local
public class MainVisitor : GrammarBaseVisitor<None>
{
    public readonly VlModuleBuilder Module = new();
    private readonly VlType _noneType = new("NONE");

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
                new VlType(x.type().IDENTIFIER().GetText()),
                x.IDENTIFIER().GetText(),
                x.type().ampersand() != null
            )
        ).ToList();

        var retType = new VlType(context.type().GetText());
        var returnValues = new List<ModuleLocalInfo>();
        if (retType != _noneType)
        {
            var countArgs = !Module.Structures.ContainsKey(retType) ? 1 : Module.Structures[retType].Count;
            returnValues = Enumerable.Range(0, countArgs)
                .Select(x => new ModuleLocalInfo(new VlType("I64"), ReturnAddress(x), true)).ToList();
        }

        _curFunc = Module.AddFunction(context.IDENTIFIER().GetText(), returnValues, args, []);
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
            mi.GetParameters().All(x => TextToType.AllowedTypes.Contains(x.ParameterType)) &&
            (mi.ReturnType == typeof(void) || TextToType.AllowedTypes.Contains(mi.ReturnType));

        Func<Type, MethodInfo, string> makeAlias = (type, info) =>
        {
            var types = string.Concat(info.GetParameters().Select(x => TextToType.TypeAsStrings[x.ParameterType]));

            var s = "";
            if (!string.IsNullOrEmpty(type.Namespace)) s += type.Namespace + ".";
            s += $"{type.Name}.{info.Name}";
            if (types.Length != 0) s += $".{types}";

            return s;
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
            ReflectionManager.AddAllMethods(
                File.Exists(path) ? Assembly.LoadFile(Path.GetFullPath(path)) : Assembly.Load(path),
                makeAlias,
                canIncludeMethod
            );
        }

        return Nothing;
    }

    public override None VisitMemSetExpr(GrammarParser.MemSetExprContext context)
    {
        VisitExpr(context.expression(1));
        VisitExpr(context.expression(0));
        _curFunc.Store64();

        return Nothing;
    }

    public override None VisitMemReadExpr(GrammarParser.MemReadExprContext context)
    {
        VisitExpr(context.expression());
        _curFunc.Load64();

        return Nothing;
    }

    public override None VisitVarDecl(GrammarParser.VarDeclContext context)
    {
        _curFunc.AddLocal(
            new ModuleLocalInfo(
                new VlType(context.type().IDENTIFIER().GetText()),
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

        VisitExpr(context.expression());

        if (Module.HasGlobal(locName)) _curFunc.StoreDataToLabel(locName);
        else if (_curFunc.HasLocal(locName)) _curFunc.SetLocal(locName);
        else Thrower.Throw(new InvalidOperationException());

        return Nothing;
    }

    public override None VisitSumSubExpr(GrammarParser.SumSubExprContext context)
    {
        VisitExprs(context.expression());

        if (context.PLUS() != null) _curFunc.Add();
        else if (context.MINUS() != null) _curFunc.Sub();

        return Nothing;
    }

    public override None VisitMulDivModExpr(GrammarParser.MulDivModExprContext context)
    {
        VisitExprs(context.expression());

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
        else Thrower.Throw(new InvalidOperationException($"Loc {locName} was not found"));

        return Nothing;
    }

    public override None VisitGlobalDecl(GrammarParser.GlobalDeclContext context)
    {
        Module.AddGlobals(
            new ModuleLocalInfo(new VlType(context.varDecl().type().GetText()),
                context.varDecl().IDENTIFIER().GetText())
        );
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
            VisitExpr(context.expression());

            for (var i = 0; i < _curFunc.ReturnsCount; i++)
                _curFunc.SetLocal(ReturnAddress(i));
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

        VisitExprs(context.expression(), 0);

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
        Module.AddStructure(
            context.IDENTIFIER().GetText(),
            context.varDecl().Select(x => (x.type().GetText(), x.IDENTIFIER().GetText())).ToList()
        );

        return Nothing;
    }

    public override None VisitAddressCallExpr(GrammarParser.AddressCallExprContext context)
    {
        var returnType = new VlType(context.type()[^1].GetText());
        var isNotNone = returnType != _noneType;
        var returnValueLocalName = Guid.NewGuid().ToString();

        VisitExprs(context.expression(), 0);

        if (isNotNone)
        {
            _curFunc.AddLocal(new ModuleLocalInfo(returnType, returnValueLocalName));
            _curFunc.LocAddress(returnValueLocalName);
        }

        var types = context.type().Select(t => new VlType(t.GetText())).ToList();

        Visit(context.expression(0));
        _curFunc.CallAddress(types[^1] == _noneType ? types[..^1] : types);

        if (_exprLevel != 0 && isNotNone)
            _curFunc.GetLocal(returnValueLocalName);

        return Nothing;
    }

    public override None VisitIf(GrammarParser.IfContext context)
    {
        _curFunc.Condition(
            () => VisitExpr(context.expression()),
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
        VisitExprs(context.expression());

        if (context.LT() != null) _curFunc.Lt();
        else if (context.LE() != null) _curFunc.Le();
        else if (context.GT() != null) _curFunc.Gt();
        else if (context.GE() != null) _curFunc.Ge();
        else Thrower.Throw(new InvalidOperationException($"Unknown sign {context.GetText()}"));

        return Nothing;
    }

    public override None VisitEqExpr(GrammarParser.EqExprContext context)
    {
        VisitExprs(context.expression());

        if (context.EQ() != null) _curFunc.Eq();
        else if (context.NEQ() != null) _curFunc.Neq();

        return Nothing;
    }

    private void VisitExprs<T>(IEnumerable<T> blocks, params Index[] elementsToIgnore) where T : IParseTree
    {
        foreach (var b in blocks.Where((_, i) => !elementsToIgnore.Contains(i)))
            VisitExpr(b);
    }

    private void VisitExpr<T>(T block) where T : IParseTree
    {
        _exprLevel++;
        Visit(block);
        _exprLevel--;
    }
}