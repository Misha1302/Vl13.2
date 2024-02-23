using System.Runtime.InteropServices;
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
    var main = new VlImageInfo();

    main.LocAddress("strPtr"); // strPtr = alloc((100 - 35 + 1) * sizeof(char))
    main.PushI64((100 - 35) * sizeof(char));
    main.CallSharp(typeof(VlRuntimeHelper), nameof(VlRuntimeHelper.Alloc), [typeof(int)]);
    main.StoreI64();

    main.LocAddress("i"); // i = 0
    main.PushI64(35);
    main.StoreI64();


    main.SetLabel("label_start"); // while

    // i != ...
    main.LocAddress("i");
    main.LoadI64();
    main.PushI64(100);
    main.Neq();
    main.BrZero("label_end"); // if false

    // body
    main.LocAddress("strPtr"); // strPtr + ((i - 35) * sizeof(char))
    main.LoadI64();

    main.LocAddress("i");
    main.LoadI64();
    main.PushI64(35);
    main.SubI64();

    main.PushI64(sizeof(char));
    main.MulI64();

    main.AddI64();

    // *(address) = value
    main.LocAddress("i");
    main.LoadI16();
    main.StoreI16();


    // i = i + 1
    main.LocAddress("i"); // first arg for store
    main.LocAddress("i"); // arg for load
    main.LoadI64(); // first arg for add
    main.PushI64(1); // second arg for add
    main.AddI64(); // add
    main.StoreI16(); // store

    main.Br("label_start"); // continue

    main.SetLabel("label_end"); // end

    main.LocAddress("strPtr"); // strPtr = alloc((100 - 35 + 1) * sizeof(char))
    main.LoadI64();
    main.PushI64((100 - 35) * sizeof(char));
    main.AddI64();
    main.PushI64('\0');
    main.StoreI16();


    main.LocAddress("strPtr");
    main.LoadI64();
    main.Ret(); // return


    vlModule.ImageFactories.Add(main);
    var translator = new VlTranslator(vlModule);
    var asm = translator.Translate();

    AsmExecutor.PrintCode(asm);
    Console.WriteLine();

    var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    var value = ((delegate*<long>)AsmExecutor.MakeFunction(asm))();
    Console.WriteLine($"Execution time: {DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime}");
    Console.WriteLine($"Res: {value}; {BitConverter.Int64BitsToDouble(value)}; {Marshal.PtrToStringAuto((nint)value)}");
}