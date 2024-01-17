Imports System
Imports System.IO
Imports System.Xml
Imports log4net.Appender
Imports log4net.Repository

Namespace Logging
    Class Log4netLogging
        Private Sub Foo(ByVal repository As ILoggerRepository, ByVal element As XmlElement, ByVal configFile As FileInfo, ByVal configUri As Uri, ByVal configStream As Stream, ByVal appender As IAppender, ParamArray appenders As IAppender())
            log4net.Config.XmlConfigurator.Configure(repository)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}
            log4net.Config.XmlConfigurator.Configure(repository, element) ' Noncompliant
            log4net.Config.XmlConfigurator.Configure(repository, configFile) ' Noncompliant
            log4net.Config.XmlConfigurator.Configure(repository, configUri) ' Noncompliant
            log4net.Config.XmlConfigurator.Configure(repository, configStream) ' Noncompliant
            log4net.Config.XmlConfigurator.ConfigureAndWatch(repository, configFile)
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}

            log4net.Config.DOMConfigurator.Configure() ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository) ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(element) ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository, element) ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(configFile) ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository, configFile) ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(configStream) ' Noncompliant
            log4net.Config.DOMConfigurator.Configure(repository, configStream) ' Noncompliant
            log4net.Config.DOMConfigurator.ConfigureAndWatch(configFile) ' Noncompliant
            log4net.Config.DOMConfigurator.ConfigureAndWatch(repository, configFile) ' Noncompliant
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}

            log4net.Config.BasicConfigurator.Configure() ' Noncompliant
            log4net.Config.BasicConfigurator.Configure(appender) ' Noncompliant
            log4net.Config.BasicConfigurator.Configure(appenders) ' Noncompliant
            log4net.Config.BasicConfigurator.Configure(repository) ' Noncompliant
            log4net.Config.BasicConfigurator.Configure(repository, appender) ' Noncompliant
            log4net.Config.BasicConfigurator.Configure(repository, appenders) ' Noncompliant
        End Sub
    End Class
End Namespace
