namespace Vl13._2.Parser;

using System.Globalization;
using System.Reflection;

public static class TextToType
{
    public static readonly HashSet<Type> AllowedTypes =
        [typeof(long), typeof(int), typeof(double), typeof(bool), typeof(char)];

    public static readonly Dictionary<Type, string> TypeAsStrings = new()
    {
        [typeof(long)] = "i64",
        [typeof(int)] = "i32",
        [typeof(char)] = "char",
        [typeof(double)] = "f64",
        [typeof(bool)] = "bool"
    };

    public static long ToInt(string s) => int.Parse(s.Replace("_", ""));

    public static double ToDouble(string getText) =>
        double.Parse(getText.Replace("_", ""), NumberStyles.Any, CultureInfo.InvariantCulture);
}