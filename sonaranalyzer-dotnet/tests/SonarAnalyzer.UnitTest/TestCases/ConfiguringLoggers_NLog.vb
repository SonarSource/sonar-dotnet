Imports LogMan = NLog.LogManager
Imports NLog.LogManager

Namespace Logging
    Class NLogLogging
        ' RSPEC-4792: https://jira.sonarsource.com/browse/RSPEC-4792
        Private Sub Foo(ByVal config As NLog.Config.LoggingConfiguration)
            NLog.LogManager.Configuration = config
'           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}
        End Sub


        Private Sub AdditionalTests(config As NLog.Config.LoggingConfiguration)
            ' Call via the alias
            LogMan.Configuration = config
'           ^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}

            ' Call via the import of the static class
            Configuration = config
'           ^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}


            ' Reading the configuration should be ok
            Dim current = Configuration
            current = LogMan.Configuration
            current = NLog.LogManager.Configuration

            AdditionalTests(Configuration)
            AdditionalTests(LogMan.Configuration)
            AdditionalTests(NLog.LogManager.Configuration)
        End Sub
    End Class
End Namespace
