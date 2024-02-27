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


// TODO: add conditions
// TODO: implement exceptions via long jumps
// TODO: implement all/most instructions
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
            nativeFunction = AsmExecutor.MakeFunction<long>(asm = translator.Translate(debugData,
                new TranslateData(2048, true) // check stack overflow is very heavy, but very useful for debug
            ));
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
    var module = new VlModuleBuilder();

    module.AddStructure("T_XYZ", new Dictionary<string, AsmType>
    {
        { "x", AsmType.I64 },
        { "y", AsmType.I64 },
        { "z", AsmType.I64 }
    });

    var main = module.AddFunction("main", module, [], AsmType.I64,
        [
            new Mli("I64", "i", false),
            new Mli("T_XYZ", "xyz", false)
        ]
    );

    main.SetField("xyz", "x", () => main.PushI(1));
    main.SetField("xyz", "y", () => main.PushI(2));
    main.SetField("xyz", "z", () => main.PushI(3));

    main.For(
        () => main.SetLocal("i", () => main.PushI(1)),
        () => main.LessThan(() => main.GetLocal("i"), () => main.PushI(100_000_000)),
        () => main.IncLoc("i"),
        () =>
        {
            main.CallFunc("square", () => main.LocAddress("xyz"));
            main.Drop();

            main.GetField("xyz", "z");
            main.GetField("xyz", "y");
            main.GetField("xyz", "x");

            PrintItems(main, 3);
        }
    );
    main.Ret(() => main.GetLocal("i"));


    var square = module.AddFunction("square", module, [new Mli("T_XYZ", "xyz", true)], AsmType.I64, []);
    
    square.IncField("xyz", "x");
    square.IncField("xyz", "y");
    square.IncField("xyz", "z");

    square.PushI(0);
    square.Ret();


    var vlTranslator = new VlTranslator(module.ImageInfos);
    return vlTranslator;
}

void PrintItems(AsmFunctionBuilder fb, int count)
{
    for (var i = 0; i < count; i++)
    {
        fb.Write(typeof(long));
        fb.Drop();

        if (i != count - 1)
        {
            fb.PushI(' ');
            fb.Write(typeof(char));
            fb.Drop();
        }
    }

    fb.WriteLine(null);
    fb.Drop();
}