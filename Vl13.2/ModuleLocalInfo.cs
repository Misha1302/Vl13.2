﻿global using Mli = Vl13._2.ModuleLocalInfo;

namespace Vl13._2;

public record ModuleLocalInfo(StringType Type, string Name, bool IsByRef = false)
{
    public readonly bool IsByRef = IsByRef || Type.Type.StartsWith('&');
}