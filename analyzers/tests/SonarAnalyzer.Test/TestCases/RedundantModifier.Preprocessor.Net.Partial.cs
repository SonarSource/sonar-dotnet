using System;
using System.Text.RegularExpressions;

namespace Repro_1129
{
    public static partial class TargetDependent
    {
        private static partial Regex GetPattern() { return null; }
    }
}
