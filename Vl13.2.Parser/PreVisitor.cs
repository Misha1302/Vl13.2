namespace Vl13._2.Parser;

using Vl13._2.Parser.Content;

public class PreVisitor : GrammarBaseVisitor<None>
{
    public readonly Dictionary<string, (StringType returnType, StringType[] argTypes)> Functions = new();

    public override None VisitFunctionDecl(GrammarParser.FunctionDeclContext context)
    {
        var args = context.varDecl().Select(x => new StringType(x.type().GetText())).ToArray();

        Functions.Add(context.IDENTIFIER().GetText(), (new StringType(context.type().GetText()), args));
        return base.VisitFunctionDecl(context);
    }
}