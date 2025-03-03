using System;
using System.Text.RegularExpressions;

namespace Repro_1129
{
    public static partial class TargetDependent
    {
        public static bool Matches(string ns) => Pattern.IsMatch(ns);

#if NETFRAMEWORK
        private static readonly Regex Pattern = new(@"\.v(?<Version>[1-9][0-9]+(_[0-9]+)*)\.", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
#else
        private static readonly Regex Pattern = GetPattern();

        [GeneratedRegex(@"\.v(?<Version>[1-9][0-9]+(_[0-9]+)*)\.", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant)]
        private static partial Regex GetPattern();
#endif
    }
}
