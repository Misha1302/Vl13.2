global using Mli = Vl13._2.ModuleLocalInfo;

namespace Vl13._2;

public record ModuleLocalInfo(string Type, string Name, bool IsByRef = false);