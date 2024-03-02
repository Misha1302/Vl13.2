namespace Vl13._2.Parser;

using System.Reflection;
using Vl13._2.Parser.Content;
using static None;

public class MainVisitor : GrammarBaseVisitor<None>
{
    private static readonly HashSet<Type> _allowedTypes = [typeof(long), typeof(int), typeof(double), typeof(bool)];
    public readonly VlModuleBuilder Module = new();
    private AsmFunctionBuilder _curFunc = null!;


    public override None VisitFunctionDecl(GrammarParser.FunctionDeclContext context)
    {
        var args = context.varDecl().Select(x =>
            new ModuleLocalInfo(
                x.type().IDENTIFIER().GetText(),
                x.IDENTIFIER().GetText(),
                x.type().ampersand() != null
            )
        ).ToArray();

        _curFunc = Module.AddFunction(context.IDENTIFIER().GetText(), args, []);
        Visit(context.block());
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

    public override None VisitIntExpr(GrammarParser.IntExprContext context)
    {
        _curFunc.PushI(TextToType.ToInt(context.INT().GetText()));
        return Nothing;
    }

    public override None VisitRet(GrammarParser.RetContext context)
    {
        _curFunc.Ret();
        return Nothing;
    }

    public override None VisitCallExpr(GrammarParser.CallExprContext context)
    {
        var fName = context.expression(0).GetText();

        foreach (var e in context.expression().Skip(1))
            Visit(e);

        if (ReflectionManager.Methods.TryGetValue(fName, out var tuple))
            _curFunc.CallSharp(tuple);
        else _curFunc.CallFunc(fName);

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