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

every function must return value to avoid the errors

when you call sharp function, dont forgot for shadow space

*/

// TODO: implement exceptions via long jumps - make global var, that will be contains stack for jumps and make functions for save position and jump to catch blocks
// TODO: implement all/most instructions
// TODO: add optimizations (associate registers with their in-memory values (and check to see if those memory locations have been modified))

unsafe
{
    var translator = CreateTranslator();

    delegate*<long> nativeFunction = null;
    long value = 0;
    Assembler asm = null!;

    var debugData = new DebugData();

    var compilationTime = MeasureTime(
        () =>
        {
            nativeFunction = AsmExecutor.MakeFunction<long>(
                asm = translator.Translate(debugData, new TranslateData(2048, true))
            );
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
    var module = new VlModuleBuilder([new Mli("I64", "globalVar", false)]);

    var main = module.AddFunction("main", [], AsmType.I64, []);
    
    main.FuncAddress("hello");
    main.CallAddress([], AsmType.I64);
    main.WriteLine(typeof(long));
    main.Drop();
    
    main.Ret(() => main.PushI(0));

    var hello = module.AddFunction("hello", [], AsmType.I64, []);
    hello.Ret(() => hello.PushI(111));


    var vlTranslator = new VlTranslator(module.Compile());
    return vlTranslator;
}