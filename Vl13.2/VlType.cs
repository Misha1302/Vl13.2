namespace Vl13._2;

public record VlType
{
    public readonly StringType MainType;
    public readonly List<VlType> ComplexTypes;

    public VlType(string type)
    {
        ComplexTypes = [];

        var ind = type.IndexOf('[');
        if (ind == -1)
        {
            MainType = new StringType(type);
        }
        else
        {
            MainType = new StringType(type[..ind]);
            // i64[&Vector3, none] -> i64 - main type, &Vector3, none - additional types

            var startIndex = ind;
            while (true)
            {
                var commaInd = type.IndexOf(',', startIndex + 1);
                if (commaInd == -1)
                {
                    commaInd = type.IndexOf(']');
                    ComplexTypes.Add(new VlType(type[(startIndex + 1)..commaInd]));
                    break;
                }

                ComplexTypes.Add(new VlType(type[(startIndex + 1)..commaInd]));
                startIndex = commaInd;
            }
        }
    }

    public VlType(StringType MainType, List<VlType>? ComplexTypes = null)
    {
        this.MainType = MainType;
        this.ComplexTypes = ComplexTypes ?? [];
    }

    public virtual bool Equals(VlType? other)
    {
        if (other == null)
            return false;

        var mainTypeEq = MainType == other.MainType;
        var complexTypesEq = ComplexTypes == other.ComplexTypes || ComplexTypes.SequenceEqual(other.ComplexTypes);
        return mainTypeEq && complexTypesEq;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MainType, ComplexTypes.Aggregate(19, (i, t) => i * 31 + t.GetHashCode()));
    }
}