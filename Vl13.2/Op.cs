namespace Vl13._2;

public record Op(OpType OpType, params object[]? Params)
{
    public T Arg<T>(int index) =>
        (T)(Params ?? Thrower.Throw<object[]>(new IndexOutOfRangeException()))[index];

    public override string ToString() =>
        $"{OpType}" + (Params is not null ? $": [{string.Join(", ", Params ?? Array.Empty<object>())}]" : "");
}