using System;
using Microsoft.Extensions.Logging;

// https://github.com/SonarSource/sonar-dotnet/issues/9545
public class Repro_9545
{
    public void Method(ILogger logger, int number)
    {
        logger.LogDebug($@"{nameof(Repro_9545)} filter: {{number}}", number);       // Compliant - FN
        logger.LogDebug(@$"{nameof(Repro_9545)} filter: {{number}}", number);       // Compliant - FN
        logger.LogDebug($$"""{{nameof(Repro_9545)}} filter: {number}""", number);   // Compliant - FN
        logger.LogDebug($$$"""
                           {{{nameof(Repro_9545)}}} filter: {number}
                           """, number);                                            // Compliant - FN
    }
}
