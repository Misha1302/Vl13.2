namespace Vl13._2.Parser;

using System.Globalization;

public static class TextToType
{
    public static long ToInt(string s) => int.Parse(s);

    public static double ToDouble(string getText) =>
        double.Parse(getText, NumberStyles.Any, CultureInfo.InvariantCulture);
}