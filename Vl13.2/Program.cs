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
            nativeFunction = AsmExecutor.MakeFunction<long>(asm = translator.Translate(debugData));
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
    var moduleBuilder = new VlModuleBuilder();

    var main = moduleBuilder.AddFunction("main", [], AsmType.I64, [new LocalInfo(AsmType.F64, "i")]);
    main.For(
        () => main.SetLocal("i", () => main.PushF(1)),
        () => main.LessThan(() => main.GetLocal("i"), () => main.PushF(100)),
        () => main.SetLocal("i", () => main.Mul(() => main.GetLocal("i"), () => main.PushF(1.0111))),
        () =>
        {
            main.CallFunc("square", () => main.GetLocal("i"));
            main.WriteLine(typeof(double));
            main.Drop();
        }
    );
    main.Ret(() => main.GetLocal("i"));


    var square = moduleBuilder.AddFunction("square", [new LocalInfo(AsmType.F64, "a")], AsmType.F64, []);
    square.Ret(() => square.Mul(() => square.GetLocal("a"), () => square.GetLocal("a")));


    var vlTranslator = new VlTranslator(moduleBuilder.ImageInfos);
    return vlTranslator;
}