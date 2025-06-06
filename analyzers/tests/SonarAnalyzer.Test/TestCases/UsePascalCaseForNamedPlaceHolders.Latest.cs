using System;
using Microsoft.Extensions.Logging;

// https://github.com/SonarSource/sonar-dotnet/issues/9545
public class Repro_9545
{
    public void Method(ILogger logger, int number)
    {
        logger.LogDebug("""Repro_9545 filter: {number}""", number);
        //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //                                     ^^^^^^                                 Secondary @-1
        logger.LogDebug("""
            Repro_9545 filter: {number}
            """, number);                                                          // Noncompliant @-2 ^25#59
                                                                                   // Secondary @-2 ^33#6
        logger.LogDebug($@"{nameof(Repro_9545)} filter: {{number}}", number);
        //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //                                                ^^^^^^                      Secondary @-1
        logger.LogDebug(@$"{nameof(Repro_9545)} filter: {{number}}", number);
        //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //                                                ^^^^^^                      Secondary @-1
        logger.LogDebug($$"""{{nameof(Repro_9545)}} filter: {number}""", number);
        //              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //                                                   ^^^^^^                   Secondary @-1
        logger.LogDebug($$$"""
                           {{{nameof(Repro_9545)}}} filter: {number}
                           """, number);                                            // Noncompliant @-2 ^25#106
                                                                                    // Secondary @-2 ^62#6
        logger.LogDebug($$$"""
                           {{{nameof(
    Repro_9545)}}} filter: {number}
                           """, number);                                            // Noncompliant @-3 ^25#111
                                                                                    // Secondary @-2 ^29#6
    }
}
