﻿namespace Vl13._2;

using Iced.Intel;

public class VlModule
{
    public LabelsManager LabelsManager = null!;
    public Assembler Assembler = null!;
    public StackManager StackManager = null!;
    public IDebugData DebugData = null!;
    public List<VlImageInfo> Images = null!;
    public TranslateData TranslateData = null!;
    public VlFunction CurrentFunction = null!;

    public void Translate(VlImageInfo image)
    {
        (CurrentFunction = new VlFunction(image, this)).Translate();
    }
}