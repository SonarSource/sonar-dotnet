﻿using System;
using System.IO;

namespace CSharpLatest.CSharp9Features.S2952;

public partial class ResourceHolder2 : IDisposable
{
    private FileStream fs;

    public void OpenResource(string path) =>
        this.fs = new FileStream(path, FileMode.Open);

    public partial void CleanUp();

    public partial void Dispose();
}
