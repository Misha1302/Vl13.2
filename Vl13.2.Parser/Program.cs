// See https://aka.ms/new-console-template for more information

using Antlr4.Runtime;
using Vl13._2;
using Vl13._2.Parser;
using Vl13._2.Parser.Content;

unsafe
{
    var inputStream = new AntlrInputStream(Text.SimpleProgram);
    var speakLexer = new GrammarLexer(inputStream);
    var commonTokenStream = new CommonTokenStream(speakLexer);
    var speakParser = new GrammarParser(commonTokenStream);
    var visitor = new MainVisitor();

    visitor.Visit(speakParser.program());


    var module = visitor.Module;
    var translator = new VlTranslator(module.Compile());
    var debugData = new DebugData();
    var asm = translator.Translate(debugData, new TranslateData(2048, true));

    AsmExecutor.PrintCode(asm, debugData);

    var nativeFunction = AsmExecutor.MakeFunction<None>(asm);
    nativeFunction();

    Console.WriteLine("Success");
}