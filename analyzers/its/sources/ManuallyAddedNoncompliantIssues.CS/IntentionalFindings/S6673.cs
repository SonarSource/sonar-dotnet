using System;
using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S6673
    {
        private void Log(ILogger logger, int arg, int anotherArg) =>
            logger.LogInformation("Arg: {Arg} {AnotherArg}", anotherArg, arg); // Noncompliant (S6673) {{Template placeholders should be in the right order: placeholder 'Arg' does not match with argument 'anotherArg'.}}
    }
}
