namespace Vl13._2;

/// <summary>
/// </summary>
/// <param name="StackMaxSizeIn64">
///     The size of the stack allocated to the entire program. The number of qwords (8 bytes) is indicated.
/// </param>
/// <param name="CheckStackOverflow">
///     Checks that stack has gone out of bound. It's enough very heavy, but very useful for debug.
/// </param>
public record TranslateData(int StackMaxSizeIn64, bool CheckStackOverflow);