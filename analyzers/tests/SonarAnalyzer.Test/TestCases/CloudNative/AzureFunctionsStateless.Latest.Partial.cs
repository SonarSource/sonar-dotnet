using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

public static partial class StaticPartialClass
{
    public static partial int PartialProperty { get { return 42; } set { } }

    public static partial void Update(int value) =>
        PartialProperty = value; //Noncompliant
}

