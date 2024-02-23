namespace Vl13._2;

using System.Runtime.InteropServices;
using Math = Math;

public static class VlRuntimeHelper
{
    public static double RemF64(double a, double b) =>
        (Math.Abs(a) - Math.Abs(b) * Math.Floor(Math.Abs(a) / Math.Abs(b))) * Math.Sign(a);

    public static long NeqI(long a, long b) => EqI(a, b) == 1 ? 0 : 1;
    public static long NeqF(float a, float b) => EqF(a, b) == 1 ? 0 : 1;

    public static long EqI(long a, long b) => a == b ? 1 : 0;
    public static long EqF(float a, float b) => AreEqualRel(a, b, 0.0001) ? 1 : 0;

    public static long LtI(long a, long b) => a < b ? 1 : 0;
    public static long LtF(double a, double b) => a < b ? 1 : 0;

    private static bool AreEqualRel(double a, double b, double epsilon) =>
        Math.Abs(a - b) <= epsilon * Math.Max(Math.Abs(a), Math.Abs(b));

    public static long Alloc(int bytes)
    {
        var ptr = Marshal.AllocCoTaskMem(bytes);

        for (var i = 0; i < bytes; i++)
            Marshal.WriteByte(ptr, i, 0);

        return ptr;
    }

    public static int WriteNumbers(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10,
        int a11)
    {
        Console.WriteLine($"{a1}, {a2}, {a3}, {a4}, {a5}, {a6}, {a7}, {a8}, {a9}, {a10}, {a11}");
        return 1;
    }
}