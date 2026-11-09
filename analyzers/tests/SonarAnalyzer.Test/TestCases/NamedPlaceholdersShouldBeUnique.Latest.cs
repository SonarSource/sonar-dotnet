using Microsoft.Extensions.Logging;

namespace CSharp15
{
    public class ParamsCollectionExpression
    {
        public void Method(ILogger logger)
        {
            logger.LogInformation("{Name} {Name}", [1, 2]);    // Noncompliant
            //                                      Secondary @-1
            logger.LogInformation("{Name} {Other}", [1, 2]);
        }
    }
}
