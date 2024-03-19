using System;
using Microsoft.Extensions.Logging;

namespace IntentionalFindings;

public static class S2139
{
    public static void LogAndThrow(ILogger logger)
    {
        try { }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message!");
            throw;
        }
    }
}
