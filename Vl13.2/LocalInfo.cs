namespace Vl13._2;

public record LocalInfo(AsmType Type, string Name, bool IsByRef)
{
    public virtual bool Equals(LocalInfo? other)
    {
        if (Name != other?.Name)
            return false;

        if (Type != other.Type)
            Thrower.Throw(new InvalidOperationException());

        return true;
    }

    public override int GetHashCode() => HashCode.Combine((int)Type, Name);
}