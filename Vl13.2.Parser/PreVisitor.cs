namespace Vl13._2.Parser;

using Vl13._2.Parser.Content;

public class PreVisitor : GrammarBaseVisitor<None>
{
    public readonly Dictionary<string, (string returnType, string[] argTypes)> Functions = new();

    public override None VisitFunctionDecl(GrammarParser.FunctionDeclContext context)
    {
        var args = context.varDecl().Select(x => x.type().GetText());

        Functions.Add(context.IDENTIFIER().GetText(), (context.type().GetText().ToUpper(), args.ToArray()));
        return base.VisitFunctionDecl(context);
    }
}