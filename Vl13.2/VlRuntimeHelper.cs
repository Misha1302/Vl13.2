namespace Vl13._2;

using System.Runtime.InteropServices;

public static class VlRuntimeHelper
{
    private static readonly Stack<(long address, long rsp, long rbp)> _stack = new();

    public static double RemF64(double a, double b) =>
        (Math.Abs(a) - Math.Abs(b) * Math.Floor(Math.Abs(a) / Math.Abs(b))) * Math.Sign(a);

    public static long NeqI(long a, long b) => EqI(a, b) == 1 ? 0 : 1;
    public static long NeqF(double a, double b) => EqF(a, b) == 1 ? 0 : 1;

    public static long EqI(long a, long b) => a == b ? 1 : 0;
    public static long EqF(double a, double b) => AreEqualRel(a, b, 0.0001) ? 1 : 0;

    public static long LtI(long a, long b) => a < b ? 1 : 0;
    public static long LtF(double a, double b) => a < b ? 1 : 0;

    public static long GtI(long a, long b) => a > b ? 1 : 0;
    public static long GtF(double a, double b) => a > b ? 1 : 0;

    public static long GeI(long a, long b) => a >= b ? 1 : 0;
    public static long GeF(double a, double b) => a >= b ? 1 : 0;

    public static long LeI(long a, long b) => a < b ? 1 : 0;
    public static long LeF(double a, double b) => a < b ? 1 : 0;

    private static bool AreEqualRel(double a, double b, double epsilon) =>
        Math.Abs(a - b) <= epsilon * Math.Max(Math.Abs(a), Math.Abs(b));

    public static long Alloc(int bytes)
    {
        var ptr = Marshal.AllocCoTaskMem(bytes);

        for (var i = 0; i < bytes; i++)
            Marshal.WriteByte(ptr, i, 0);

        return ptr;
    }

    public static void Free(long ptr) =>
        Marshal.FreeCoTaskMem((nint)ptr);

    public static int WriteNumbers(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10,
        int a11)
    {
        Console.WriteLine($"{a1}, {a2}, {a3}, {a4}, {a5}, {a6}, {a7}, {a8}, {a9}, {a10}, {a11}");
        return 1;
    }

    public static void StackOverflow(long value) =>
        Thrower.Throw(new StackOverflowException($"Index was {value}"));

    public static long RndInt(long a, long b) =>
        Random.Shared.NextInt64(a, b);

    public static long I8ToI64(sbyte value) => value;
    public static long I16ToI64(short value) => value;
    public static long I32ToI64(int value) => value;
    public static sbyte I64ToI8(long value) => (sbyte)value;
    public static short I64ToI16(long value) => (short)value;
    public static int I64ToI32(long value) => (int)value;
    public static double I64ToF64(long value) => value;
    public static long F64ToI64(double value) => (long)(value + 0.0001);

    public static void PushAddress(long address, long rsp, long rbp) => _stack.Push((address, rsp, rbp));

    public static unsafe void PopAddress(long addressPtr, long rspPtr, long rbpPtr) =>
        (*(long*)(void*)addressPtr, *(long*)(void*)rspPtr, *(long*)(void*)rbpPtr) = _stack.Pop();

    public static void DropAddress() => _stack.Pop();

    public static long Time() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
}