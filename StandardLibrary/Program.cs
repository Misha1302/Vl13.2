// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global

#pragma warning disable CA1050

using StandardLibrary;

// We don't need a namespace, so the method call will be a bit shorter
public static class Std
{
    public static void WriteLine(long i) => Console.WriteLine(i);
    public static void WriteLine(int i) => Console.WriteLine(i);
    public static void WriteLine(double i) => Console.WriteLine(i);
    public static void WriteLine(bool i) => Console.WriteLine(i);
    public static void WriteLine(char i) => Console.WriteLine(i);

    public static void Write(long i) => Console.Write(i);
    public static void Write(int i) => Console.Write(i);
    public static void Write(double i) => Console.Write(i);
    public static void Write(bool i) => Console.Write(i);
    public static void Write(char i) => Console.Write(i);

    public static int WriteNumbers11(
        int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10, int a11)
    {
        Console.WriteLine($"{a1}, {a2}, {a3}, {a4}, {a5}, {a6}, {a7}, {a8}, {a9}, {a10}, {a11}");
        return 1;
    }

    public static int WriteNumbers10(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10)
    {
        Console.WriteLine($"{a1}, {a2}, {a3}, {a4}, {a5}, {a6}, {a7}, {a8}, {a9}, {a10}");
        return 1;
    }

    public static long Calloc(int bytes) => MemoryManager.Calloc(bytes);

    public static void Free(long ptr) => MemoryManager.Free(ptr);

    public static long RndInt(long a, long b) =>
        Random.Shared.NextInt64(a, b);

    public static long Time() => DateTimeOffset.Now.ToUnixTimeMilliseconds();
}