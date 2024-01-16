Imports Serilog
Imports Serilog.Core

Namespace Logging
    Class SerilogLogging
        ' RSPEC-4792: https://jira.sonarsource.com/browse/RSPEC-4792
        Private Sub Foo()
            Dim config As Serilog.LoggerConfiguration = New Serilog.LoggerConfiguration()
'                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}
        End Sub

        Private Sub AdditionalTests()

            Dim config = New LoggerConfiguration()  ' Noncompliant
            config = New MyConfiguration()          ' Noncompliant

            ' Using the logger shouldn't raise issues
            Dim levelSwitch = New LoggingLevelSwitch()
            levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Warning


            Dim newLog = config.
                MinimumLevel.ControlledBy(levelSwitch) _
                .WriteTo.Console() _
                .CreateLogger()

            Log.Logger = newLog
            Log.Information("logged info")
            Log.CloseAndFlush()
        End Sub
    End Class

    Friend Class MyConfiguration
        Inherits LoggerConfiguration

    End Class
End Namespace
