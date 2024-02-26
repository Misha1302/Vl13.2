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
    var module = new VlModuleBuilder();

    module.AddStructure("T_XYZ", new Dictionary<string, AsmType>
    {
        { "x", AsmType.I64 },
        { "y", AsmType.I64 },
        { "z", AsmType.I64 }
    });

    var main = module.AddFunction("main", module, [], AsmType.I64,
        [
            new Mli("I64", "i"),
            new Mli("T_XYZ", "xyz")
        ]
    );

    main.SetField("xyz", "x", () => main.PushI(0));
    main.SetField("xyz", "y", () => main.PushI(1));
    main.SetField("xyz", "z", () => main.PushI(2));

    main.For(
        () => main.SetLocal("i", () => main.PushI(1)),
        () => main.LessThan(() => main.GetLocal("i"), () => main.PushI(100)),
        () => main.IncLoc("i"),
        () =>
        {
            main.IncField("xyz", "x");
            main.IncField("xyz", "y");
            main.IncField("xyz", "z");

            main.CallFunc("square", () => main.GetLocal("xyz"), () => main.FieldAddress("xyz", "x"));

            main.GetField("xyz", "z");
            main.GetField("xyz", "y");
            main.GetField("xyz", "x");

            main.Write(typeof(long));
            main.Drop();
            main.PushI(' ');
            main.Write(typeof(char));
            main.Drop();
            main.Write(typeof(long));
            main.Drop();
            main.PushI(' ');
            main.Write(typeof(char));
            main.Drop();
            main.WriteLine(typeof(long));
            main.Drop();
        }
    );
    main.Ret(() => main.GetLocal("i"));


    var square = module.AddFunction("square", module, [new Mli("T_XYZ", "xyz"), new Mli("I64", "pointer")], AsmType.F64,
        []);
    square.Mul(() => square.GetField("xyz", "x"), () => square.Dup());
    square.Mul(() => square.GetField("xyz", "y"), () => square.Dup());
    square.Mul(() => square.GetField("xyz", "z"), () => square.Dup());

    square.PushI(55);
    square.GetLocal("pointer");
    square.Store64();

    square.Ret();


    var vlTranslator = new VlTranslator(module.ImageInfos);
    return vlTranslator;
}