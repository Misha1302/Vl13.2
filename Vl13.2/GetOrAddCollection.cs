namespace Vl13._2;

public class GetOrAddCollection<T>(Func<string, T> valueCreator)
{
    private readonly Dictionary<string, T> _labels = new();

    public ref T GetOrAdd(string name)
    {
        if (!_labels.ContainsKey(name)) // no need to call func if key contains
            _labels.Add(name, valueCreator(name));

        return ref new[] { _labels[name] }[0];
    }
}