namespace Vl13._2;

using System.Reflection;
using System.Runtime.CompilerServices;

public static class ReflectionManager
{
    public static readonly Dictionary<string, (MethodInfo mi, nint ptr)> Methods = new();

    public static ulong GetPtr(string alias, Type type, string methodName, Type[]? parameters = null) =>
        (ulong)Get(alias, type, methodName, parameters).ptr;

    public static ulong GetPtr(Type type, string methodName, Type[]? parameters = null) =>
        GetPtr(MakeAlias(type, methodName, parameters), type, methodName, parameters);

    private static string MakeAlias(Type type, string methodName, Type[]? parameters) =>
        HashCode.Combine(type, methodName, parameters).ToString();

    public static MethodInfo GetMethodInfo(string alias, Type type, string methodName, Type[]? parameters = null) =>
        Get(alias, type, methodName, parameters).mi;

    public static (MethodInfo mi, nint ptr) Get(Type type, string methName, Type[]? parameters = null) =>
        Get(MakeAlias(type, methName, parameters), type, methName, parameters);

    public static (MethodInfo mi, nint ptr) Get(string alias, Type type, string methName, Type[]? parameters = null) =>
        Methods.TryGetValue(alias, out var res)
            ? res
            : Add(alias, FindMethod(type, methName, parameters));


    public static void AddAllMethods(Assembly assembly, Func<Type, MethodInfo, string> getAlias,
        Predicate<MethodInfo> include)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
            foreach (var mi in type.GetMethods())
                if (include(mi))
                    Get(getAlias(type, mi), type, mi.Name, mi.GetParameters().Select(x => x.ParameterType).ToArray());
    }


    private static MethodInfo FindMethod(Type type, string methodName, IReadOnlyList<Type>? parameters)
    {
        var methodInfos = type.GetMethods().Where(x => x.Name == methodName).ToArray();

        var mi = methodInfos.Length != 1
            ? methodInfos.FirstOrDefault(x => ParamsEq(x.GetParameters(), parameters))
            : methodInfos.First();

        return mi != null
            ? mi
            : Thrower.Throw<MethodInfo>(new InvalidOperationException($"Cannot found {methodName} in {type}"));
    }

    private static (MethodInfo mi, nint ptr) Add(string alias, MethodInfo mi)
    {
        RuntimeHelpers.PrepareMethod(mi.MethodHandle);

        var value = (mi, mi.MethodHandle.GetFunctionPointer());
        Methods.Add(alias, value);
        return value;
    }

    private static bool ParamsEq(IReadOnlyList<ParameterInfo> parameters1, IReadOnlyList<Type>? parameters2)
    {
        parameters2 ??= [];

        if (parameters1.Count != parameters2.Count)
            return false;

        for (var i = 0; i < parameters1.Count; i++)
            if (parameters1[i].ParameterType != parameters2[i])
                return false;

        return true;
    }
}