using Microsoft.Extensions.Logging;
using System;

using AliasedFactory = Microsoft.Extensions.Logging.ILoggerFactory;

class AnotherType : EnclosingType { }

class EnclosingType
{
    void Strings(ILoggerFactory factory, string loggerName)
    {
        factory.CreateLogger("Whatever");                               // Compliant, ignore strings apart from nameof and typeof
        factory.CreateLogger(loggerName);                               // Compliant
        factory.CreateLogger(GetLoggerName());                          // Compliant

        factory.CreateLogger(GetType().FullName);                       // Compliant
        factory.CreateLogger(new AnotherType().GetType().FullName);     // FN, we do not parse this

        factory.CreateLogger(nameof(EnclosingType));                    // Compliant
        factory.CreateLogger(nameof(AnotherType));                      // Noncompliant {{Update this logger to use its enclosing type.}}
        //                          ^^^^^^^^^^^

        factory.CreateLogger(typeof(EnclosingType).Name);               // Compliant
        factory.CreateLogger(typeof(AnotherType).Name);                 // Noncompliant

        factory.CreateLogger(typeof(EnclosingType).FullName);           // Compliant
        factory.CreateLogger(typeof(AnotherType).FullName);             // Noncompliant
    }

    void Types(ILoggerFactory factory)
    {
        // runtime
        factory.CreateLogger(GetType());                                // Compliant
        factory.CreateLogger(new AnotherType().GetType());              // FN, we do not parse this
        factory.CreateLogger(GetLoggerType());                          // FN, we do not parse this

        // compile time, typeof
        factory.CreateLogger(typeof(EnclosingType));                   // Compliant
        factory.CreateLogger(typeof(AnotherType));                     // Noncompliant
        //                          ^^^^^^^^^^^

        // compile time, generics
        factory.CreateLogger<EnclosingType>();                         // Compliant
        factory.CreateLogger<AnotherType>();                           // Noncompliant
        //                   ^^^^^^^^^^^

        LoggerFactoryExtensions.CreateLogger<AnotherType>(factory);    // Noncompliant
        //                                   ^^^^^^^^^^^
    }

    void Aliasing(AliasedFactory factory)
    {
        factory.CreateLogger(nameof(EnclosingType));                   // Compliant
        factory.CreateLogger(nameof(AnotherType));                     // Noncompliant
        //                          ^^^^^^^^^^^
    }

    static string GetLoggerName() => "Whatever";
    static Type GetLoggerType() => null;

    class Inner
    {
        void InnerMethod(ILoggerFactory factory)
        {
            factory.CreateLogger(nameof(Inner));                                         // Compliant
            factory.CreateLogger(nameof(AnotherType));                                   // Noncompliant

            factory.CreateLogger(nameof(EnclosingType.Inner));                           // Compliant
            factory.CreateLogger(nameof(AnotherType.Inner));                             // Compliant, `Inner` is the same type
            factory.CreateLogger(typeof(AnotherType.Inner));                             // Compliant
            factory.CreateLogger(typeof(AnotherType.Inner).Name);                        // Compliant
            factory.CreateLogger(typeof(AnotherType.Inner).FullName);                    // Compliant
            factory.CreateLogger(typeof(AnotherType.Inner).AssemblyQualifiedName);       // Compliant

            factory.CreateLogger<EnclosingType.Inner>();                                 // Compliant
            factory.CreateLogger<AnotherType.Inner>();                                   // Compliant
            factory.CreateLogger<AnotherType>();                                         // Noncompliant

            LoggerFactoryExtensions.CreateLogger(factory, typeof(EnclosingType.Inner));  // Compliant
            LoggerFactoryExtensions.CreateLogger(factory, typeof(AnotherType));          // FN, we do not go into arguments if their Count is > 1

            LoggerFactoryExtensions.CreateLogger<EnclosingType.Inner>(factory);          // Compliant
            LoggerFactoryExtensions.CreateLogger<AnotherType>(factory);                  // Noncompliant
            //                                   ^^^^^^^^^^^
        }
    }
}

class Generic<T>
{
    public Generic<T> Same;
    public ILoggerFactory factory;

    void Method()
    {
        factory.CreateLogger<Generic<T>>();                                             // Compliant
        factory.CreateLogger(nameof(Generic<T>));                                       // Compliant
        factory.CreateLogger(typeof(Generic<T>));                                       // Compliant
        factory.CreateLogger(typeof(Generic<T>).Name);                                  // Compliant
        factory.CreateLogger(typeof(Generic<T>).FullName);                              // Compliant
        factory.CreateLogger(typeof(Generic<T>).AssemblyQualifiedName);                 // Compliant

        this.Same.factory.CreateLogger<Generic<T>>();                                   // Compliant
        this.Same.Same.Same.factory.CreateLogger<Generic<T>>();                         // Compliant

        this.Same.Same.Same.factory.CreateLogger<AnotherType>();                        // Noncompliant
        this.Same.Same.Same.factory.CreateLogger(nameof(AnotherType));                  // Noncompliant
        this.Same.Same.Same.factory.CreateLogger(typeof(AnotherType));                  // Noncompliant

        this.Chain<T>().Chain<T>().Factory().CreateLogger(nameof(Generic<T>));          // Compliant
        this.Chain<T>().Chain<T>().Factory().CreateLogger(nameof(AnotherType));         // Noncompliant

        this.Chain<T>().Chain<T>().Factory().CreateLogger(typeof(Generic<T>));          // Compliant
        this.Chain<T>().Chain<T>().Factory().CreateLogger(typeof(AnotherType));         // Noncompliant

        this.Chain<T>().Chain<T>().Factory().CreateLogger(typeof(Generic<T>).Name);     // Compliant
        this.Chain<T>().Chain<T>().Factory().CreateLogger(typeof(AnotherType).Name);    // Noncompliant

        this.Chain<T>().Chain<T>().Factory().CreateLogger<Generic<T>>();                // Compliant
        this.Chain<T>().Chain<T>().Factory().CreateLogger<AnotherType>();               // Noncompliant
    }

    Generic<T> Chain<T>() => null;
    ImplFactory Factory() => null;
}

class ImplFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public void Dispose() { }

    public ILogger CreateLogger(string categoryName) => null;
}

class FakeMethods
{
    public ILogger GetLogger(string name) => null;
    public ILogger GetLogger(Type type) => null;

    public ILogger CreateLogger(string name) => null;
    public ILogger CreateLogger(Type type) => null;
    public ILogger CreateLogger<T>() => null;
    public ILogger CreateLogger<T1, T2>() => null;

    string nameof(object a) => null;
    string nameof(object a, object b) => null;

    public void Method(ILoggerFactory factory)
    {
        CreateLogger(typeof(AnotherType));              // Compliant, not relevant method
        CreateLogger(typeof(AnotherType).Name);         // Compliant
        CreateLogger<AnotherType>();                    // Compliant
        CreateLogger<AnotherType, AnotherType>();       // Compliant

        GetLogger(typeof(AnotherType));                 // Compliant, not relevant method
        GetLogger(typeof(AnotherType).Name);            // Compliant

        var FakeType = "FakeType";
        factory.CreateLogger(nameof(FakeType));         // Noncompliant FP, very unrealistic
        factory.CreateLogger(nameof(FakeType, 42));     // Compliant
    }
}

class LoggerHelper
{
    ILoggerFactory factory;

    ILogger<T> CreateLogger<T>() => factory.CreateLogger<T>(); // Compliant, T is GenericType
}

public class Service
{
    public Service(ILogger logger) { }
    public Service(ILogger logger, string otherParameter) { }
    public Service(ILogger<Service> logger) { }
    public Service(ILogger<Service> logger, string otherParameter) { }
    public Service(Service service) { }
    public Service(AnotherService service) { }
}

public class AnotherService
{
    public AnotherService(Service service) { }
    public AnotherService(ILogger<Service> logger) { }
}


public class Factory
{
    private readonly ILogger<Service> logger = LoggerFactory.Create(builder => { }).CreateLogger<Service>();        // Noncompliant

    public void CreateType(ILoggerFactory loggerFactory, string otherParameter)
    {
        new Service(loggerFactory.CreateLogger<Service>());                                                         // Compliant
        new Service(loggerFactory.CreateLogger<AnotherService>());                                                  // Noncompliant

        new Service(loggerFactory.CreateLogger<Service>(), otherParameter);
        new Service(loggerFactory.CreateLogger(nameof(Service)), otherParameter);
        new Service(loggerFactory.CreateLogger(typeof(Service)), otherParameter);
        new Service(loggerFactory.CreateLogger(typeof(Service).Name), otherParameter);
        new Service(loggerFactory.CreateLogger(typeof(Service).FullName), otherParameter);
        new Service(loggerFactory.CreateLogger(typeof(Service).AssemblyQualifiedName), otherParameter);

        new Service(loggerFactory.CreateLogger<string>(), otherParameter);                                           // Noncompliant
        new Service(loggerFactory.CreateLogger(nameof(AnotherService)), otherParameter);                             // Noncompliant
        new Service(loggerFactory.CreateLogger(typeof(AnotherService)), otherParameter);                             // Noncompliant
        new Service(loggerFactory.CreateLogger(typeof(string).Name), otherParameter);                                // Noncompliant
        new Service(loggerFactory.CreateLogger(typeof(string).FullName), otherParameter);                            // Noncompliant
        new Service(loggerFactory.CreateLogger(typeof(Decorator<Service>).AssemblyQualifiedName), otherParameter);   // Noncompliant

        new AnotherService(new Service(loggerFactory.CreateLogger<Service>()));                                      // Compliant
        new Service(new AnotherService(loggerFactory.CreateLogger<Service>()));                                      // Noncompliant
    }

    public Service CreateType_LocalVariable(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Service>();                                                         // Noncompliant FP
        return new Service(logger);
    }

    public Service CreateType_LocalVariableField()
    {
        return new Service(logger);
    }

    public Service CreateType_Decorator(ILoggerFactory loggerFactory)
    {
        return new Service(new Decorator<Service>(loggerFactory.CreateLogger<Service>()));                          // Compliant
        return new Service(new Decorator(loggerFactory.CreateLogger(nameof(Service))));                             // Compliant
    }

    class Decorator : ILogger
    {
        public Decorator(ILogger logger) { }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => null;
    }

    private class Decorator<T> : ILogger<T>
    {
        public Decorator(ILogger<T> logger) { }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => null;
    }
}
