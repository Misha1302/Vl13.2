namespace Vl13._2;

public static class Thrower
{
    public static T Throw<T>(Exception ex) => throw ex;
    public static void Throw(Exception ex) => Throw<object>(ex);
}