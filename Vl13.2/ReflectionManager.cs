namespace Vl13._2;

using System.Reflection;
using System.Runtime.CompilerServices;

public static class ReflectionManager
{
    private static readonly Dictionary<int, (MethodInfo mi, nint ptr)> _dict = new();

    public static ulong GetPtr(Type type, string methodName, Type[]? parameters = null) =>
        (ulong)Get(type, methodName, parameters).ptr;

    public static MethodInfo GetMethodInfo(Type type, string methodName, Type[]? parameters = null) =>
        Get(type, methodName, parameters).mi;

    public static (MethodInfo mi, nint ptr) Get(Type type, string methodName, Type[]? parameters = null)
    {
        parameters ??= [];

        if (_dict.TryGetValue(HashOfMethod(type, methodName, parameters), out var res))
            return res;

        var methodInfos = type.GetMethods().Where(x => x.Name == methodName).ToArray();

        var mi = methodInfos.Length != 1
            ? methodInfos.FirstOrDefault(x => ParamsEq(x.GetParameters(), parameters))
            : methodInfos.First();

        if (mi == null)
            return Thrower.Throw<(MethodInfo mi, nint ptr)>(
                new InvalidOperationException($"Cannot found {methodName} in {type}"));

        RuntimeHelpers.PrepareMethod(mi.MethodHandle);

        var value = (mi, mi.MethodHandle.GetFunctionPointer());
        _dict.Add(HashCode.Combine(type, methodName, parameters), value);
        return value;
    }

    private static int HashOfMethod(Type type, string methodName, Type[] parameters) =>
        HashCode.Combine(type, methodName, parameters);

    private static bool ParamsEq(IReadOnlyList<ParameterInfo> parameters1, IReadOnlyList<Type> parameters2)
    {
        if (parameters1.Count != parameters2.Count)
            return false;

        for (var i = 0; i < parameters1.Count; i++)
            if (parameters1[i].ParameterType != parameters2[i])
                return false;

        return true;
    }
}