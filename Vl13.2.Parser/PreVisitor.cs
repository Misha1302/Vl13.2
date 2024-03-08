namespace Vl13._2.Parser;

using Vl13._2.Parser.Content;

public class PreVisitor : GrammarBaseVisitor<None>
{
    public readonly Dictionary<string, (VlType returnType, VlType[] argTypes)> Functions = new();

    public override None VisitFunctionDecl(GrammarParser.FunctionDeclContext context)
    {
        var args = context.varDecl().Select(x => new VlType(x.type().GetText())).ToArray();

        Functions.Add(context.IDENTIFIER().GetText(), (new VlType(context.type().GetText()), args));
        return base.VisitFunctionDecl(context);
    }
}