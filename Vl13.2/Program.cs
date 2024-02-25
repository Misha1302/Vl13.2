﻿using System.Diagnostics;
using Iced.Intel;
using Vl13._2;

/*

The registers RAX, RCX, RDX, R8, R9, R10, R11 are considered volatile (caller-saved).

The registers RBX, RBP, RDI, RSI, RSP, R12, R13, R14, and R15 are considered nonvolatile (callee-saved).

*/

// TODO: add debug data
// TODO: add functions
// TODO: implement all/most instructions
// TODO: add optimizations (associate registers with their in-memory values (and check to see if those memory locations have been modified))

unsafe
{
    var main = new AsmFunctionBuilder();

    main.DeclareLocals(new LocalInfo(AsmType.F64, "i"));

    main.For(
        () => main.SetLocal("i", () => main.PushF(1)),
        () => main.LessThan(() => main.GetLocal("i"), () => main.PushF(100)),
        () => main.SetLocal("i", () => main.Mul(() => main.GetLocal("i"), () => main.PushF(1.0111))),
        () =>
        {
            main.GetLocal("i");
            main.WriteLine(typeof(double));
            main.Drop();
        }
    );

    main.GetLocal("i");
    main.Ret();


    var vlModule = new VlModule { ImageFactories = [main] };
    var translator = new VlTranslator(vlModule);

    Console.WriteLine();

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