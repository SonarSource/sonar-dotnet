using log4net.Appender;
using log4net.Repository;
using System;
using System.IO;
using System.Xml;

namespace Logging
{
    class Log4netLogging
    {
        void Foo(ILoggerRepository repository, XmlElement element, FileInfo configFile, Uri configUri, Stream configStream,
        IAppender appender, params IAppender[] appenders)
        {
            log4net.Config.XmlConfigurator.Configure(repository);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}

            log4net.Config.XmlConfigurator.Configure(repository, element); // Noncompliant
            log4net.Config.XmlConfigurator.Configure(repository, configFile); // Noncompliant
            log4net.Config.XmlConfigurator.Configure(repository, configUri); // Noncompliant
            log4net.Config.XmlConfigurator.Configure(repository, configStream); // Noncompliant
            log4net.Config.XmlConfigurator.ConfigureAndWatch(repository, configFile); // Noncompliant

            log4net.Config.DOMConfigurator.Configure(); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(element); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository, element); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(configFile); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository, configFile); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(configStream); // Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository, configStream); // Noncompliant
            log4net.Config.DOMConfigurator.ConfigureAndWatch(configFile); // Noncompliant
            log4net.Config.DOMConfigurator.ConfigureAndWatch(repository, configFile); // Noncompliant

            log4net.Config.BasicConfigurator.Configure(); // Noncompliant
            log4net.Config.BasicConfigurator.Configure(appender); // Noncompliant
            log4net.Config.BasicConfigurator.Configure(appenders); // Noncompliant
            log4net.Config.BasicConfigurator.Configure(repository); // Noncompliant
            log4net.Config.BasicConfigurator.Configure(repository, appender); // Noncompliant
            log4net.Config.BasicConfigurator.Configure(repository, appenders);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}
        }
    }
}
