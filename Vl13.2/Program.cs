/*

r15 keeps stack array address
r14 is index of stack array

every program in this language starts with init function, that allocates stack and sets the registers
init function calls main function, that user written
after main function executing, init function restore registers and returns to C# method, that calls it

when you call sharp function, dont forgot for shadow space

*/

// TODO: strings?
// TODO: syntax?
// TODO: implement all/most instructions?
// TODO: add optimizations (associate registers with their in-memory values (and check to see if those memory locations have been modified))?


namespace Vl13._2;

using System.Diagnostics;
using Iced.Intel;

public static class Program
{
    public static unsafe void Main()
    {
        delegate*<none> nativeFunction = null;
        Assembler asm = null!;

        var debugData = new DebugData();

        var compilationTime = MeasureTime(
            () => nativeFunction = AsmExecutor.MakeFunction<none>(
                asm = CreateTranslator().Translate(debugData, new TranslateData(2048, true))
            )
        );

        AsmExecutor.PrintCode(asm, debugData);

        Console.WriteLine(new string('-', Console.WindowWidth));

        var executionTime = MeasureTime(
            () => nativeFunction()
        );

        Console.WriteLine($"Compilation time: {compilationTime}");
        Console.WriteLine($"Execution time: {executionTime}");
    }

    private static long MeasureTime(Action method)
    {
        var sw = Stopwatch.StartNew();
        method();
        return sw.ElapsedMilliseconds;
    }

    private static VlTranslator CreateTranslator()
    {
        var module = new VlModuleBuilder();

        var main = module.AddFunction("main", [], []);

        main.TryCatch(
            () =>
            {
                main.CallFunc("breakFunc");

                main.PushI(11); // must not to be executed
                main.WriteLine(typeof(long));
                main.Drop();
            },
            () =>
            {
                main.PushI(-1000); // must be executed if try block thrown exception
                main.WriteLine(typeof(long));
                main.Drop();
            }
        );

        main.PushI(1001); // must be executed too
        main.WriteLine(typeof(long));
        main.Drop();

        main.Ret();


        var breakFunc = module.AddFunction("breakFunc", [], []);
        breakFunc.CallFunc("breakFunc2");

        var breakFunc2 = module.AddFunction("breakFunc2", [], []);
        breakFunc2.ThrowEx();


        var vlTranslator = new VlTranslator(module.Compile());
        return vlTranslator;
    }
}