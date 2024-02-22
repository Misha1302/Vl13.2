namespace Vl13._2;

public record Op(OpType OpType, params object[]? Params)
{
    public T Arg<T>(int index) =>
        (T)(Params ?? Thrower.Throw<object[]>(new IndexOutOfRangeException()))[index];
}