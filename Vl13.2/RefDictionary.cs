namespace Vl13._2;

public class RefDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
    public RefDictionary(IEnumerable<KeyValuePair<TKey, TValue>> d)
    {
        foreach (var pair in d)
            Add(pair.Key, pair.Value);
    }

    public new ref TValue this[TKey index] =>
        ref new[] { base[index] }[0];
}