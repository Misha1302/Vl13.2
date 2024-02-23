using Vl13._2;

/*

The registers RAX, RCX, RDX, R8, R9, R10, R11 are considered volatile (caller-saved).

The registers RBX, RBP, RDI, RSI, RSP, R12, R13, R14, and R15 are considered nonvolatile (callee-saved).

*/

// TODO: add debug data
// TODO: add doubles 
// TODO: add functions
// TODO: implement all/most instructions
// TODO: add optimizations (associate registers with their in-memory values (and check to see if those memory locations have been modified))

unsafe
{
    var vlModule = new VlModule();
    var main = new AsmFunctionBuilder();

    main.DeclareLocals(new LocalInfo(AsmType.F64, "i"));
    main.SetLocal("i", () => main.PushF(0.000));


    main.While(
        () => main.LessThan(() => main.GetLocal("i"), () => main.PushF(10.0)),
        () =>
        {
            main.GetLocal("i");
            main.WriteLine(typeof(double));
            main.Drop();
            main.SetLocal("i", () => main.Add(() => main.GetLocal("i"), () => main.PushF(1.111)));
        }
    );

    main.WriteLine(null);

    main.Ret();


    vlModule.ImageFactories.Add(main);
    var translator = new VlTranslator(vlModule);
    var asm = translator.Translate();

    AsmExecutor.PrintCode(asm);
    Console.WriteLine();

    var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    var value = ((delegate*<long>)AsmExecutor.MakeFunction(asm))();
    Console.WriteLine($"Execution time: {DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime}");

    Console.WriteLine($"Res: {value}; {BitConverter.Int64BitsToDouble(value)}");
}