namespace Vl13._2;

public class GetOrAddCollection<T>(Func<string, T> valueCreator)
{
    private readonly Dictionary<string, T> _items = new();
    public IReadOnlyDictionary<string, T> Items => _items;

    public T GetOrAdd(string name)
    {
        if (!_items.ContainsKey(name)) // no need to call func if key contains
            _items.Add(name, valueCreator(name));

        return _items[name];
    }
}