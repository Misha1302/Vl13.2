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
    var vlFunction = new VlImageInfo();


    vlFunction.LocAddress("i"); // i = 0
    vlFunction.PushI64(0);
    vlFunction.StoreI64();


    vlFunction.SetLabel("label_start"); // while

    // i != 100_000
    vlFunction.LocAddress("i");
    vlFunction.LoadI64();
    vlFunction.PushI64(1_000);
    vlFunction.Neq();
    vlFunction.BrZero("label_end"); // if false

    vlFunction.LocAddress("i");
    vlFunction.LoadI64();
    vlFunction.CallSharp(typeof(Console), nameof(Console.WriteLine), [typeof(long)]);
    vlFunction.Drop();

    // i = i + 1
    vlFunction.LocAddress("i"); // first arg for store
    vlFunction.LocAddress("i"); // arg for load
    vlFunction.LoadI64(); // first arg for add
    vlFunction.PushI64(1); // second arg for add
    vlFunction.AddI64(); // add
    vlFunction.StoreI64(); // store

    vlFunction.Br("label_start"); // continue

    vlFunction.SetLabel("label_end"); // end
    vlFunction.Ret(); // return


    vlModule.ImageFactories.Add(vlFunction);
    var translator = new VlTranslator(vlModule);
    var asm = translator.Translate();

    AsmExecutor.PrintCode(asm);
    Console.WriteLine();

    var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    var value = ((delegate*<long>)AsmExecutor.MakeFunction(asm))();
    Console.WriteLine(DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime);
    Console.WriteLine($"res: {value}; {BitConverter.Int64BitsToDouble(value)}");
}