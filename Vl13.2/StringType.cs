namespace Vl13._2;

public record StringType(string Type)
{
    public readonly string Type = Type.ToUpper();
    public bool IsByRef => Type.StartsWith('&');

    public virtual bool Equals(StringType? other) =>
        Type == other?.Type;

    public override int GetHashCode() => Type.GetHashCode();
}