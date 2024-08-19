using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S3416
    {
        public void Method(ILoggerFactory factory)
        {
            factory.CreateLogger(nameof(AnotherType));
            factory.CreateLogger(typeof(AnotherType));
            factory.CreateLogger(typeof(AnotherType).Name);
            factory.CreateLogger(typeof(AnotherType).FullName);
            factory.CreateLogger(typeof(AnotherType).AssemblyQualifiedName);
            factory.CreateLogger<AnotherType>();
        }
    }

    public class AnotherType { }
}
