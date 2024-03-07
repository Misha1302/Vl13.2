namespace Vl13._2.Parser;

using System.Globalization;
using System.Reflection;

public static class TextToType
{
    public static long ToInt(string s) => int.Parse(s.Replace("_", ""));

    public static double ToDouble(string getText) =>
        double.Parse(getText.Replace("_", ""), NumberStyles.Any, CultureInfo.InvariantCulture);

    public static string TypeToShortString(IEnumerable<ParameterInfo> parameters)
    {
        return string.Join("", parameters.Select(x =>
            x.ParameterType == typeof(long) ? "i64" :
            x.ParameterType == typeof(int) ? "i32" :
            x.ParameterType == typeof(double) ? "f64" :
            x.ParameterType == typeof(bool) ? "i8" :
            Thrower.Throw<string>(new InvalidOperationException("Unknown type"))
        ));
        ;
    }
}