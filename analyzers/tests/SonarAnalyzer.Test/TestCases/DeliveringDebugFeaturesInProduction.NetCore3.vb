Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.Extensions.Hosting

Namespace Tests.Diagnostics

    Public Class Startup

        Public Shared DefaultBuilder As IApplicationBuilder
        Public Builder As IApplicationBuilder = DefaultBuilder.UseDeveloperExceptionPage() ' Noncompliant

        Public Sub Configure(ByVal app As IApplicationBuilder, ByVal env As IWebHostEnvironment)
            ' Invoking as extension methods
            If env.IsDevelopment() Then
                app.UseDeveloperExceptionPage() ' Compliant
            End If

            ' Invoking as static methods
            If HostEnvironmentEnvExtensions.IsDevelopment(env) Then
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Compliant
            End If

            ' Not in development
            If Not env.IsDevelopment() Then
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Noncompliant
            End If

            ' Custom conditions are deliberately ignored
            Dim isDevelopment = env.IsDevelopment()
            If isDevelopment Then
                app.UseDeveloperExceptionPage() ' Noncompliant, False Positive
            End If

            ' Only simple IF checks are considered
            While env.IsDevelopment
                app.UseDeveloperExceptionPage   ' Noncompliant FP
                Exit While
            End While

            ' These are called unconditionally
            app.UseDeveloperExceptionPage() ' Noncompliant
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Noncompliant

            ' VB-specific syntax
            If env.IsDevelopment() Then app.UseDeveloperExceptionPage() ' Compliant
            If Not env.IsDevelopment() Then app.UseDeveloperExceptionPage() ' Noncompliant
        End Sub

        Public Shared Sub ConfigureNotProduction(ByVal app As IApplicationBuilder, ByVal env As IWebHostEnvironment)
            ' https://sonarsource.atlassian.net/browse/NET-4818
            If Not env.IsProduction() Then
                app.UseDeveloperExceptionPage() ' Compliant
                Dim configure As System.Action = Sub() app.UseDeveloperExceptionPage() ' Compliant
                configure()
            Else
                app.UseDeveloperExceptionPage() ' Noncompliant
            End If

            If Not HostEnvironmentEnvExtensions.IsProduction(env) Then
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app) ' Compliant
            End If

            ' VB-specific syntax
            If Not env.IsProduction() Then app.UseDeveloperExceptionPage() ' Compliant
            If Not env.IsProduction Then app.UseDeveloperExceptionPage() ' Compliant, parentheses are optional

            If env.IsProduction() Then
                ' Production-only setup
            Else
                app.UseDeveloperExceptionPage() ' Compliant
            End If

            If env.IsProduction() Then
                app.UseExceptionHandler("/Error")
            ElseIf env.EnvironmentName = "Staging" Then
                app.UseDeveloperExceptionPage() ' Compliant
            End If

            If env.IsProduction() Then
                app.UseDeveloperExceptionPage() ' Noncompliant
            End If

            If env.IsProduction() Then app.UseExceptionHandler("/Error") Else app.UseDeveloperExceptionPage() ' Compliant
            If Not IsProduction(env) Then app.UseDeveloperExceptionPage() ' Noncompliant: unrelated method
        End Sub

        Public Shared Sub ConfigureElseIf(ByVal app As IApplicationBuilder, ByVal env As IWebHostEnvironment)
            If env.EnvironmentName = "Staging" Then
                ' Staging-only setup
            ElseIf Not env.IsProduction() Then
                app.UseDeveloperExceptionPage() ' Compliant
            Else
                app.UseDeveloperExceptionPage() ' Noncompliant
            End If
        End Sub

        Private Shared Function IsProduction(env As IWebHostEnvironment) As Boolean
            Return env.EnvironmentName = "Production"
        End Function

    End Class

    Public Class StartupDevelopment

        Public Sub Configure(ByVal app As IApplicationBuilder, ByVal env As IWebHostEnvironment)
            ' See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-5.0#startup-class-conventions
            app.UseDeveloperExceptionPage() ' Compliant, it is inside StartupDevelopment class
        End Sub

    End Class

End Namespace
