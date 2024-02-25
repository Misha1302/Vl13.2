using System.Diagnostics;
using Iced.Intel;
using Vl13._2;

/*

r15 keeps stack array address
r14 is index of stack array

every program in this language starts with init function, that allocates stack and sets the registers
init function also saves registers
init function calls main function, that user written
after main function executing, init function restore registers and returns to C# method, that calls it

*/


// TODO: add structures
// TODO: implement all/most instructions?
// TODO: implement exceptions via long jumps?
// TODO: add optimizations (associate registers with their in-memory values (and check to see if those memory locations have been modified))

unsafe
{
    var translator = CreateTranslator();

    delegate*<long> nativeFunction = null;
    long value = 0;
    Assembler asm = null!;
    DebugData debugData = null!;

    var compilationTime = MeasureTime(
        () =>
        {
            debugData = new DebugData();
            nativeFunction = (delegate*<long>)AsmExecutor.MakeFunction(asm = translator.Translate(debugData));
        });

    AsmExecutor.PrintCode(asm, debugData);

    var executionTime = MeasureTime(
        () => value = nativeFunction()
    );

    Console.WriteLine($"Compilation time: {compilationTime}");
    Console.WriteLine($"Execution time: {executionTime}");

    Console.WriteLine($"Res: {value}; {BitConverter.Int64BitsToDouble(value)}");
}

return;


long MeasureTime(Action method)
{
    var sw = Stopwatch.StartNew();
    method();
    return sw.ElapsedMilliseconds;
}

VlTranslator CreateTranslator()
{
    var init = new AsmFunctionBuilder("init", []);
    init.Init();

    init.CallFunc("main", [], AsmType.F64);

    init.End();


    var main = new AsmFunctionBuilder("main", []);

    main.DeclareLocals(new LocalInfo(AsmType.F64, "i"));

    main.For(
        () => main.SetLocal("i", () => main.PushF(1)),
        () => main.LessThan(() => main.GetLocal("i"), () => main.PushF(100)),
        () => main.SetLocal("i", () => main.Mul(() => main.GetLocal("i"), () => main.PushF(1.0111))),
        () =>
        {
            main.CallFunc("square", [() => main.GetLocal("i")], AsmType.F64);
            main.WriteLine(typeof(double));
            main.Drop();
        }
    );

    main.Ret(() => main.GetLocal("i"));


    var square = new AsmFunctionBuilder("square", [AsmType.F64]);
    square.DeclareLocals(new LocalInfo(AsmType.F64, "a"));
    square.SetLocal("a", null); // set arg
    square.Ret(() => square.Mul(() => square.GetLocal("a"), () => square.GetLocal("a")));
    square.Ret(() => square.GetLocal("a"));


    // не умеет смотреть ветвления?

    var vlModule = new VlModule { ImageFactories = [init, main, square] };
    var vlTranslator = new VlTranslator(vlModule);
    return vlTranslator;
}