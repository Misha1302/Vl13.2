namespace Vl13._2;

public class LocalsManager
{
    private int _address;

    private readonly GetOrAddCollection<int> _col;

    public LocalsManager()
    {
        _col = new GetOrAddCollection<int>(GetNextAddress);
    }

    private int GetNextAddress(string name)
    {
        _address += 8;
        return _address;
    }

    public int GetOrAddLocal(string name) => _col.GetOrAdd(name);
}